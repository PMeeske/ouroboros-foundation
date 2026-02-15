// <copyright file="ISafetyGuard.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Defines permission levels for agent actions.
/// </summary>
public enum PermissionLevel
{
    /// <summary>No permission granted.</summary>
    None = 0,

    /// <summary>Isolated execution with no external access.</summary>
    Isolated = 1,

    /// <summary>Read-only permission.</summary>
    Read = 2,

    /// <summary>Read-only permission (alias for Read).</summary>
    ReadOnly = 2,

    /// <summary>Write permission (includes Read).</summary>
    Write = 3,

    /// <summary>Execute permission.</summary>
    Execute = 4,

    /// <summary>User data access with confirmation required.</summary>
    UserDataWithConfirmation = 5,

    /// <summary>Full administrative access.</summary>
    Admin = 6,
}

/// <summary>
/// Represents a permission for an agent action.
/// </summary>
/// <param name="Resource">The resource being accessed.</param>
/// <param name="Level">The permission level required.</param>
/// <param name="Reason">Reason for the permission request.</param>
public sealed record Permission(
    string Resource,
    PermissionLevel Level,
    string Reason);

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

/// <summary>
/// Result of sandboxing a step for safe execution.
/// </summary>
/// <param name="Success">Whether sandboxing was successful.</param>
/// <param name="SandboxedStep">The sandboxed step ready for execution.</param>
/// <param name="Restrictions">Restrictions applied to the step.</param>
/// <param name="Error">Error message if sandboxing failed.</param>
public sealed record SandboxResult(
    bool Success,
    PlanStep? SandboxedStep,
    IReadOnlyList<string> Restrictions,
    string? Error);

/// <summary>
/// Interface for safety guard that validates agent actions before execution.
/// Ensures actions comply with safety policies and ethical constraints.
/// </summary>
public interface ISafetyGuard
{
    /// <summary>
    /// Checks if an action is safe to execute.
    /// </summary>
    /// <param name="actionName">Name of the action to check.</param>
    /// <param name="parameters">Parameters for the action.</param>
    /// <param name="context">Execution context.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Safety check result indicating if action is allowed.</returns>
    Task<SafetyCheckResult> CheckActionSafetyAsync(
        string actionName,
        IReadOnlyDictionary<string, object> parameters,
        object? context = null,
        CancellationToken ct = default);

    /// <summary>
    /// Checks safety of an action (simplified async overload).
    /// </summary>
    /// <param name="action">The action description.</param>
    /// <param name="permissionLevel">Required permission level.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Safety check result.</returns>
    Task<SafetyCheckResult> CheckSafetyAsync(
        string action,
        PermissionLevel permissionLevel,
        CancellationToken ct = default);

    /// <summary>
    /// Checks safety of an action (sync convenience method).
    /// </summary>
    /// <param name="action">The action description.</param>
    /// <param name="parameters">Parameters for the action.</param>
    /// <param name="permissionLevel">Required permission level.</param>
    /// <returns>Safety check result.</returns>
    SafetyCheckResult CheckSafety(
        string action,
        Dictionary<string, object> parameters,
        PermissionLevel permissionLevel);

    /// <summary>
    /// Sandboxes a step for safe execution (async).
    /// </summary>
    /// <param name="step">The step to sandbox.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Sandbox result with the sandboxed step.</returns>
    Task<SandboxResult> SandboxStepAsync(
        PlanStep step,
        CancellationToken ct = default);

    /// <summary>
    /// Sandboxes a step for safe execution (sync convenience method).
    /// </summary>
    /// <param name="step">The step to sandbox.</param>
    /// <returns>The sandboxed step.</returns>
    PlanStep SandboxStep(PlanStep step);

    /// <summary>
    /// Checks if an agent has the required permissions.
    /// </summary>
    /// <param name="agentId">The agent identifier.</param>
    /// <param name="permissions">Permissions to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if all permissions are granted.</returns>
    Task<bool> CheckPermissionsAsync(
        string agentId,
        IReadOnlyList<Permission> permissions,
        CancellationToken ct = default);

    /// <summary>
    /// Assesses the risk level of an action.
    /// </summary>
    /// <param name="actionName">Name of the action.</param>
    /// <param name="parameters">Action parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Risk score from 0.0 (safe) to 1.0 (high risk).</returns>
    Task<double> AssessRiskAsync(
        string actionName,
        IReadOnlyDictionary<string, object> parameters,
        CancellationToken ct = default);
}
