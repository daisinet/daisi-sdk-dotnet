namespace Daisi.Inference.Models;

/// <summary>
/// Configuration for an inference backend's native library and runtime selection.
/// </summary>
public class BackendConfiguration
{
    /// <summary>Runtime to use (e.g. Auto, Cuda, Vulkan, Avx, Avx2, Avx512).</summary>
    public string Runtime { get; set; } = "Auto";

    /// <summary>Whether to log native library output.</summary>
    public bool ShowLogs { get; set; }

    /// <summary>Whether to automatically fall back to another runtime if the preferred one fails.</summary>
    public bool AutoFallback { get; set; } = true;

    /// <summary>Whether to skip the native library check.</summary>
    public bool SkipCheck { get; set; }

    /// <summary>Primary native library path.</summary>
    public string? LibraryPath { get; set; }

    /// <summary>Secondary native library path (e.g. for multimodal projectors).</summary>
    public string? SecondaryLibraryPath { get; set; }

    /// <summary>Additional directories to search for native libraries.</summary>
    public List<string> SearchDirectories { get; set; } = [];

    /// <summary>Optional callback for native library log output.</summary>
    public Action<string, string>? LogCallback { get; set; }
}
