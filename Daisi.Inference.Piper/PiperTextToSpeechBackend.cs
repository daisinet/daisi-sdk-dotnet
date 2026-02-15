using Daisi.Inference.Interfaces;
using Daisi.Inference.Models;
using PiperSharp;
using PiperSharp.Models;
using System.Diagnostics;

namespace Daisi.Inference.Piper;

/// <summary>
/// PiperSharp implementation of ITextToSpeechBackend.
/// </summary>
public class PiperTextToSpeechBackend : ITextToSpeechBackend
{
    public string BackendName => "PiperSharp";

    private string? _piperExecutablePath;

    public Task ConfigureAsync(BackendConfiguration config)
    {
        // LibraryPath can point to the Piper executable/binary
        _piperExecutablePath = config.LibraryPath;
        return Task.CompletedTask;
    }

    public Task<IModelHandle> LoadModelAsync(ModelLoadRequest request)
    {
        if (!File.Exists(request.FilePath))
            throw new FileNotFoundException($"Voice model not found: {request.FilePath}");

        IModelHandle handle = new PiperModelHandle(request.ModelId, request.FilePath);
        return Task.FromResult(handle);
    }

    public void UnloadModel(IModelHandle handle)
    {
        handle.Dispose();
    }

    public async Task<AudioGenerationResult> SynthesizeAsync(IModelHandle handle, AudioGenerationParams parameters, CancellationToken ct = default)
    {
        if (handle is not PiperModelHandle piperHandle || !piperHandle.IsLoaded)
            throw new InvalidOperationException("Invalid or unloaded model handle.");

        var sw = Stopwatch.StartNew();

        var voiceModel = await VoiceModel.LoadModel(Path.GetDirectoryName(piperHandle.ModelPath)!);

        var execPath = _piperExecutablePath ?? PiperDownloader.DefaultPiperExecutableLocation;

        var piperProvider = new PiperProvider(new PiperConfiguration
        {
            ExecutableLocation = execPath,
            WorkingDirectory = Path.GetDirectoryName(execPath)!,
            Model = voiceModel,
            SpeakingRate = parameters.Speed
        });

        var audioData = await piperProvider.InferAsync(parameters.Text, AudioOutputType.Wav);

        sw.Stop();

        return new AudioGenerationResult
        {
            AudioData = audioData,
            Format = "wav",
            SampleRate = parameters.SampleRate,
            DurationMs = sw.ElapsedMilliseconds
        };
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
