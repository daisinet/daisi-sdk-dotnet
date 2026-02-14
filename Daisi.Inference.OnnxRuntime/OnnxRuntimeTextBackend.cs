using Daisi.Inference.Interfaces;
using Daisi.Inference.Models;
using Microsoft.ML.OnnxRuntimeGenAI;

namespace Daisi.Inference.OnnxRuntime;

/// <summary>
/// ONNX Runtime GenAI implementation of ITextInferenceBackend.
/// </summary>
public class OnnxRuntimeTextBackend : ITextInferenceBackend
{
    public string BackendName => "OnnxRuntimeGenAI";

    public Task ConfigureAsync(BackendConfiguration config)
    {
        // ONNX Runtime GenAI does not require global native library configuration
        return Task.CompletedTask;
    }

    public Task<IModelHandle> LoadModelAsync(ModelLoadRequest request)
    {
        // ONNX Runtime GenAI loads from a directory containing the model files
        var model = new Model(request.FilePath);
        var tokenizer = new Tokenizer(model);

        IModelHandle handle = new OnnxRuntimeModelHandle(request.ModelId, request.FilePath, model, tokenizer);
        return Task.FromResult(handle);
    }

    public void UnloadModel(IModelHandle handle)
    {
        handle.Dispose();
    }

    public Task<IChatSession> CreateChatSessionAsync(IModelHandle handle, string? systemPrompt = null)
    {
        if (handle is not OnnxRuntimeModelHandle onnxHandle || !onnxHandle.IsLoaded)
            throw new InvalidOperationException("Invalid or unloaded model handle.");

        IChatSession session = new OnnxRuntimeChatSession(onnxHandle, systemPrompt);
        return Task.FromResult(session);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
