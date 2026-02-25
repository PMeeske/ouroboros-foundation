namespace Ouroboros.Domain.Persistence;

/// <summary>
/// Statistics about thoughts in a session.
/// </summary>
public sealed record ThoughtStatistics
{
    /// <summary>Total number of thoughts.</summary>
    public int TotalCount { get; init; }

    /// <summary>Count by thought type.</summary>
    public Dictionary<string, int> CountByType { get; init; } = new();

    /// <summary>Count by origin.</summary>
    public Dictionary<string, int> CountByOrigin { get; init; } = new();

    /// <summary>Average confidence.</summary>
    public double AverageConfidence { get; init; }

    /// <summary>Average relevance.</summary>
    public double AverageRelevance { get; init; }

    /// <summary>Time range of thoughts.</summary>
    public DateTime? EarliestThought { get; init; }

    /// <summary>Time range of thoughts.</summary>
    public DateTime? LatestThought { get; init; }

    /// <summary>Number of thought chains.</summary>
    public int ChainCount { get; init; }
}