namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Result of plan execution.
/// </summary>
/// <param name="Plan">The plan that was executed.</param>

/// <param name="StepResults">The results of each step execution.</param>
/// <param name="Success">Whether execution was successful.</param>

/// <param name="FinalOutput">Final output or error message.</param>
/// <param name="Metadata">Additional metadata about execution.</param>
/// <param name="Duration">How long execution took.</param>
public sealed record PlanExecutionResult(
    Plan Plan,

    IReadOnlyList<StepResult> StepResults,
    bool Success,

    string? FinalOutput,
    IReadOnlyDictionary<string, object> Metadata,
    TimeSpan Duration);