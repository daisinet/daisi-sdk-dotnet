using Daisi.Inference.Interfaces;
using Daisi.Inference.Models;
using LLama;
using LLama.Common;
using LLama.Native;
using LLama.Transformers;
using Microsoft.Extensions.Logging;

namespace Daisi.Inference.LlamaSharp;

/// <summary>
/// LlamaSharp implementation of ITextInferenceBackend.
/// </summary>
public class LlamaSharpTextBackend : ITextInferenceBackend
{
    private readonly ILogger? _logger;

    public string BackendName => "LlamaSharp";

    public LlamaSharpTextBackend(ILogger<LlamaSharpTextBackend>? logger = null)
    {
        _logger = logger;
    }

    public Task ConfigureAsync(BackendConfiguration config)
    {
        foreach (var dir in config.SearchDirectories)
        {
            if (Directory.Exists(dir))
                NativeLibraryConfig.All.WithSearchDirectory(dir);
        }

        if (config.LogCallback is not null)
        {
            NativeLibraryConfig.All.WithLogCallback((level, message) =>
                config.LogCallback(level.ToString(), message));
        }
        else if (config.ShowLogs && _logger is not null)
        {
            NativeLibraryConfig.All.WithLogCallback(_logger);
        }

        switch (config.Runtime)
        {
            case "Avx":
                NativeLibraryConfig.All.WithAvx(AvxLevel.Avx);
                NativeLibraryConfig.All.WithCuda(false);
                NativeLibraryConfig.All.WithVulkan(false);
                break;
            case "Avx2":
                NativeLibraryConfig.All.WithAvx(AvxLevel.Avx2);
                NativeLibraryConfig.All.WithCuda(false);
                NativeLibraryConfig.All.WithVulkan(false);
                break;
            case "Avx512":
                NativeLibraryConfig.All.WithAvx(AvxLevel.Avx512);
                NativeLibraryConfig.All.WithCuda(false);
                NativeLibraryConfig.All.WithVulkan(false);
                break;
            case "Cuda":
                NativeLibraryConfig.All.WithAvx(AvxLevel.None);
                NativeLibraryConfig.All.WithCuda(true);
                NativeLibraryConfig.All.WithVulkan(false);
                break;
            case "Vulkan":
                NativeLibraryConfig.All.WithAvx(AvxLevel.None);
                NativeLibraryConfig.All.WithCuda(false);
                NativeLibraryConfig.All.WithVulkan(true);
                break;
            // "Auto" — leave defaults
        }

        if (!string.IsNullOrWhiteSpace(config.LibraryPath) || !string.IsNullOrWhiteSpace(config.SecondaryLibraryPath))
            NativeLibraryConfig.All.WithLibrary(config.LibraryPath, config.SecondaryLibraryPath);

        NativeLibraryConfig.All.WithAutoFallback(config.AutoFallback);

        if (!config.AutoFallback && config.SkipCheck)
            NativeLibraryConfig.All.SkipCheck();

        return Task.CompletedTask;
    }

    public Task<IModelHandle> LoadModelAsync(ModelLoadRequest request)
    {
        var parameters = new ModelParams(request.FilePath)
        {
            ContextSize = request.ContextSize,
            GpuLayerCount = request.GpuLayerCount,
            BatchSize = request.BatchSize
        };

        var weights = LLamaWeights.LoadFromFile(parameters);

        IModelHandle handle = new LlamaSharpModelHandle(request.ModelId, request.FilePath, weights, parameters);
        return Task.FromResult(handle);
    }

    public void UnloadModel(IModelHandle handle)
    {
        handle.Dispose();
    }

    public async Task<IChatSession> CreateChatSessionAsync(IModelHandle handle, string? systemPrompt = null)
    {
        if (handle is not LlamaSharpModelHandle llamaHandle || !llamaHandle.IsLoaded)
            throw new InvalidOperationException("Invalid or unloaded model handle.");

        var context = llamaHandle.Weights!.CreateContext(llamaHandle.Parameters);
        var executor = new InteractiveExecutor(context);
        var history = new ChatHistory();

        // Try to use the GGUF-embedded chat template for proper role token formatting.
        // Models like GLM-4/CodeGeeX4 require <|system|>/<|user|>/<|assistant|> tokens.
        var hasTemplate = HasChatTemplate(llamaHandle.Weights!);

        if (hasTemplate)
        {
            // Add system prompt to history — the template transform will format it
            if (systemPrompt is not null)
                history.AddMessage(AuthorRole.System, systemPrompt);

            var session = new LLama.ChatSession(executor, history);
            session.WithHistoryTransform(new PromptTemplateTransformer(llamaHandle.Weights!, true));
            return new LlamaSharpChatSession(session, context);
        }
        else
        {
            // No embedded template — use PrefillPromptAsync for raw text injection
            try
            {
                if (systemPrompt is not null)
                    await executor.PrefillPromptAsync(systemPrompt);
            }
            catch
            {
                if (systemPrompt is not null)
                    history.AddMessage(AuthorRole.System, systemPrompt);
            }

            var session = new LLama.ChatSession(executor, history);
            return new LlamaSharpChatSession(session, context);
        }
    }

    /// <summary>
    /// Check if the model has an embedded chat template in its GGUF metadata.
    /// </summary>
    private static bool HasChatTemplate(LLamaWeights weights)
    {
        try
        {
            // LLamaTemplate with strict=true throws if no template is found
            _ = new LLamaTemplate(weights, true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
