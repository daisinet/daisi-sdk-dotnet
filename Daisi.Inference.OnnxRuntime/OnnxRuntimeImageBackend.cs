using Daisi.Inference.Interfaces;
using Daisi.Inference.Models;

namespace Daisi.Inference.OnnxRuntime;

/// <summary>
/// ONNX Runtime implementation of IImageGenerationBackend for ONNX-based diffusion models.
/// This is a placeholder — full implementation requires model-specific ONNX pipeline setup.
/// </summary>
public class OnnxRuntimeImageBackend : IImageGenerationBackend
{
    public string BackendName => "OnnxRuntimeImage";

    public Task ConfigureAsync(BackendConfiguration config)
    {
        return Task.CompletedTask;
    }

    public Task<IModelHandle> LoadModelAsync(ModelLoadRequest request)
    {
        throw new NotSupportedException("ONNX Runtime image generation requires model-specific pipeline configuration. Use StableDiffusion.NET for image generation.");
    }

    public void UnloadModel(IModelHandle handle)
    {
        handle.Dispose();
    }

    public Task<ImageGenerationResult> GenerateAsync(IModelHandle handle, ImageGenerationParams parameters, CancellationToken ct = default)
    {
        throw new NotSupportedException("ONNX Runtime image generation is not yet implemented.");
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
