// <copyright file="SafetyGuardExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Extension methods for ISafetyGuard providing backward compatibility.
/// </summary>
public static class SafetyGuardExtensions
{
    /// <summary>
    /// Checks safety of an action synchronously (legacy method).
    /// </summary>
    public static SafetyCheckResult CheckSafety(
        this ISafetyGuard guard,
        string action,
        PermissionLevel permissionLevel)
    {
        ArgumentNullException.ThrowIfNull(guard);
        return guard.CheckSafetyAsync(action, permissionLevel).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Sandboxes a step synchronously (legacy method).
    /// </summary>
    public static PlanStep SandboxStep(this ISafetyGuard guard, PlanStep step)
    {
        ArgumentNullException.ThrowIfNull(guard);
        ArgumentNullException.ThrowIfNull(step);

        var result = guard.SandboxStepAsync(step).GetAwaiter().GetResult();
        return result.Success && result.SandboxedStep != null
            ? result.SandboxedStep
            : step;
    }
}