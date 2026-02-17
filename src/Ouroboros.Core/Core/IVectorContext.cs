namespace Ouroboros.Core;

/// <summary>
/// Represents a context with vector store capabilities.
/// </summary>
[Obsolete("No implementations exist. Scheduled for removal.")]
public interface IVectorContext
{
    /// <summary>
    /// Gets the vector store associated with this context.
    /// </summary>
    object? VectorStore { get; }
}