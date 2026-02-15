namespace Daisi.Inference.Models;

/// <summary>
/// Parameters for text-to-speech synthesis.
/// </summary>
public class AudioGenerationParams
{
    /// <summary>Text to synthesize into speech.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Voice model identifier. Null uses the default voice.</summary>
    public string? VoiceId { get; set; }

    /// <summary>Speech speed multiplier.</summary>
    public float Speed { get; set; } = 1.0f;

    /// <summary>Output sample rate in Hz.</summary>
    public int SampleRate { get; set; } = 22050;

    /// <summary>Output audio format.</summary>
    public string OutputFormat { get; set; } = "wav";
}
