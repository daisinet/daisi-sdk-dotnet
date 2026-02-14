using Daisi.Inference.Models;

namespace Daisi.Inference.Interfaces;

/// <summary>
/// A stateful chat session that maintains conversation history.
/// </summary>
public interface IChatSession : IDisposable
{
    /// <summary>
    /// Send a message and stream back generated tokens.
    /// </summary>
    IAsyncEnumerable<string> ChatAsync(ChatMessage message, TextGenerationParams parameters, CancellationToken ct = default);

    /// <summary>
    /// The conversation history.
    /// </summary>
    IReadOnlyList<ChatMessage> History { get; }

    /// <summary>
    /// Get a snapshot of the session's internal state.
    /// </summary>
    ChatSessionState GetState();

    /// <summary>
    /// Add a message to the history without generating a response.
    /// </summary>
    void AddMessage(ChatMessage message);
}
