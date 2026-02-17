namespace Ouroboros.Core;

/// <summary>
/// Represents a context with LLM capabilities.
/// </summary>
[Obsolete("No implementations exist. Scheduled for removal.")]
public interface ILlmContext
{
    /// <summary>
    /// Gets the tool-aware chat model.
    /// </summary>
    object Llm { get; }

    /// <summary>
    /// Gets the tool registry.
    /// </summary>
    object Tools { get; }
}