using Daisi.Inference.Interfaces;
using Microsoft.ML.OnnxRuntimeGenAI;

namespace Daisi.Inference.OnnxRuntime;

/// <summary>
/// Wraps ONNX Runtime GenAI Model as an IModelHandle.
/// </summary>
public class OnnxRuntimeModelHandle : IModelHandle
{
    public string ModelId { get; }
    public string FilePath { get; }
    public bool IsLoaded => Model is not null;

    internal Model? Model { get; private set; }
    internal Tokenizer? Tokenizer { get; private set; }

    public OnnxRuntimeModelHandle(string modelId, string filePath, Model model, Tokenizer tokenizer)
    {
        ModelId = modelId;
        FilePath = filePath;
        Model = model;
        Tokenizer = tokenizer;
    }

    public void Dispose()
    {
        Tokenizer?.Dispose();
        Tokenizer = null;
        Model?.Dispose();
        Model = null;
    }
}
