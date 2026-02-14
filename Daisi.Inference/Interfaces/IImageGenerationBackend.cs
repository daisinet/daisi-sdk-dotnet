using Daisi.Inference.Models;

namespace Daisi.Inference.Interfaces;

/// <summary>
/// Abstraction for an image generation backend (StableDiffusion.NET, ONNX Runtime, etc.).
/// </summary>
public interface IImageGenerationBackend : IAsyncDisposable
{
    /// <summary>Name of this backend implementation.</summary>
    string BackendName { get; }

    /// <summary>Configure the backend's native library and runtime settings.</summary>
    Task ConfigureAsync(BackendConfiguration config);

    /// <summary>Load a model file and return a handle.</summary>
    Task<IModelHandle> LoadModelAsync(ModelLoadRequest request);

    /// <summary>Unload a previously loaded model.</summary>
    void UnloadModel(IModelHandle handle);

    /// <summary>Generate an image from the given parameters.</summary>
    Task<ImageGenerationResult> GenerateAsync(IModelHandle handle, ImageGenerationParams parameters, CancellationToken ct = default);
}
