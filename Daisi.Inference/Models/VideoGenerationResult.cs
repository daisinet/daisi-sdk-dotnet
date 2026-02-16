namespace Daisi.Inference.Models;

/// <summary>
/// Result of a video generation request.
/// </summary>
public class VideoGenerationResult
{
    /// <summary>Raw video data.</summary>
    public byte[] VideoData { get; set; } = [];

    /// <summary>Video format (e.g. "mp4").</summary>
    public string Format { get; set; } = "mp4";

    public int Width { get; set; }
    public int Height { get; set; }
    public int FrameCount { get; set; }
    public int Fps { get; set; }

    /// <summary>Duration of the video in milliseconds.</summary>
    public long DurationMs { get; set; }
}
