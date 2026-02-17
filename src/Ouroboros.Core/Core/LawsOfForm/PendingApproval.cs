namespace Ouroboros.Core.LawsOfForm;

/// <summary>
/// Represents a tool call pending human approval.
/// </summary>
public sealed record PendingApproval
{
    /// <summary>
    /// Gets the queue ID for this pending approval.
    /// </summary>
    public string QueueId { get; init; }

    /// <summary>
    /// Gets the tool call awaiting approval.
    /// </summary>
    public ToolCall Call { get; init; }

    /// <summary>
    /// Gets the original uncertain decision.
    /// </summary>
    public AuditableDecision<ToolResult> OriginalDecision { get; init; }

    /// <summary>
    /// Gets the timestamp when this was queued.
    /// </summary>
    public DateTime QueuedAt { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PendingApproval"/> class.
    /// </summary>
    /// <param name="queueId">The queue ID.</param>
    /// <param name="call">The tool call.</param>
    /// <param name="originalDecision">The original decision.</param>
    /// <param name="queuedAt">The queued timestamp.</param>
    public PendingApproval(
        string queueId,
        ToolCall call,
        AuditableDecision<ToolResult> originalDecision,
        DateTime queuedAt)
    {
        this.QueueId = queueId;
        this.Call = call;
        this.OriginalDecision = originalDecision;
        this.QueuedAt = queuedAt;
    }
}