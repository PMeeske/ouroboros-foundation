namespace Ouroboros.Domain.Governance;

/// <summary>
/// Interface for policy engine operations.
/// </summary>
public interface IPolicyEngine
{
    /// <summary>
    /// Registers a new policy.
    /// </summary>
    Result<Policy> RegisterPolicy(Policy policy);

    /// <summary>
    /// Updates an existing policy.
    /// </summary>
    Result<Policy> UpdatePolicy(Policy policy);

    /// <summary>
    /// Removes a policy.
    /// </summary>
    Result<bool> RemovePolicy(Guid policyId);

    /// <summary>
    /// Gets all policies.
    /// </summary>
    IReadOnlyList<Policy> GetPolicies(bool activeOnly = true);

    /// <summary>
    /// Evaluates all policies.
    /// </summary>
    Task<Result<IReadOnlyList<PolicyEvaluationResult>>> EvaluatePoliciesAsync(
        object context,
        CancellationToken ct = default);

    /// <summary>
    /// Evaluates a single policy.
    /// </summary>
    Task<Result<PolicyEvaluationResult>> EvaluatePolicyAsync(
        Policy policy,
        object context,
        CancellationToken ct = default);

    /// <summary>
    /// Simulates policy evaluation.
    /// </summary>
    Task<Result<PolicySimulationResult>> SimulatePolicyAsync(
        Policy policy,
        object context,
        CancellationToken ct = default);

    /// <summary>
    /// Enforces policies.
    /// </summary>
    Task<Result<PolicyEnforcementResult>> EnforcePoliciesAsync(
        object context,
        CancellationToken ct = default);

    /// <summary>
    /// Submits an approval.
    /// </summary>
    Result<ApprovalRequest> SubmitApproval(Guid requestId, Approval approval);

    /// <summary>
    /// Gets pending approvals.
    /// </summary>
    IReadOnlyList<ApprovalRequest> GetPendingApprovals();

    /// <summary>
    /// Gets audit trail.
    /// </summary>
    IReadOnlyList<PolicyAuditEntry> GetAuditTrail(int? limit = null, DateTime? since = null);

    /// <summary>
    /// Registers a custom condition evaluator.
    /// </summary>
    void RegisterConditionEvaluator(string name, Func<object, bool> evaluator);
}