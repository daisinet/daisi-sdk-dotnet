using Daisi.Inference.Interfaces;
using LLama;
using LLama.Common;

namespace Daisi.Inference.LlamaSharp;

/// <summary>
/// Wraps LLamaWeights and ModelParams as an IModelHandle.
/// </summary>
public class LlamaSharpModelHandle : IModelHandle
{
    public string ModelId { get; }
    public string FilePath { get; }
    public bool IsLoaded => Weights is not null;

    internal LLamaWeights? Weights { get; private set; }
    internal ModelParams Parameters { get; }

    public LlamaSharpModelHandle(string modelId, string filePath, LLamaWeights weights, ModelParams parameters)
    {
        ModelId = modelId;
        FilePath = filePath;
        Weights = weights;
        Parameters = parameters;
    }

    public void Dispose()
    {
        Weights?.Dispose();
        Weights = null;
    }
}
