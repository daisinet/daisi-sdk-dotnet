using Daisi.Inference.Interfaces;
using Daisi.Inference.Models;
using Microsoft.ML.OnnxRuntimeGenAI;
using System.Runtime.CompilerServices;
using System.Text;

namespace Daisi.Inference.OnnxRuntime;

/// <summary>
/// ONNX Runtime GenAI implementation of IChatSession.
/// </summary>
public class OnnxRuntimeChatSession : IChatSession
{
    private readonly OnnxRuntimeModelHandle _handle;
    private readonly List<ChatMessage> _history = [];

    public IReadOnlyList<ChatMessage> History => _history;

    public OnnxRuntimeChatSession(OnnxRuntimeModelHandle handle, string? systemPrompt = null)
    {
        _handle = handle;

        if (!string.IsNullOrWhiteSpace(systemPrompt))
            _history.Add(new ChatMessage(ChatRole.System, systemPrompt));
    }

    public async IAsyncEnumerable<string> ChatAsync(ChatMessage message, TextGenerationParams parameters, [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!_handle.IsLoaded)
            throw new InvalidOperationException("Model is not loaded.");

        _history.Add(message);

        // Build the full prompt from history
        var prompt = BuildPrompt();

        var sequences = _handle.Tokenizer!.Encode(prompt);

        using var genParams = new GeneratorParams(_handle.Model!);
        genParams.SetSearchOption("max_length", parameters.MaxTokens > 0 ? parameters.MaxTokens : 256);
        genParams.SetSearchOption("temperature", parameters.Temperature);
        genParams.SetSearchOption("top_p", parameters.TopP);
        genParams.SetSearchOption("top_k", parameters.TopK);
        genParams.SetSearchOption("repetition_penalty", parameters.RepeatPenalty > 0 ? parameters.RepeatPenalty : 1.0f);

        using var generator = new Generator(_handle.Model!, genParams);
        generator.AppendTokenSequences(sequences);

        var responseBuilder = new StringBuilder();

        while (!generator.IsDone())
        {
            if (ct.IsCancellationRequested)
                break;

            generator.GenerateNextToken();

            var outputSequence = generator.GetSequence(0);
            var newToken = _handle.Tokenizer.Decode(outputSequence[^1..]);

            responseBuilder.Append(newToken);
            yield return newToken;
        }

        _history.Add(new ChatMessage(ChatRole.Assistant, responseBuilder.ToString()));
    }

    public ChatSessionState GetState()
    {
        return new ChatSessionState
        {
            PastTokensCount = 0,
            ConsumedTokensCount = 0,
            LastTokensCapacity = 0,
            ConsumedSessionCount = 0
        };
    }

    public void AddMessage(ChatMessage message)
    {
        _history.Add(message);
    }

    public void Dispose()
    {
        // Model and tokenizer lifecycle is managed by the handle
    }

    private string BuildPrompt()
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
            sb.AppendLine($"<|{role}|>");
            sb.AppendLine(msg.Content);
            sb.AppendLine($"<|end|>");
        }
        sb.AppendLine("<|assistant|>");
        return sb.ToString();
    }
}
