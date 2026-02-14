namespace Daisi.Inference.Models;

/// <summary>
/// Parameters for loading a model into a backend.
/// </summary>
public class ModelLoadRequest
{
    /// <summary>Unique identifier for the model.</summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>Path to the model file.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Context size in tokens.</summary>
    public uint ContextSize { get; set; } = 2048;

    /// <summary>Number of layers to offload to GPU. -1 means all.</summary>
    public int GpuLayerCount { get; set; } = -1;

    /// <summary>Batch size for prompt processing.</summary>
    public uint BatchSize { get; set; } = 128;
}
