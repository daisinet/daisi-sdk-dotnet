namespace Daisi.Inference.Models;

/// <summary>
/// Parameters for speech-to-text transcription.
/// </summary>
public class TranscriptionParams
{
    /// <summary>Language code (e.g. "en"). Null means auto-detect.</summary>
    public string? Language { get; set; }

    /// <summary>Whether to translate the transcription to English.</summary>
    public bool Translate { get; set; }

    /// <summary>Maximum segment length in characters.</summary>
    public int MaxSegmentLength { get; set; } = 0;
}
