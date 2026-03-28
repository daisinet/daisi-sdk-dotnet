using System.Runtime.CompilerServices;
using System.Text;
using Daisi.Inference.Interfaces;
using Daisi.Inference.Models;
using Daisi.Llogos.Inference;
using Microsoft.Extensions.Logging;

namespace Daisi.Inference.DaisiLlogos;

/// <summary>
/// Chat session backed by daisi-llogos's TextGenerator.
/// Manages conversation history, formats prompts using chat templates,
/// and streams generated tokens.
/// </summary>
public class DaisiLlogosChatSession : IChatSession
{
    private readonly DaisiLlogosModelHandle _handle;
    private readonly string? _chatTemplate;
    private readonly ILogger? _logger;
    private readonly List<ChatMessage> _history = [];
    private int _totalTokensGenerated;

    public IReadOnlyList<ChatMessage> History => _history;

    public DaisiLlogosChatSession(DaisiLlogosModelHandle handle, string? chatTemplate, string? systemPrompt, ILogger? logger = null)
    {
        _handle = handle;
        _chatTemplate = chatTemplate;
        _logger = logger;

        if (systemPrompt is not null)
            _history.Add(new ChatMessage(ChatRole.System, systemPrompt));
    }

    public async IAsyncEnumerable<string> ChatAsync(
        ChatMessage message, TextGenerationParams parameters,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        _history.Add(message);

        // Reset model state and run the full conversation from scratch
        // (daisi-llogos's TextGenerator is stateless per-call, so we replay history each turn)
        _handle.ResetState();

        var prompt = FormatPrompt();
        var genParams = MapToGenerationParams(parameters);

        // Run generation on a background thread since daisi-llogos uses synchronous iteration
        var channel = System.Threading.Channels.Channel.CreateUnbounded<string>();

        _logger?.LogInformation("[DaisiLlogos] Starting generation, prompt length={Length}, maxTokens={MaxTokens}", prompt.Length, genParams.MaxTokens);

        _ = Task.Run(() =>
        {
            try
            {
                int tokenCount = 0;
                foreach (var token in _handle.Generate(prompt, genParams))
                {
                    if (ct.IsCancellationRequested) break;
                    if (token.IsDone)
                    {
                        _totalTokensGenerated += token.TotalTokens;
                        _logger?.LogInformation("[DaisiLlogos] Generation complete, {Count} tokens", token.TotalTokens);
                        break;
                    }
                    tokenCount++;
                    channel.Writer.TryWrite(token.Text);
                }
                if (tokenCount == 0)
                    _logger?.LogWarning("[DaisiLlogos] Generation produced 0 tokens");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "[DaisiLlogos] Generation failed: {Message}", ex.Message);
            }
            finally
            {
                channel.Writer.Complete();
            }
        }, ct);

        var assistantResponse = new StringBuilder();
        await foreach (var text in channel.Reader.ReadAllAsync(ct))
        {
            assistantResponse.Append(text);
            yield return text;
        }

        // Add assistant response to history
        _history.Add(new ChatMessage(ChatRole.Assistant, assistantResponse.ToString()));
    }

    public ChatSessionState GetState()
    {
        return new ChatSessionState
        {
            PastTokensCount = _totalTokensGenerated,
            ConsumedTokensCount = _totalTokensGenerated,
        };
    }

    public void AddMessage(ChatMessage message)
    {
        _history.Add(message);
    }

    public void Dispose()
    {
        // Model handle is owned by the backend, not the session
    }

    /// <summary>
    /// Format the conversation history into a prompt string.
    /// Uses ChatML-style formatting if a chat template is present, otherwise simple concatenation.
    /// </summary>
    private string FormatPrompt()
    {
        if (_chatTemplate is not null && _chatTemplate.Contains("im_start"))
            return FormatChatML();

        return FormatSimple();
    }

    /// <summary>
    /// ChatML format: &lt;|im_start|&gt;role\ncontent&lt;|im_end|&gt;
    /// Used by Qwen, Yi, and many other models.
    /// </summary>
    private string FormatChatML()
    {
        var sb = new StringBuilder();
        foreach (var msg in _history)
        {
            var role = msg.Role switch
            {
                ChatRole.System => "system",
                ChatRole.User => "user",
                ChatRole.Assistant => "assistant",
                _ => "user"
            };
            sb.Append($"<|im_start|>{role}\n{msg.Content}<|im_end|>\n");
        }
        // Prompt for assistant response
        sb.Append("<|im_start|>assistant\n");
        return sb.ToString();
    }

    /// <summary>
    /// Simple format for models without a known chat template.
    /// </summary>
    private string FormatSimple()
    {
        var sb = new StringBuilder();
        foreach (var msg in _history)
        {
            switch (msg.Role)
            {
                case ChatRole.System:
                    sb.AppendLine(msg.Content);
                    sb.AppendLine();
                    break;
                case ChatRole.User:
                    sb.Append("User: ");
                    sb.AppendLine(msg.Content);
                    break;
                case ChatRole.Assistant:
                    sb.Append("Assistant: ");
                    sb.AppendLine(msg.Content);
                    break;
            }
        }
        sb.Append("Assistant: ");
        return sb.ToString();
    }

    private static GenerationParams MapToGenerationParams(TextGenerationParams p)
    {
        return new GenerationParams
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
    }
}
