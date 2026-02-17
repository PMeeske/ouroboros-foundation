namespace Ouroboros.Core.Ethics;

/// <summary>
/// Minimal representation of a Plan for ethics evaluation.
/// This is a lightweight version to avoid circular dependencies.
/// </summary>
public sealed record Plan
{
    /// <summary>
    /// Gets the goal this plan aims to achieve.
    /// </summary>
    public required string Goal { get; init; }

    /// <summary>
    /// Gets the steps in this plan.
    /// </summary>
    public required IReadOnlyList<PlanStep> Steps { get; init; }

    /// <summary>
    /// Gets the confidence scores for various aspects of the plan.
    /// </summary>
    public IReadOnlyDictionary<string, double> ConfidenceScores { get; init; } =
        new Dictionary<string, double>();

    /// <summary>
    /// Gets when this plan was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}