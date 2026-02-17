namespace Ouroboros.Domain.Governance;

/// <summary>
/// Defines an approval gate that requires human approval.
/// </summary>
public sealed record ApprovalGate
{
    /// <summary>
    /// Gets the gate identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the gate name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the condition that triggers this approval gate.
    /// </summary>
    public required string Condition { get; init; }

    /// <summary>
    /// Gets the required approvers (roles or user IDs).
    /// </summary>
    public IReadOnlyList<string> RequiredApprovers { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the minimum number of approvals required.
    /// </summary>
    public int MinimumApprovals { get; init; } = 1;

    /// <summary>
    /// Gets the timeout for approval requests.
    /// </summary>
    public TimeSpan ApprovalTimeout { get; init; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Gets the action to take if approval times out.
    /// </summary>
    public ApprovalTimeoutAction TimeoutAction { get; init; } = ApprovalTimeoutAction.Block;
}