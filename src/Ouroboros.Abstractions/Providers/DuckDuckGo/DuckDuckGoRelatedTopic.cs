using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Providers.DuckDuckGo;

/// <summary>
/// A related topic from DuckDuckGo instant answers.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record DuckDuckGoRelatedTopic
{
    /// <summary>Gets the topic text.</summary>
    public string? Text { get; init; }

    /// <summary>Gets the topic URL.</summary>
    public string? Url { get; init; }
}
