namespace Daisi.Inference.Interfaces;

/// <summary>
/// Handle to a loaded model. Disposing unloads the model from memory.
/// </summary>
public interface IModelHandle : IDisposable
{
    string ModelId { get; }
    string FilePath { get; }
    bool IsLoaded { get; }
}
