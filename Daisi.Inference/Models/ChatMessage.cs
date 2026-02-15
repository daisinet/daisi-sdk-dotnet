namespace Daisi.Inference.Models;

/// <summary>
/// Represents a single message in a chat conversation.
/// </summary>
public class ChatMessage
{
    public ChatRole Role { get; set; }
    public string Content { get; set; } = string.Empty;

    /// <summary>Optional media attachments (images, audio, video).</summary>
    public List<MediaAttachment>? Attachments { get; set; }

    public ChatMessage() { }

    public ChatMessage(ChatRole role, string content)
    {
        Role = role;
        Content = content;
    }
}

/// <summary>
/// A media attachment on a chat message.
/// </summary>
public class MediaAttachment
{
    public MediaType Type { get; set; }
    public byte[] Data { get; set; } = [];
    public string MimeType { get; set; } = string.Empty;
}

/// <summary>
/// The type of a media attachment.
/// </summary>
public enum MediaType
{
    Image,
    Audio,
    Video
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
