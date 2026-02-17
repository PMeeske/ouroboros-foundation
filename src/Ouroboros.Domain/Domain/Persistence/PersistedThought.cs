namespace Ouroboros.Domain.Persistence;

/// <summary>
/// A thought record suitable for persistence.
/// </summary>
public sealed record PersistedThought
{
    /// <summary>Unique identifier for the thought.</summary>
    public required Guid Id { get; init; }

    /// <summary>Type of thought (Observation, Analytical, Curiosity, etc.).</summary>
    public required string Type { get; init; }

    /// <summary>The content/text of the thought.</summary>
    public required string Content { get; init; }

    /// <summary>Confidence level (0-1).</summary>
    public double Confidence { get; init; }

    /// <summary>Relevance to current context (0-1).</summary>
    public double Relevance { get; init; }

    /// <summary>When the thought occurred.</summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>Origin of the thought (Reactive, Autonomous, Chained).</summary>
    public string Origin { get; init; } = "Reactive";

    /// <summary>Priority level.</summary>
    public string Priority { get; init; } = "Normal";

    /// <summary>Parent thought ID for chained thoughts.</summary>
    public Guid? ParentThoughtId { get; init; }

    /// <summary>Personality trait that triggered this thought.</summary>
    public string? TriggeringTrait { get; init; }

    /// <summary>Associated topic/context.</summary>
    public string? Topic { get; init; }

    /// <summary>Tags for categorization.</summary>
    public string[]? Tags { get; init; }

    /// <summary>Additional metadata as JSON.</summary>
    public string? MetadataJson { get; init; }
}