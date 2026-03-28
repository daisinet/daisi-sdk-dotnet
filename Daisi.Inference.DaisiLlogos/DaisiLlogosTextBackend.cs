using Daisi.Inference.Interfaces;
using Daisi.Inference.Models;
using Daisi.Llogos;
using Daisi.Llogos.Chat;
using Daisi.Llogos.Cpu;
using Daisi.Llogos.Cuda;
using Daisi.Llogos.Gguf;
using Daisi.Llogos.Inference;
using Daisi.Llogos.Model;
using Daisi.Llogos.Tokenizer;
using Daisi.Llogos.Vulkan;
using Microsoft.Extensions.Logging;

namespace Daisi.Inference.DaisiLlogos;

/// <summary>
/// Daisi Llogos implementation of ITextInferenceBackend.
/// Uses the pure C# daisi-llogos inference engine with CPU, CUDA, and Vulkan compute backends.
/// </summary>
public class DaisiLlogosTextBackend : ITextInferenceBackend, IPipelineBackend
{
    private readonly ILogger? _logger;
    private string _runtime = "Auto";
    private Action<string, string>? _logCallback;

    public string BackendName => "DaisiLlogos";

    public DaisiLlogosTextBackend(ILogger<DaisiLlogosTextBackend>? logger = null)
    {
        _logger = logger;
    }

    public Task ConfigureAsync(BackendConfiguration config)
    {
        _runtime = config.Runtime;
        _logCallback = config.LogCallback;
        return Task.CompletedTask;
    }

    public Task<IModelHandle> LoadModelAsync(ModelLoadRequest request)
    {
        _logger?.LogInformation("[DaisiLlogos] LoadModelAsync: {ModelId}, file={File}, ctx={Ctx}", request.ModelId, request.FilePath, request.ContextSize);
        var computeBackend = CreateComputeBackend();

        using var stream = File.OpenRead(request.FilePath);
        var gguf = GgufFile.Read(stream);
        var config = ModelConfig.FromGguf(gguf);
        var tokenizer = TokenizerFactory.FromGguf(gguf);

        int maxContext = (int)request.ContextSize;
        bool isBitNet = config.Architecture.StartsWith("bitnet", StringComparison.OrdinalIgnoreCase);

        DaisiLlogosModelHandle handle;

        if (isBitNet)
        {
            var weights = BitNetModelLoader.Load(gguf, stream, computeBackend, config);
            var kvCache = new BitNetKvCache(computeBackend, config, maxSeqLen: maxContext);
            var forward = new BitNetForwardPass(computeBackend, config, weights, kvCache);
            var generator = new BitNetTextGenerator(forward, tokenizer);

            handle = new DaisiLlogosModelHandle(
                request.ModelId, request.FilePath, computeBackend,
                config, tokenizer, generator, weights, kvCache, forward);
        }
        else
        {
            var weights = MmapModelLoader.Load(gguf, request.FilePath, computeBackend, config);
            var kvCache = new KvCache(computeBackend, config, maxSeqLen: maxContext);
            var deltaState = new DeltaNetState(computeBackend, config, weights);
            var forward = new ForwardPass(computeBackend, config, weights, kvCache, deltaState);
            var generator = new TextGenerator(forward, tokenizer);

            handle = new DaisiLlogosModelHandle(
                request.ModelId, request.FilePath, computeBackend,
                config, tokenizer, generator, weights, kvCache, deltaState, forward);
        }

        Log("Model loaded", $"{config.Architecture}, {config.NumLayers} layers, {config.HiddenDim}d, ctx={maxContext}");
        return Task.FromResult<IModelHandle>(handle);
    }

    /// <summary>
    /// Load a pipeline stage — only the specified layer range (and optionally embedding/output head).
    /// Used by DaisiChain to distribute model layers across multiple hosts.
    /// </summary>
    public Task<IPipelineHandle> LoadPipelineStageAsync(
        ModelLoadRequest request, int startLayer, int endLayer,
        bool includeEmbedding, bool includeOutputHead)
    {
        _logger?.LogInformation("[DaisiLlogos] LoadPipelineStageAsync: {ModelId}, layers [{Start},{End}), embed={Embed}, head={Head}",
            request.ModelId, startLayer, endLayer, includeEmbedding, includeOutputHead);

        var computeBackend = CreateComputeBackend();

        using var stream = File.OpenRead(request.FilePath);
        var gguf = GgufFile.Read(stream);
        var config = ModelConfig.FromGguf(gguf);

        // Only load tokenizer for the first stage (embedding) or last stage (output head)
        BpeTokenizer? tokenizer = (includeEmbedding || includeOutputHead)
            ? TokenizerFactory.FromGguf(gguf)
            : null;

        int maxContext = (int)request.ContextSize;

        var weights = MmapModelLoader.LoadPartial(gguf, request.FilePath, computeBackend, config,
            startLayer, endLayer, includeEmbedding, includeOutputHead);
        var kvCache = new KvCache(computeBackend, config, maxSeqLen: maxContext,
            startLayer: startLayer, endLayer: endLayer);
        var deltaState = new DeltaNetState(computeBackend, config, weights,
            startLayer: startLayer, endLayer: endLayer);
        var forward = new ForwardPass(computeBackend, config, weights, kvCache, deltaState);

        var handle = new DaisiLlogosPipelineHandle(
            request.ModelId, request.FilePath, computeBackend,
            config, weights, kvCache, deltaState, forward,
            startLayer, endLayer, includeEmbedding, includeOutputHead, tokenizer);

        Log("Pipeline stage loaded", $"{config.Architecture}, layers [{startLayer},{endLayer}), embed={includeEmbedding}, head={includeOutputHead}");
        return Task.FromResult<IPipelineHandle>(handle);
    }

    public void UnloadModel(IModelHandle handle)
    {
        handle.Dispose();
    }

    public Task<IChatSession> CreateChatSessionAsync(IModelHandle handle, string? systemPrompt = null)
    {
        _logger?.LogInformation("[DaisiLlogos] CreateChatSessionAsync: handle type={Type}, isLoaded={Loaded}", handle?.GetType().Name, (handle as DaisiLlogosModelHandle)?.IsLoaded);
        if (handle is not DaisiLlogosModelHandle llogosHandle || !llogosHandle.IsLoaded)
            throw new InvalidOperationException("Invalid or unloaded model handle.");

        // Read chat template from GGUF metadata
        ChatTemplate chatTemplate;
        using (var stream = File.OpenRead(llogosHandle.FilePath))
        {
            var gguf = GgufFile.Read(stream);
            chatTemplate = ChatTemplate.FromGguf(gguf);
        }

        // Use the LLogos core chat session — provides proper ChatML formatting,
        // <|im_end|> stop sequences, KV cache, grammar constraints, and AntiPrompt support.
        var coreSession = llogosHandle.CreateCoreChatSession(chatTemplate, systemPrompt);
        _logger?.LogInformation("[DaisiLlogos] Created core chat session with {Format} template", chatTemplate.Format);

        IChatSession session = new DaisiLlogosCoreSessionAdapter(coreSession);
        return Task.FromResult(session);
    }

    private IComputeBackend CreateComputeBackend()
    {
        if (_runtime == "Auto")
        {
            // Try CUDA first, then Vulkan, then CPU
            try
            {
                var cuda = new CudaBackend();
                _logger?.LogInformation("[DaisiLlogos] Auto-detected CUDA backend");
                return cuda;
            }
            catch
            {
                try
                {
                    var vulkan = new VulkanBackend();
                    _logger?.LogInformation("[DaisiLlogos] Auto-detected Vulkan backend");
                    return vulkan;
                }
                catch
                {
                    _logger?.LogInformation("[DaisiLlogos] Falling back to CPU backend");
                    return new CpuBackend();
                }
            }
        }

        return _runtime switch
        {
            "Cuda" => new CudaBackend(),
            "Vulkan" => new VulkanBackend(),
            _ => new CpuBackend(),
        };
    }

    private void Log(string level, string message)
    {
        if (_logCallback is not null)
            _logCallback(level, message);
        else
            _logger?.LogInformation("[DaisiLlogos] {Message}", message);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
