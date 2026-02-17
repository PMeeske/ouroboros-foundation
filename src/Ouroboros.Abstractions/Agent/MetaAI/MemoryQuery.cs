namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Query parameters for retrieving experiences from memory.
/// </summary>
/// <param name="Tags">Filter by tags.</param>
/// <param name="ContextSimilarity">Context to find similar experiences.</param>
/// <param name="SuccessOnly">Whether to return only successful experiences.</param>
/// <param name="FromDate">Filter experiences from this date.</param>
/// <param name="ToDate">Filter experiences to this date.</param>
/// <param name="MaxResults">Maximum number of results to return.</param>
/// <param name="Goal">Goal to search for relevant experiences.</param>
/// <param name="MinSimilarity">Minimum similarity threshold (0.0 to 1.0).</param>
/// <param name="Context">Context for similarity search (alias for ContextSimilarity).</param>
public sealed record MemoryQuery(
    IReadOnlyList<string>? Tags = null,
    string? ContextSimilarity = null,
    bool? SuccessOnly = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int MaxResults = 100,
    string? Goal = null,
    double MinSimilarity = 0.0,
    string? Context = null);