// <copyright file="SafetyGuardExtensions.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Agent;

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
        // Intentional: sync wrapper for non-async callers
        return Task.Run(() => guard.CheckSafetyAsync(action, permissionLevel)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Sandboxes a step synchronously (legacy method).
    /// </summary>
    public static PlanStep SandboxStep(this ISafetyGuard guard, PlanStep step)
    {
        ArgumentNullException.ThrowIfNull(guard);
        ArgumentNullException.ThrowIfNull(step);

        // Intentional: sync wrapper for non-async callers
        var result = Task.Run(() => guard.SandboxStepAsync(step)).GetAwaiter().GetResult();
        return result.Success && result.SandboxedStep != null
            ? result.SandboxedStep
            : step;
    }
}