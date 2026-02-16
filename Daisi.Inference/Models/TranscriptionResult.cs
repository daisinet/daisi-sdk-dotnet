namespace Daisi.Inference.Models;

/// <summary>
/// Result of a speech-to-text transcription.
/// </summary>
public class TranscriptionResult
{
    /// <summary>Full transcribed text.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Detected or specified language.</summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>Individual transcription segments with timestamps.</summary>
    public List<TranscriptionSegment> Segments { get; set; } = [];

    /// <summary>Duration of the audio in milliseconds.</summary>
    public long DurationMs { get; set; }
}

/// <summary>
/// A single segment of a transcription with timing information.
/// </summary>
public class TranscriptionSegment
{
    /// <summary>Start time of the segment.</summary>
    public TimeSpan Start { get; set; }

    /// <summary>End time of the segment.</summary>
    public TimeSpan End { get; set; }

    /// <summary>Transcribed text for this segment.</summary>
    public string Text { get; set; } = string.Empty;
}
