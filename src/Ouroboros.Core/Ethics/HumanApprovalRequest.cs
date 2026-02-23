namespace Ouroboros.Core.Ethics;

/// <summary>
/// Encapsulates a request for human approval of an ethically flagged action or plan.
/// </summary>
public sealed record HumanApprovalRequest
{
    /// <summary>
    /// Gets the unique identifier for this approval request.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the category of the item requiring approval (e.g., "plan", "action", "self-modification").
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Gets a human-readable description of what needs approval.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the ethical clearance that triggered the approval requirement.
    /// </summary>
    public required EthicalClearance Clearance { get; init; }

    /// <summary>
    /// Gets additional context for the reviewer to make an informed decision.
    /// </summary>
    public IReadOnlyDictionary<string, object> Context { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets the maximum time to wait for a response before timing out.
    /// Defaults to 5 minutes. Null means wait indefinitely.
    /// </summary>
    public TimeSpan? Timeout { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets the timestamp when this request was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
