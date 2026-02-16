using Daisi.Inference.Interfaces;

namespace Daisi.Inference.Piper;

/// <summary>
/// Wraps a Piper voice model as an IModelHandle.
/// </summary>
public class PiperModelHandle : IModelHandle
{
    public string ModelId { get; }
    public string FilePath { get; }
    public bool IsLoaded { get; private set; } = true;

    /// <summary>Path to the ONNX voice model file.</summary>
    internal string ModelPath { get; }

    /// <summary>Path to the model's JSON config file.</summary>
    internal string ConfigPath { get; }

    public PiperModelHandle(string modelId, string filePath)
    {
        ModelId = modelId;
        FilePath = filePath;
        ModelPath = filePath;

        // Piper expects a .onnx.json config file alongside the .onnx model
        ConfigPath = filePath + ".json";
    }

    public void Dispose()
    {
        IsLoaded = false;
    }
}
