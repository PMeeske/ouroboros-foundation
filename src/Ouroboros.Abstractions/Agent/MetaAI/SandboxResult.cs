namespace Ouroboros.Agent.MetaAI;

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