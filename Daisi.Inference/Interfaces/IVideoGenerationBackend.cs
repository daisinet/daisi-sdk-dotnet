using Daisi.Inference.Models;

namespace Daisi.Inference.Interfaces;

/// <summary>
/// Abstraction for a video generation backend.
/// </summary>
public interface IVideoGenerationBackend : IAsyncDisposable
{
    /// <summary>Name of this backend implementation.</summary>
    string BackendName { get; }

    /// <summary>Configure the backend's native library and runtime settings.</summary>
    Task ConfigureAsync(BackendConfiguration config);

    /// <summary>Load a model file and return a handle.</summary>
    Task<IModelHandle> LoadModelAsync(ModelLoadRequest request);

    /// <summary>Unload a previously loaded model.</summary>
    void UnloadModel(IModelHandle handle);

    /// <summary>Generate a video from the given parameters.</summary>
    Task<VideoGenerationResult> GenerateAsync(IModelHandle handle, VideoGenerationParams parameters, CancellationToken ct = default);
}
