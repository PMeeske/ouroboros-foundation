// <copyright file="ISafetyGuard.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Agent.MetaAI;

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
