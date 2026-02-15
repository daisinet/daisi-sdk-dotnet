using Daisi.Inference.Models;

namespace Daisi.Inference.Interfaces;

/// <summary>
/// Abstraction for a speech-to-text backend (Whisper, etc.).
/// </summary>
public interface ISpeechToTextBackend : IAsyncDisposable
{
    /// <summary>Name of this backend implementation.</summary>
    string BackendName { get; }

    /// <summary>Configure the backend's native library and runtime settings.</summary>
    Task ConfigureAsync(BackendConfiguration config);

    /// <summary>Load a model file and return a handle.</summary>
    Task<IModelHandle> LoadModelAsync(ModelLoadRequest request);

    /// <summary>Unload a previously loaded model.</summary>
    void UnloadModel(IModelHandle handle);

    /// <summary>Transcribe audio data to text.</summary>
    Task<TranscriptionResult> TranscribeAsync(IModelHandle handle, byte[] audioData, TranscriptionParams parameters, CancellationToken ct = default);
}
