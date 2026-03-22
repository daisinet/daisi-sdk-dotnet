using Daisi.Inference.Interfaces;
using Daisi.Llogos;
using Daisi.Llogos.Inference;
using Daisi.Llogos.Model;
using Daisi.Llogos.Tokenizer;

namespace Daisi.Inference.DaisiLlogos;

/// <summary>
/// Holds the loaded daisi-llogos model state: compute backend, weights, caches, and generator.
/// Disposing unloads everything and frees GPU/CPU memory.
/// </summary>
public class DaisiLlogosModelHandle : IModelHandle
{
    public string ModelId { get; }
    public string FilePath { get; }
    public bool IsLoaded => _computeBackend is not null;

    internal ModelConfig Config { get; }
    internal BpeTokenizer Tokenizer { get; }
    internal bool IsBitNet { get; }

    // Text generator (one of these will be set based on model type)
    private TextGenerator? _textGenerator;
    private BitNetTextGenerator? _bitNetGenerator;

    // Model resources
    private ModelWeights? _weights;
    private IKvCache? _kvCache;
    private DeltaNetState? _deltaState;
    private ForwardPass? _forwardPass;
    private BitNetForwardPass? _bitNetForwardPass;
    private IComputeBackend? _computeBackend;

    /// <summary>Standard model constructor.</summary>
    public DaisiLlogosModelHandle(
        string modelId, string filePath, IComputeBackend computeBackend,
        ModelConfig config, BpeTokenizer tokenizer, TextGenerator generator,
        ModelWeights weights, KvCache kvCache,
        DeltaNetState deltaState, ForwardPass forwardPass)
    {
        ModelId = modelId;
        FilePath = filePath;
        Config = config;
        Tokenizer = tokenizer;
        IsBitNet = false;
        _computeBackend = computeBackend;
        _textGenerator = generator;
        _weights = weights;
        _kvCache = kvCache;
        _deltaState = deltaState;
        _forwardPass = forwardPass;
    }

    /// <summary>BitNet model constructor.</summary>
    public DaisiLlogosModelHandle(
        string modelId, string filePath, IComputeBackend computeBackend,
        ModelConfig config, BpeTokenizer tokenizer, BitNetTextGenerator generator,
        ModelWeights weights, BitNetKvCache kvCache,
        BitNetForwardPass forwardPass)
    {
        ModelId = modelId;
        FilePath = filePath;
        Config = config;
        Tokenizer = tokenizer;
        IsBitNet = true;
        _computeBackend = computeBackend;
        _bitNetGenerator = generator;
        _weights = weights;
        _kvCache = kvCache;
        _bitNetForwardPass = forwardPass;
    }

    /// <summary>
    /// Generate tokens from a prompt using the loaded model.
    /// </summary>
    internal IEnumerable<GenerationToken> Generate(string prompt, GenerationParams parameters)
    {
        if (_textGenerator is not null)
            return _textGenerator.Generate(prompt, parameters);
        if (_bitNetGenerator is not null)
            return _bitNetGenerator.Generate(prompt, parameters);
        throw new InvalidOperationException("No generator available — model may be unloaded.");
    }

    /// <summary>
    /// Reset the KV cache and DeltaNet state for a new conversation.
    /// </summary>
    internal void ResetState()
    {
        _kvCache?.Reset();
        _deltaState?.Reset();
    }

    public void Dispose()
    {
        _forwardPass?.Dispose();
        _bitNetForwardPass?.Dispose();
        _deltaState?.Dispose();
        _kvCache?.Dispose();
        _weights?.Dispose();
        _computeBackend?.Dispose();

        _forwardPass = null;
        _bitNetForwardPass = null;
        _deltaState = null;
        _kvCache = null;
        _weights = null;
        _textGenerator = null;
        _bitNetGenerator = null;
        _computeBackend = null;
    }
}
