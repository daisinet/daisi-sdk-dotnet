using Daisi.Inference.Interfaces;
using Daisi.Inference.Models;
using HPPH.SkiaSharp;
using StableDiffusion.NET;
using System.Diagnostics;

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

        var model = new DiffusionModel(modelParams);

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

        var image = sdHandle.Model!.GenerateImage(genParams);

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
}
