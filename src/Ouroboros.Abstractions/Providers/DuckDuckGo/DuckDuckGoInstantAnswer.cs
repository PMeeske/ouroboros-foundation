namespace Ouroboros.Providers.DuckDuckGo;

/// <summary>
/// A DuckDuckGo instant answer (from the Instant Answer API).
/// </summary>
public sealed record DuckDuckGoInstantAnswer
{
    /// <summary>Gets the heading/topic.</summary>
    public string? Heading { get; init; }

    /// <summary>Gets the abstract text.</summary>
    public string? AbstractText { get; init; }

    /// <summary>Gets the abstract source.</summary>
    public string? AbstractSource { get; init; }

    /// <summary>Gets the abstract URL.</summary>
    public string? AbstractUrl { get; init; }

    /// <summary>Gets the image URL.</summary>
    public string? ImageUrl { get; init; }

    /// <summary>Gets the answer text (direct answer).</summary>
    public string? Answer { get; init; }

    /// <summary>Gets the answer type (e.g., "calc", "ip", etc.).</summary>
    public string? AnswerType { get; init; }

    /// <summary>Gets the definition text.</summary>
    public string? Definition { get; init; }

    /// <summary>Gets related topics.</summary>
    public IReadOnlyList<DuckDuckGoRelatedTopic> RelatedTopics { get; init; } = [];
}