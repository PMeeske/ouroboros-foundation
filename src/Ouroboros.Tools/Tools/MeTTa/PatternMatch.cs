using Ouroboros.Core.Hyperon;

namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Represents a pattern match result.
/// </summary>
public class PatternMatch
{
    /// <summary>
    /// Gets or sets the pattern that was matched.
    /// </summary>
    public required string Pattern { get; set; }

    /// <summary>
    /// Gets or sets the subscription that triggered this match.
    /// </summary>
    public required string SubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the bindings from the match.
    /// </summary>
    public required Substitution Bindings { get; set; }

    /// <summary>
    /// Gets or sets the matched atoms.
    /// </summary>
    public IReadOnlyList<Atom> MatchedAtoms { get; set; } = Array.Empty<Atom>();

    /// <summary>
    /// Gets or sets the timestamp of the match.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}