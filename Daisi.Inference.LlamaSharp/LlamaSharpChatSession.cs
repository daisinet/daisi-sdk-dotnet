using Daisi.Inference.Interfaces;
using Daisi.Inference.Models;
using LLama;
using LLama.Common;
using LLama.Sampling;
using System.Runtime.CompilerServices;

namespace Daisi.Inference.LlamaSharp;

/// <summary>
/// Wraps LLamaSharp's ChatSession as an IChatSession.
/// </summary>
public class LlamaSharpChatSession : IChatSession
{
    private readonly LLama.ChatSession _session;
    private readonly LLamaContext _context;

    public LlamaSharpChatSession(LLama.ChatSession session, LLamaContext context)
    {
        _session = session;
        _context = context;
    }

    public IReadOnlyList<ChatMessage> History =>
        _session.History.Messages
            .Select(m => new ChatMessage(MapRole(m.AuthorRole), m.Content))
            .ToList();

    public async IAsyncEnumerable<string> ChatAsync(ChatMessage message, TextGenerationParams parameters, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var llamaMessage = new ChatHistory.Message(MapToAuthorRole(message.Role), message.Content);
        var inferenceParams = MapToInferenceParams(parameters);

        await foreach (var token in _session.ChatAsync(llamaMessage, inferenceParams, ct))
        {
            yield return token;
        }
    }

    public ChatSessionState GetState()
    {
        var sessionState = _session.GetSessionState();
        var executorState = sessionState?.ExecutorState;

        return new ChatSessionState
        {
            PastTokensCount = executorState?.PastTokensCount ?? 0,
            ConsumedTokensCount = executorState?.ConsumedTokensCount ?? 0,
            LastTokensCapacity = executorState?.LastTokensCapacity ?? 0,
            ConsumedSessionCount = executorState?.ConsumedSessionCount ?? 0
        };
    }

    public void AddMessage(ChatMessage message)
    {
        _session.History.AddMessage(MapToAuthorRole(message.Role), message.Content);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private static InferenceParams MapToInferenceParams(TextGenerationParams p)
    {
        Grammar? grammar = null;
        if (!string.IsNullOrEmpty(p.GrammarText))
            grammar = new Grammar(p.GrammarText, p.GrammarRootRule);

        return new InferenceParams
        {
            AntiPrompts = p.AntiPrompts is { Count: > 0 } ? p.AntiPrompts.ToList() : [],
            TokensKeep = p.TokensKeep,
            MaxTokens = p.MaxTokens,
            DecodeSpecialTokens = p.DecodeSpecialTokens,
            SamplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = p.Temperature,
                TopP = p.TopP,
                TopK = p.TopK,
                RepeatPenalty = p.RepeatPenalty,
                Seed = p.Seed,
                FrequencyPenalty = p.FrequencyPenalty,
                MinKeep = p.MinKeep,
                MinP = p.MinP,
                PenalizeNewline = p.PenalizeNewline,
                PenaltyCount = p.PenaltyCount,
                PresencePenalty = p.PresencePenalty,
                PreventEOS = p.PreventEOS,
                TypicalP = p.TypicalP,
                Grammar = grammar
            }
        };
    }

    private static ChatRole MapRole(AuthorRole role) => role switch
    {
        AuthorRole.System => ChatRole.System,
        AuthorRole.User => ChatRole.User,
        AuthorRole.Assistant => ChatRole.Assistant,
        _ => ChatRole.User
    };

    private static AuthorRole MapToAuthorRole(ChatRole role) => role switch
    {
        ChatRole.System => AuthorRole.System,
        ChatRole.User => AuthorRole.User,
        ChatRole.Assistant => AuthorRole.Assistant,
        _ => AuthorRole.User
    };
}
