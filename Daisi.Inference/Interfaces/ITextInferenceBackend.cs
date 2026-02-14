using Daisi.Inference.Models;

namespace Daisi.Inference.Interfaces;

/// <summary>
/// Abstraction for a text inference backend (LlamaSharp, ONNX Runtime GenAI, etc.).
/// </summary>
public interface ITextInferenceBackend : IAsyncDisposable
{
    /// <summary>Name of this backend implementation.</summary>
    string BackendName { get; }

    /// <summary>Configure the backend's native library and runtime settings.</summary>
    Task ConfigureAsync(BackendConfiguration config);

    /// <summary>Load a model file and return a handle.</summary>
    Task<IModelHandle> LoadModelAsync(ModelLoadRequest request);

    /// <summary>Unload a previously loaded model.</summary>
    void UnloadModel(IModelHandle handle);

    /// <summary>Create a new chat session against a loaded model.</summary>
    Task<IChatSession> CreateChatSessionAsync(IModelHandle handle, string? systemPrompt = null);
}
