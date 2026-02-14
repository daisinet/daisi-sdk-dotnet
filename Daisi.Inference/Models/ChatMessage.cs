namespace Daisi.Inference.Models;

/// <summary>
/// Represents a single message in a chat conversation.
/// </summary>
public class ChatMessage
{
    public ChatRole Role { get; set; }
    public string Content { get; set; } = string.Empty;

    public ChatMessage() { }

    public ChatMessage(ChatRole role, string content)
    {
        Role = role;
        Content = content;
    }
}

/// <summary>
/// The role of a chat message author.
/// </summary>
public enum ChatRole
{
    System,
    User,
    Assistant
}
