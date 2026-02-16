namespace Daisi.Inference.Models;

/// <summary>
/// Sampling and generation parameters for text inference.
/// </summary>
public class TextGenerationParams
{
    public IReadOnlyList<string> AntiPrompts { get; set; } = [];
    public int TokensKeep { get; set; }
    public int MaxTokens { get; set; } = 256;
    public bool DecodeSpecialTokens { get; set; }

    // Sampling parameters
    public float Temperature { get; set; } = 0.8f;
    public float TopP { get; set; } = 0.95f;
    public int TopK { get; set; } = 40;
    public float RepeatPenalty { get; set; } = 1.1f;
    public uint Seed { get; set; }
    public float FrequencyPenalty { get; set; }
    public int MinKeep { get; set; } = 1;
    public float MinP { get; set; }
    public bool PenalizeNewline { get; set; }
    public int PenaltyCount { get; set; } = 64;
    public float PresencePenalty { get; set; }
    public bool PreventEOS { get; set; }
    public float TypicalP { get; set; } = 1.0f;

    /// <summary>GBNF grammar text for constrained output. Null means unconstrained.</summary>
    public string? GrammarText { get; set; }

    /// <summary>Root rule name for the grammar (default "root").</summary>
    public string GrammarRootRule { get; set; } = "root";
}
