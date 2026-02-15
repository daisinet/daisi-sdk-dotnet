namespace Daisi.Inference.Models;

/// <summary>
/// Parameters for video generation.
/// </summary>
public class VideoGenerationParams
{
    public string Prompt { get; set; } = string.Empty;
    public string? NegativePrompt { get; set; }
    public int Width { get; set; } = 512;
    public int Height { get; set; } = 512;
    public int Steps { get; set; } = 20;
    public float CfgScale { get; set; } = 7.0f;
    public long Seed { get; set; } = -1;
    public int FrameCount { get; set; } = 16;
    public int Fps { get; set; } = 8;
}
