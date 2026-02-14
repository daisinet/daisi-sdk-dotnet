namespace Daisi.Inference.Models;

/// <summary>
/// Snapshot of a chat session's internal state for diagnostics.
/// </summary>
public class ChatSessionState
{
    public int PastTokensCount { get; set; }
    public int ConsumedTokensCount { get; set; }
    public int LastTokensCapacity { get; set; }
    public int ConsumedSessionCount { get; set; }
}
