namespace Ouroboros.Domain.Governance;

/// <summary>
/// Defines a single policy rule.
/// </summary>
public sealed record PolicyRule
{
    /// <summary>
    /// Gets the rule identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the rule name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the condition that triggers this rule.
    /// </summary>
    public required string Condition { get; init; }

    /// <summary>
    /// Gets the action to take when the rule is triggered.
    /// </summary>
    public required PolicyAction Action { get; init; }

    /// <summary>
    /// Gets additional metadata for the rule.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();
}