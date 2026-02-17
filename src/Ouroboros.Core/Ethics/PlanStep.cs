namespace Ouroboros.Core.Ethics;

/// <summary>
/// Minimal representation of a plan step for ethics evaluation.
/// </summary>
public sealed record PlanStep
{
    /// <summary>
    /// Gets the action to be performed.
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// Gets the parameters for this action.
    /// </summary>
    public required IReadOnlyDictionary<string, object> Parameters { get; init; }

    /// <summary>
    /// Gets the expected outcome of this step.
    /// </summary>
    public required string ExpectedOutcome { get; init; }

    /// <summary>
    /// Gets the confidence score for this step (0.0 to 1.0).
    /// </summary>
    public double ConfidenceScore { get; init; } = 1.0;
}