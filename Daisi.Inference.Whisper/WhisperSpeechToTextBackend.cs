using Daisi.Inference.Interfaces;
using Daisi.Inference.Models;
using Whisper.net;
using System.Diagnostics;

namespace Daisi.Inference.Whisper;

/// <summary>
/// Whisper.net implementation of ISpeechToTextBackend.
/// </summary>
public class WhisperSpeechToTextBackend : ISpeechToTextBackend
{
    public string BackendName => "Whisper.net";

    public Task ConfigureAsync(BackendConfiguration config)
    {
        return Task.CompletedTask;
    }

    public Task<IModelHandle> LoadModelAsync(ModelLoadRequest request)
    {
        var factory = WhisperFactory.FromPath(request.FilePath);
        IModelHandle handle = new WhisperModelHandle(request.ModelId, request.FilePath, factory);
        return Task.FromResult(handle);
    }

    public void UnloadModel(IModelHandle handle)
    {
        handle.Dispose();
    }

    public async Task<TranscriptionResult> TranscribeAsync(IModelHandle handle, byte[] audioData, TranscriptionParams parameters, CancellationToken ct = default)
    {
        if (handle is not WhisperModelHandle whisperHandle || !whisperHandle.IsLoaded)
            throw new InvalidOperationException("Invalid or unloaded model handle.");

        var sw = Stopwatch.StartNew();

        var builder = whisperHandle.Factory!.CreateBuilder();

        if (!string.IsNullOrWhiteSpace(parameters.Language))
            builder.WithLanguage(parameters.Language);

        if (parameters.Translate)
            builder.WithTranslate();

        using var processor = builder.Build();

        using var audioStream = new MemoryStream(audioData);

        var segments = new List<TranscriptionSegment>();
        var fullText = new System.Text.StringBuilder();

        await foreach (var segment in processor.ProcessAsync(audioStream, ct))
        {
            segments.Add(new TranscriptionSegment
            {
                Start = segment.Start,
                End = segment.End,
                Text = segment.Text
            });
            fullText.Append(segment.Text);
        }

        sw.Stop();

        return new TranscriptionResult
        {
            Text = fullText.ToString().Trim(),
            Language = parameters.Language ?? "auto",
            Segments = segments,
            DurationMs = sw.ElapsedMilliseconds
        };
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
