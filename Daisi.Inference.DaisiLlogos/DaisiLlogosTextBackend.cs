using Daisi.Inference.Interfaces;
using Daisi.Inference.Models;
using Daisi.Llogos;
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
public class DaisiLlogosTextBackend : ITextInferenceBackend
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
            var deltaState = new DeltaNetState(computeBackend, config);
            var forward = new ForwardPass(computeBackend, config, weights, kvCache, deltaState);
            var generator = new TextGenerator(forward, tokenizer);

            handle = new DaisiLlogosModelHandle(
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
        _logger?.LogInformation("[DaisiLlogos] CreateChatSessionAsync: handle type={Type}, isLoaded={Loaded}", handle?.GetType().Name, (handle as DaisiLlogosModelHandle)?.IsLoaded);
        if (handle is not DaisiLlogosModelHandle llogosHandle || !llogosHandle.IsLoaded)
            throw new InvalidOperationException("Invalid or unloaded model handle.");

        // Read chat template from GGUF metadata if available
        string? chatTemplate = null;
        using (var stream = File.OpenRead(llogosHandle.FilePath))
        {
            var gguf = GgufFile.Read(stream);
            chatTemplate = gguf.GetMetadataString("tokenizer.chat_template");
        }

        IChatSession session = new DaisiLlogosChatSession(llogosHandle, chatTemplate, systemPrompt, _logger);
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
