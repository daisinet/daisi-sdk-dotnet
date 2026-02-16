namespace Daisi.Inference.Models;

/// <summary>
/// Parameters for image generation.
/// </summary>
public class ImageGenerationParams
{
    public string Prompt { get; set; } = string.Empty;
    public string? NegativePrompt { get; set; }
    public int Width { get; set; } = 512;
    public int Height { get; set; } = 512;
    public int Steps { get; set; } = 20;
    public float CfgScale { get; set; } = 7.0f;
    public long Seed { get; set; } = -1;
    public int BatchCount { get; set; } = 1;
}
