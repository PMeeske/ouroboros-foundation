// <copyright file="ISafetyGuard.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Abstractions.Agent;

/// <summary>
/// Defines permission levels for agent actions.
/// </summary>
public enum PermissionLevel
{
    /// <summary>No permission granted.</summary>
    None = 0,

    /// <summary>Read-only permission.</summary>
    Read = 1,

    /// <summary>Write permission (includes Read).</summary>
    Write = 2,

    /// <summary>Execute permission.</summary>
    Execute = 3,

    /// <summary>Full administrative access.</summary>
    Admin = 4,
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
public sealed record SafetyCheckResult(
    bool IsAllowed,
    string Reason,
    IReadOnlyList<Permission> RequiredPermissions,
    double RiskScore);

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
