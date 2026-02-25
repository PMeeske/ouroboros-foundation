namespace Ouroboros.Domain.Governance;

/// <summary>
/// Result of policy simulation.
/// </summary>
public sealed record PolicySimulationResult
{
    /// <summary>
    /// Gets the policy that was simulated.
    /// </summary>
    public required Policy Policy { get; init; }

    /// <summary>
    /// Gets the evaluation result.
    /// </summary>
    public required PolicyEvaluationResult EvaluationResult { get; init; }

    /// <summary>
    /// Gets a value indicating whether the operation would be blocked.
    /// </summary>
    public bool WouldBlock { get; init; }

    /// <summary>
    /// Gets the approval gates that would be required.
    /// </summary>
    public IReadOnlyList<ApprovalGate> RequiredApprovals { get; init; } = Array.Empty<ApprovalGate>();

    /// <summary>
    /// Gets the timestamp of the simulation.
    /// </summary>
    public DateTime SimulatedAt { get; init; } = DateTime.UtcNow;
}