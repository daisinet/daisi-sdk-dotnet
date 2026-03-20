using Daisi.Inference.Interfaces;
using Daisi.Inference.Models;
using Daisi.Llama;
using Daisi.Llama.Cpu;
using Daisi.Llama.Cuda;
using Daisi.Llama.Gguf;
using Daisi.Llama.Inference;
using Daisi.Llama.Model;
using Daisi.Llama.Tokenizer;
using Daisi.Llama.Vulkan;
using Microsoft.Extensions.Logging;

namespace Daisi.Inference.DaisiLlama;

/// <summary>
/// Daisi Llama implementation of ITextInferenceBackend.
/// Uses the pure C# daisi-llama inference engine with CPU, CUDA, and Vulkan compute backends.
/// </summary>
public class DaisiLlamaTextBackend : ITextInferenceBackend
{
    private readonly ILogger? _logger;
    private string _runtime = "Auto";
    private Action<string, string>? _logCallback;

    public string BackendName => "DaisiLlama";

    public DaisiLlamaTextBackend(ILogger<DaisiLlamaTextBackend>? logger = null)
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
        var computeBackend = CreateComputeBackend();

        using var stream = File.OpenRead(request.FilePath);
        var gguf = GgufFile.Read(stream);
        var config = ModelConfig.FromGguf(gguf);
        var tokenizer = TokenizerFactory.FromGguf(gguf);

        int maxContext = (int)request.ContextSize;
        bool isBitNet = config.Architecture.StartsWith("bitnet", StringComparison.OrdinalIgnoreCase);

        DaisiLlamaModelHandle handle;

        if (isBitNet)
        {
            var weights = BitNetModelLoader.Load(gguf, stream, computeBackend, config);
            var kvCache = new BitNetKvCache(computeBackend, config, maxSeqLen: maxContext);
            var forward = new BitNetForwardPass(computeBackend, config, weights, kvCache);
            var generator = new BitNetTextGenerator(forward, tokenizer);

            handle = new DaisiLlamaModelHandle(
                request.ModelId, request.FilePath, computeBackend,
                config, tokenizer, generator, weights, kvCache, forward);
        }
        else
        {
            var weights = MmapModelLoader.Load(gguf, request.FilePath, computeBackend, config);
            var kvCache = new KvCache(computeBackend, config, maxSeqLen: maxContext);
            var deltaState = new DeltaNetState(computeBackend, config);
            var forward = new ForwardPass(computeBackend, config, weights, kvCache, deltaState);
            var generator = new TextGenerator(forward, tokenizer);

            handle = new DaisiLlamaModelHandle(
                request.ModelId, request.FilePath, computeBackend,
                config, tokenizer, generator, weights, kvCache, deltaState, forward);
        }

        Log("Model loaded", $"{config.Architecture}, {config.NumLayers} layers, {config.HiddenDim}d, ctx={maxContext}");
        return Task.FromResult<IModelHandle>(handle);
    }

    public void UnloadModel(IModelHandle handle)
    {
        handle.Dispose();
    }

    public Task<IChatSession> CreateChatSessionAsync(IModelHandle handle, string? systemPrompt = null)
    {
        if (handle is not DaisiLlamaModelHandle llamaHandle || !llamaHandle.IsLoaded)
            throw new InvalidOperationException("Invalid or unloaded model handle.");

        // Read chat template from GGUF metadata if available
        string? chatTemplate = null;
        using (var stream = File.OpenRead(llamaHandle.FilePath))
        {
            var gguf = GgufFile.Read(stream);
            chatTemplate = gguf.GetMetadataString("tokenizer.chat_template");
        }

        IChatSession session = new DaisiLlamaChatSession(llamaHandle, chatTemplate, systemPrompt);
        return Task.FromResult(session);
    }

    private IComputeBackend CreateComputeBackend()
    {
        return _runtime switch
        {
            "Cuda" => new CudaBackend(),
            "Vulkan" => new VulkanBackend(),
            "Avx" or "Avx2" or "Avx512" or "Auto" => new CpuBackend(),
            _ => new CpuBackend(),
        };
    }

    private void Log(string level, string message)
    {
        if (_logCallback is not null)
            _logCallback(level, message);
        else
            _logger?.LogInformation("[DaisiLlama] {Message}", message);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
