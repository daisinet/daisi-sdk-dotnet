namespace Daisi.Inference.Models;

/// <summary>
/// Result of an image generation request.
/// </summary>
public class ImageGenerationResult
{
    public byte[] ImageData { get; set; } = [];
    public int Width { get; set; }
    public int Height { get; set; }
    public string Format { get; set; } = "png";
    public long GenerationTimeMs { get; set; }
}
