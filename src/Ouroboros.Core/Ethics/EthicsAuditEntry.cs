namespace Ouroboros.Core.Ethics;

/// <summary>
/// Represents an immutable audit entry for ethical evaluations.
/// </summary>
public sealed record EthicsAuditEntry
{
    /// <summary>
    /// Gets the unique identifier for this audit entry.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the timestamp when this evaluation occurred.
    /// </summary>
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets the agent ID that requested the evaluation.
    /// </summary>
    public required string AgentId { get; init; }

    /// <summary>
    /// Gets the user ID associated with this request (if applicable).
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the type of evaluation performed (e.g., "Action", "Plan", "Goal", "SelfModification").
    /// </summary>
    public required string EvaluationType { get; init; }

    /// <summary>
    /// Gets a brief description of what was evaluated.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the clearance decision that was made.
    /// </summary>
    public required EthicalClearance Clearance { get; init; }

    /// <summary>
    /// Gets additional context data for this audit entry.
    /// </summary>
    public IReadOnlyDictionary<string, object> Context { get; init; } = 
        new Dictionary<string, object>();
}