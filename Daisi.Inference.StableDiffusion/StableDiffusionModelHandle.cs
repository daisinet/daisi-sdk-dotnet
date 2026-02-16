using Daisi.Inference.Interfaces;
using StableDiffusion.NET;

namespace Daisi.Inference.StableDiffusion;

/// <summary>
/// Wraps StableDiffusion.NET model context as an IModelHandle.
/// </summary>
public class StableDiffusionModelHandle : IModelHandle
{
    public string ModelId { get; }
    public string FilePath { get; }
    public bool IsLoaded => Model is not null;

    internal DiffusionModel? Model { get; private set; }

    public StableDiffusionModelHandle(string modelId, string filePath, DiffusionModel model)
    {
        ModelId = modelId;
        FilePath = filePath;
        Model = model;
    }

    public void Dispose()
    {
        Model?.Dispose();
        Model = null;
    }
}
