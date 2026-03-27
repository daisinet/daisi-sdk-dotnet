using Daisi.Inference.Interfaces;
using Daisi.Llogos;
using Daisi.Llogos.Inference;
using Daisi.Llogos.Model;
using Daisi.Llogos.Tokenizer;

namespace Daisi.Inference.DaisiLlogos;

/// <summary>
/// Holds a partially-loaded model for a DaisiChain pipeline stage.
/// Only the assigned layer range (and optionally embedding/output head) is loaded into VRAM.
/// Unlike DaisiLlogosModelHandle, this does not create a TextGenerator — pipeline stages
/// process individual forward pass steps, not full inference loops.
/// </summary>
public class DaisiLlogosPipelineHandle : IPipelineHandle
{
    public string ModelId { get; }
    public string FilePath { get; }
    public bool IsLoaded => _computeBackend is not null;

    internal ModelConfig Config { get; }
    internal ForwardPass ForwardPass => _forwardPass!;
    internal BpeTokenizer? Tokenizer { get; }

    /// <summary>First layer this stage is responsible for (inclusive).</summary>
    public int StartLayer { get; }

    /// <summary>Last layer this stage is responsible for (exclusive).</summary>
    public int EndLayer { get; }

    /// <summary>Whether this stage includes the embedding lookup.</summary>
    public bool IncludeEmbedding { get; }

    /// <summary>Whether this stage includes the output head (RmsNorm + LM head).</summary>
    public bool IncludeOutputHead { get; }

    private ModelWeights? _weights;
    private IKvCache? _kvCache;
    private DeltaNetState? _deltaState;
    private ForwardPass? _forwardPass;
    private IComputeBackend? _computeBackend;

    public DaisiLlogosPipelineHandle(
        string modelId, string filePath, IComputeBackend computeBackend,
        ModelConfig config, ModelWeights weights, KvCache kvCache,
        DeltaNetState deltaState, ForwardPass forwardPass,
        int startLayer, int endLayer, bool includeEmbedding, bool includeOutputHead,
        BpeTokenizer? tokenizer = null)
    {
        ModelId = modelId;
        FilePath = filePath;
        Config = config;
        Tokenizer = tokenizer;
        StartLayer = startLayer;
        EndLayer = endLayer;
        IncludeEmbedding = includeEmbedding;
        IncludeOutputHead = includeOutputHead;
        _computeBackend = computeBackend;
        _weights = weights;
        _kvCache = kvCache;
        _deltaState = deltaState;
        _forwardPass = forwardPass;
    }

    /// <summary>
    /// Process a single token through this pipeline stage.
    /// First stage (with embedding): tokenId → embedding → layers → hidden state.
    /// Middle stages: hidden state in → layers → hidden state out.
    /// Last stage (with output head): hidden state in → layers → logits.
    /// </summary>
    public float[] ProcessActivation(int position, int tokenId = -1, float[]? inputHidden = null)
    {
        if (_forwardPass is null)
            throw new InvalidOperationException("Pipeline handle is not loaded.");

        if (IncludeEmbedding && tokenId >= 0)
        {
            _forwardPass.ForwardEmbedding(tokenId);
        }
        else if (inputHidden is not null)
        {
            _forwardPass.SetHidden(inputHidden);
        }
        else
        {
            throw new ArgumentException("Pipeline stage requires either a tokenId (first stage) or inputHidden (middle/last stage).");
        }

        _forwardPass.ForwardLayers(StartLayer, EndLayer, position);

        if (IncludeOutputHead)
        {
            var logits = new float[Config.VocabSize];
            _forwardPass.ForwardOutputHead(logits);
            return logits;
        }
        else
        {
            var hidden = new float[Config.HiddenDim];
            _forwardPass.GetHidden(hidden);
            return hidden;
        }
    }

    /// <summary>Reset KV cache and DeltaNet state for a new sequence.</summary>
    public void ResetState()
    {
        _kvCache?.Reset();
        _deltaState?.Reset();
    }

    public bool HasTokenizer => Tokenizer is not null;

    public int[] Tokenize(string text)
    {
        if (Tokenizer is null)
            throw new InvalidOperationException("This pipeline stage does not have a tokenizer.");
        return Tokenizer.Encode(text);
    }

    public string Detokenize(int tokenId)
    {
        if (Tokenizer is null)
            throw new InvalidOperationException("This pipeline stage does not have a tokenizer.");
        return Tokenizer.Decode(new[] { tokenId });
    }

    public int[] GetStopTokenIds()
    {
        if (Tokenizer is null)
            return Array.Empty<int>();
        return new[] { Tokenizer.Vocabulary.EosTokenId };
    }

    public void Dispose()
    {
        _forwardPass?.Dispose();
        _deltaState?.Dispose();
        _kvCache?.Dispose();
        _weights?.Dispose();
        _computeBackend?.Dispose();

        _forwardPass = null;
        _deltaState = null;
        _kvCache = null;
        _weights = null;
        _computeBackend = null;
    }
}
