#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// ==========================================================
// Homeostasis Policy Interface
// Phase 3: Affective Dynamics - SLA regulation & corrective triggers
// ==========================================================

namespace Ouroboros.Agent.MetaAI.Affect;

/// <summary>
/// Interface for homeostasis policy management.
/// Enforces SLA boundaries and triggers corrective actions.
/// </summary>
public interface IHomeostasisPolicy
{
    /// <summary>
    /// Adds a new homeostasis rule.
    /// </summary>
    /// <param name="name">Rule name</param>
    /// <param name="description">Rule description</param>
    /// <param name="targetSignal">Signal type to monitor</param>
    /// <param name="lowerBound">Lower acceptable bound</param>
    /// <param name="upperBound">Upper acceptable bound</param>
    /// <param name="targetValue">Target equilibrium value</param>
    /// <param name="action">Action to take on violation</param>
    /// <param name="priority">Rule priority (higher = more important)</param>
    /// <returns>The created rule</returns>
    HomeostasisRule AddRule(
        string name,
        string description,
        SignalType targetSignal,
        double lowerBound,
        double upperBound,
        double targetValue,
        HomeostasisAction action,
        double priority = 1.0);

    /// <summary>
    /// Updates an existing rule.
    /// </summary>
    /// <param name="ruleId">Rule ID</param>
    /// <param name="lowerBound">New lower bound</param>
    /// <param name="upperBound">New upper bound</param>
    /// <param name="targetValue">New target value</param>
    void UpdateRule(Guid ruleId, double? lowerBound = null, double? upperBound = null, double? targetValue = null);

    /// <summary>
    /// Enables or disables a rule.
    /// </summary>
    /// <param name="ruleId">Rule ID</param>
    /// <param name="isActive">Whether the rule should be active</param>
    void SetRuleActive(Guid ruleId, bool isActive);

    /// <summary>
    /// Gets all rules.
    /// </summary>
    /// <param name="activeOnly">Only return active rules</param>
    /// <returns>List of rules</returns>
    List<HomeostasisRule> GetRules(bool activeOnly = true);

    /// <summary>
    /// Evaluates current state against all active policies.
    /// </summary>
    /// <param name="state">Current affective state</param>
    /// <returns>List of policy violations</returns>
    List<PolicyViolation> EvaluatePolicies(AffectiveState state);

    /// <summary>
    /// Applies corrective action for a violation.
    /// </summary>
    /// <param name="violation">The policy violation</param>
    /// <param name="monitor">The valence monitor to apply corrections to</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Correction result</returns>
    Task<CorrectionResult> ApplyCorrectionAsync(
        PolicyViolation violation,
        IValenceMonitor monitor,
        CancellationToken ct = default);

    /// <summary>
    /// Gets violation history.
    /// </summary>
    /// <param name="count">Number of violations to retrieve</param>
    /// <returns>List of recent violations</returns>
    List<PolicyViolation> GetViolationHistory(int count = 50);

    /// <summary>
    /// Gets correction history.
    /// </summary>
    /// <param name="count">Number of corrections to retrieve</param>
    /// <returns>List of recent corrections</returns>
    List<CorrectionResult> GetCorrectionHistory(int count = 50);

    /// <summary>
    /// Registers a custom correction handler.
    /// </summary>
    /// <param name="name">Handler name</param>
    /// <param name="handler">Handler function</param>
    void RegisterCustomHandler(string name, Func<PolicyViolation, IValenceMonitor, Task<CorrectionResult>> handler);

    /// <summary>
    /// Gets policy health summary.
    /// </summary>
    /// <returns>Policy health metrics</returns>
    PolicyHealthSummary GetHealthSummary();
}