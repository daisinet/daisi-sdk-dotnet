namespace Daisi.Inference.Models;

/// <summary>
/// Result of a text-to-speech synthesis.
/// </summary>
public class AudioGenerationResult
{
    /// <summary>Raw audio data.</summary>
    public byte[] AudioData { get; set; } = [];

    /// <summary>Audio format (e.g. "wav").</summary>
    public string Format { get; set; } = "wav";

    /// <summary>Sample rate in Hz.</summary>
    public int SampleRate { get; set; }

    /// <summary>Duration of the audio in milliseconds.</summary>
    public long DurationMs { get; set; }
}
