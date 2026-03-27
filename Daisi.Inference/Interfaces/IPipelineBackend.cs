using Daisi.Inference.Models;

namespace Daisi.Inference.Interfaces;

/// <summary>
/// DaisiChain pipeline parallelism backend interface.
/// Implementations can load a partial model (a contiguous subset of transformer layers)
/// and process forward pass steps for their assigned layer range.
/// </summary>
public interface IPipelineBackend
{
    /// <summary>
    /// Load a pipeline stage — only the specified layer range and optionally embedding/output head.
    /// </summary>
    Task<IPipelineHandle> LoadPipelineStageAsync(
        ModelLoadRequest request, int startLayer, int endLayer,
        bool includeEmbedding, bool includeOutputHead);
}

/// <summary>
/// Handle to a loaded pipeline stage. Processes activations for its assigned layer range.
/// </summary>
public interface IPipelineHandle : IModelHandle
{
    int StartLayer { get; }
    int EndLayer { get; }
    bool IncludeEmbedding { get; }
    bool IncludeOutputHead { get; }

    /// <summary>
    /// Process a single forward pass step for this pipeline stage.
    /// First stage (with embedding): pass tokenId, inputHidden=null.
    /// Middle/last stages: pass tokenId=-1, inputHidden from previous stage.
    /// Returns hidden state (middle stages) or logits (last stage with output head).
    /// </summary>
    float[] ProcessActivation(int position, int tokenId = -1, float[]? inputHidden = null);

    /// <summary>Reset KV cache and state for a new sequence.</summary>
    void ResetState();

    /// <summary>Whether this handle has a tokenizer (first/last stage).</summary>
    bool HasTokenizer { get; }

    /// <summary>Tokenize input text into token IDs.</summary>
    int[] Tokenize(string text);

    /// <summary>Decode a single token ID to text.</summary>
    string Detokenize(int tokenId);

    /// <summary>Get EOS/stop token IDs for this model.</summary>
    int[] GetStopTokenIds();
}
