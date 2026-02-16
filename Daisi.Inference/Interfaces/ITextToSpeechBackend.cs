using Daisi.Inference.Models;

namespace Daisi.Inference.Interfaces;

/// <summary>
/// Abstraction for a text-to-speech backend (Piper, etc.).
/// </summary>
public interface ITextToSpeechBackend : IAsyncDisposable
{
    /// <summary>Name of this backend implementation.</summary>
    string BackendName { get; }

    /// <summary>Configure the backend's native library and runtime settings.</summary>
    Task ConfigureAsync(BackendConfiguration config);

    /// <summary>Load a voice model and return a handle.</summary>
    Task<IModelHandle> LoadModelAsync(ModelLoadRequest request);

    /// <summary>Unload a previously loaded model.</summary>
    void UnloadModel(IModelHandle handle);

    /// <summary>Synthesize speech from text.</summary>
    Task<AudioGenerationResult> SynthesizeAsync(IModelHandle handle, AudioGenerationParams parameters, CancellationToken ct = default);
}
