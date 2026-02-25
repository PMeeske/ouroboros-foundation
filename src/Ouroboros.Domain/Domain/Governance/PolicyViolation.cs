namespace Ouroboros.Domain.Governance;

/// <summary>
/// Represents a policy violation.
/// </summary>
public sealed record PolicyViolation
{
    /// <summary>
    /// Gets the violation identifier.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the policy rule that was violated.
    /// </summary>
    public required PolicyRule Rule { get; init; }

    /// <summary>
    /// Gets the severity of the violation.
    /// </summary>
    public ViolationSeverity Severity { get; init; }

    /// <summary>
    /// Gets the violation message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Gets the actual value that violated the policy.
    /// </summary>
    public object? ActualValue { get; init; }

    /// <summary>
    /// Gets the expected value or range.
    /// </summary>
    public object? ExpectedValue { get; init; }

    /// <summary>
    /// Gets the recommended action to resolve the violation.
    /// </summary>
    public required PolicyAction RecommendedAction { get; init; }

    /// <summary>
    /// Gets the timestamp when the violation was detected.
    /// </summary>
    public DateTime DetectedAt { get; init; } = DateTime.UtcNow;
}