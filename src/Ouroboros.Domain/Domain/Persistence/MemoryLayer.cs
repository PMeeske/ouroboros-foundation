namespace Ouroboros.Domain.Persistence;

/// <summary>
/// Memory layer types in Ouroboros's cognitive architecture.
/// </summary>
public enum MemoryLayer
{
    /// <summary>Immediate working memory - active thoughts.</summary>
    Working,

    /// <summary>Short-term episodic memory - recent conversations.</summary>
    Episodic,

    /// <summary>Long-term semantic memory - knowledge and facts.</summary>
    Semantic,

    /// <summary>Procedural memory - skills and tool patterns.</summary>
    Procedural,

    /// <summary>Self-referential memory - identity and self-model.</summary>
    Autobiographical,
}