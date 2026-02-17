namespace Ouroboros.Core.Synthesis;

/// <summary>
/// Represents an execution trace of a program.
/// </summary>
/// <param name="Steps">The execution steps taken during program execution.</param>
/// <param name="FinalResult">The final result produced by the program.</param>
/// <param name="Duration">The time taken to execute the program.</param>
public sealed record ExecutionTrace(
    List<ExecutionStep> Steps,
    object FinalResult,
    TimeSpan Duration);