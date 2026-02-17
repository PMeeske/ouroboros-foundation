namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Result of a safety check evaluation.
/// </summary>
/// <param name="IsAllowed">Whether the action is allowed.</param>
/// <param name="Reason">Explanation for the decision.</param>
/// <param name="RequiredPermissions">Permissions that were evaluated.</param>
/// <param name="RiskScore">Risk assessment score (0.0 to 1.0).</param>
/// <param name="Violations">List of safety violations found.</param>
public sealed record SafetyCheckResult(
    bool IsAllowed,
    string Reason,
    IReadOnlyList<Permission> RequiredPermissions,
    double RiskScore,
    IReadOnlyList<string> Violations)
{
    /// <summary>
    /// Gets whether the action is safe (alias for IsAllowed).
    /// </summary>
    public bool Safe => IsAllowed;

    /// <summary>
    /// Gets any warnings (empty list, for compatibility).
    /// </summary>
    public IReadOnlyList<string> Warnings => Array.Empty<string>();

    /// <summary>
    /// Gets the required permission level (first permission's level or None).
    /// </summary>
    public PermissionLevel RequiredLevel => RequiredPermissions.Count > 0
        ? RequiredPermissions[0].Level
        : PermissionLevel.None;

    /// <summary>
    /// Creates a safe result with no violations.
    /// </summary>
    /// <param name="reason">Reason for allowing the action.</param>
    /// <returns>A safe result.</returns>
    public static SafetyCheckResult Allowed(string reason = "Action is safe") =>
        new(true, reason, Array.Empty<Permission>(), 0.0, Array.Empty<string>());

    /// <summary>
    /// Creates an unsafe result with violations.
    /// </summary>
    /// <param name="reason">Reason for denying the action.</param>
    /// <param name="violations">List of violations.</param>
    /// <param name="riskScore">Risk score.</param>
    /// <returns>An unsafe result.</returns>
    public static SafetyCheckResult Denied(string reason, IReadOnlyList<string> violations, double riskScore = 1.0) =>
        new(false, reason, Array.Empty<Permission>(), riskScore, violations);
}