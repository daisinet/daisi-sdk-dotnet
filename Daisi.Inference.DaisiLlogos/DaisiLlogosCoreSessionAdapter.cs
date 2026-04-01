using System.Runtime.CompilerServices;
using System.Text;
using Daisi.Inference.Interfaces;
using Daisi.Inference.Models;
using Daisi.Llogos.Inference;

namespace Daisi.Inference.DaisiLlogos;

/// <summary>
/// Adapts Daisi.Llogos.Chat.DaisiLlogosChatSession to the Daisi.Inference.IChatSession interface.
/// This provides proper ChatML template rendering, <![CDATA[<|im_end|>]]> stop sequences,
/// KV cache reuse across turns, grammar constraints, and AntiPrompt support.
/// </summary>
public class DaisiLlogosCoreSessionAdapter : IChatSession
{
    private readonly Llogos.Chat.DaisiLlogosChatSession _inner;

    public DaisiLlogosCoreSessionAdapter(Llogos.Chat.DaisiLlogosChatSession inner)
    {
        _inner = inner;
    }

    public IReadOnlyList<ChatMessage> History =>
        _inner.History.Select(m => new ChatMessage(
            MapRole(m.Role), m.Content)).ToList();

    public async IAsyncEnumerable<string> ChatAsync(
        ChatMessage message, TextGenerationParams parameters,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var innerMsg = new Llogos.Chat.ChatMessage(
            MapRoleToString(message.Role), message.Content);
        var genParams = MapParams(parameters);

        await foreach (var token in _inner.ChatAsync(innerMsg, genParams, ct))
            yield return token;
    }

    public ChatSessionState GetState() => new()
    {
        PastTokensCount = _inner.CachedTokenCount,
    };

    public void AddMessage(ChatMessage message)
    {
        _inner.AddMessage(new Llogos.Chat.ChatMessage(
            MapRoleToString(message.Role), message.Content));
    }

    public void Dispose() => _inner.Dispose();

    private static GenerationParams MapParams(TextGenerationParams p) => new()
    {
        MaxTokens = p.MaxTokens,
        Temperature = p.Temperature,
        TopK = p.TopK,
        TopP = p.TopP,
        RepetitionPenalty = p.RepeatPenalty,
        Seed = p.Seed > 0 ? (int)p.Seed : null,
        FrequencyPenalty = p.FrequencyPenalty,
        PresencePenalty = p.PresencePenalty,
        MinP = p.MinP,
        TypicalP = p.TypicalP,
        PenalizeNewline = p.PenalizeNewline,
        PenaltyCount = p.PenaltyCount,
        MinKeep = p.MinKeep,
        PreventEOS = p.PreventEOS,
        AntiPrompts = p.AntiPrompts?.Count > 0 ? p.AntiPrompts.ToArray() : null,
        GrammarText = p.GrammarText,
    };

    private static ChatRole MapRole(string role) => role switch
    {
        "system" => ChatRole.System,
        "assistant" => ChatRole.Assistant,
        _ => ChatRole.User,
    };

    private static string MapRoleToString(ChatRole role) => role switch
    {
        ChatRole.System => "system",
        ChatRole.Assistant => "assistant",
        ChatRole.User => "user",
        _ => "user",
    };
}
