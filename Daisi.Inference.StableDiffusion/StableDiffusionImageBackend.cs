using Daisi.Inference.Interfaces;
using Daisi.Inference.Models;
using HPPH.SkiaSharp;
using StableDiffusion.NET;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Daisi.Inference.StableDiffusion;

/// <summary>
/// StableDiffusion.NET implementation of IImageGenerationBackend.
/// </summary>
public class StableDiffusionImageBackend : IImageGenerationBackend
{
    public string BackendName => "StableDiffusion.NET";

    public Task ConfigureAsync(BackendConfiguration config)
    {
        return Task.CompletedTask;
    }

    public Task<IModelHandle> LoadModelAsync(ModelLoadRequest request)
    {
        var modelParams = DiffusionModelParameter.Create()
            .WithModelPath(request.FilePath)
            .WithMultithreading()
            .WithFlashAttention();

        DiffusionModel model;
        using (new NativeOutputSuppressor())
        {
            model = new DiffusionModel(modelParams);
        }

        IModelHandle handle = new StableDiffusionModelHandle(request.ModelId, request.FilePath, model);
        return Task.FromResult(handle);
    }

    public void UnloadModel(IModelHandle handle)
    {
        handle.Dispose();
    }

    public Task<ImageGenerationResult> GenerateAsync(IModelHandle handle, ImageGenerationParams parameters, CancellationToken ct = default)
    {
        if (handle is not StableDiffusionModelHandle sdHandle || !sdHandle.IsLoaded)
            throw new InvalidOperationException("Invalid or unloaded model handle.");

        var sw = Stopwatch.StartNew();

        var genParams = ImageGenerationParameter.TextToImage(parameters.Prompt)
            .WithSize(parameters.Width, parameters.Height)
            .WithCfg(parameters.CfgScale)
            .WithSteps(parameters.Steps);

        if (parameters.Seed >= 0)
            genParams = genParams.WithSeed(parameters.Seed);

        if (!string.IsNullOrWhiteSpace(parameters.NegativePrompt))
            genParams = genParams.WithNegativePrompt(parameters.NegativePrompt);

        object? imageResult;
        using (new NativeOutputSuppressor())
        {
            imageResult = sdHandle.Model!.GenerateImage(genParams);
        }
        var image = imageResult as HPPH.IImage;

        sw.Stop();

        byte[] imageData = [];
        if (image is not null)
        {
            imageData = image.ToPng();
        }

        return Task.FromResult(new ImageGenerationResult
        {
            ImageData = imageData,
            Width = parameters.Width,
            Height = parameters.Height,
            Format = "png",
            GenerationTimeMs = sw.ElapsedMilliseconds
        });
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Suppresses native stdout/stderr output by redirecting the OS-level file handles to NUL.
    /// This prevents native libraries (like stable-diffusion.cpp) from writing progress bars
    /// that corrupt the Host TUI.
    /// </summary>
    private sealed class NativeOutputSuppressor : IDisposable
    {
        private const int STD_OUTPUT_HANDLE = -11;
        private const int STD_ERROR_HANDLE = -12;
        private const uint GENERIC_WRITE = 0x40000000;
        private const uint OPEN_EXISTING = 3;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode,
            IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private readonly IntPtr _savedStdout;
        private readonly IntPtr _savedStderr;
        private readonly IntPtr _nullHandle;
        private readonly TextWriter _savedConsoleOut;
        private readonly TextWriter _savedConsoleErr;
        private readonly bool _isWindows;

        public NativeOutputSuppressor()
        {
            _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            _savedConsoleOut = Console.Out;
            _savedConsoleErr = Console.Error;
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);

            if (_isWindows)
            {
                _savedStdout = GetStdHandle(STD_OUTPUT_HANDLE);
                _savedStderr = GetStdHandle(STD_ERROR_HANDLE);
                _nullHandle = CreateFile("NUL", GENERIC_WRITE, 3, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                if (_nullHandle != IntPtr.Zero && _nullHandle != new IntPtr(-1))
                {
                    SetStdHandle(STD_OUTPUT_HANDLE, _nullHandle);
                    SetStdHandle(STD_ERROR_HANDLE, _nullHandle);
                }
            }
        }

        public void Dispose()
        {
            if (_isWindows)
            {
                SetStdHandle(STD_OUTPUT_HANDLE, _savedStdout);
                SetStdHandle(STD_ERROR_HANDLE, _savedStderr);
                if (_nullHandle != IntPtr.Zero && _nullHandle != new IntPtr(-1))
                    CloseHandle(_nullHandle);
            }
            Console.SetOut(_savedConsoleOut);
            Console.SetError(_savedConsoleErr);
        }
    }
}
