using Daisi.Inference.Interfaces;
using Whisper.net;

namespace Daisi.Inference.Whisper;

/// <summary>
/// Wraps a Whisper.net factory and processor as an IModelHandle.
/// </summary>
public class WhisperModelHandle : IModelHandle
{
    public string ModelId { get; }
    public string FilePath { get; }
    public bool IsLoaded => Factory is not null;

    internal WhisperFactory? Factory { get; private set; }

    public WhisperModelHandle(string modelId, string filePath, WhisperFactory factory)
    {
        ModelId = modelId;
        FilePath = filePath;
        Factory = factory;
    }

    public void Dispose()
    {
        Factory?.Dispose();
        Factory = null;
    }
}
