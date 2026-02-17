namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Result of a single plan step execution.
/// </summary>

/// <param name="Step">The plan step that was executed.</param>
/// <param name="Success">Whether the step was successful.</param>
/// <param name="Output">Output from the step.</param>

/// <param name="Error">Error message if step failed.</param>
/// <param name="Duration">How long the step took to execute.</param>
/// <param name="ObservedState">State observed after execution.</param>
public sealed record StepResult(

    PlanStep Step,
    bool Success,
    string? Output,

    string? Error,
    TimeSpan Duration,
    IReadOnlyDictionary<string, object> ObservedState);