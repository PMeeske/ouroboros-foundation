namespace Ouroboros.Core.Ethics;

/// <summary>
/// Minimal representation of a Skill for ethics evaluation.
/// This is a lightweight version to avoid circular dependencies.
/// </summary>
public sealed record Skill
{
    /// <summary>
    /// Gets the name of the skill.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the description of what this skill does.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the prerequisites for using this skill.
    /// </summary>
    public IReadOnlyList<string> Prerequisites { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the steps involved in executing this skill.
    /// </summary>
    public IReadOnlyList<PlanStep> Steps { get; init; } = Array.Empty<PlanStep>();

    /// <summary>
    /// Gets the historical success rate of this skill (0.0 to 1.0).
    /// </summary>
    public double SuccessRate { get; init; } = 0.0;

    /// <summary>
    /// Gets the number of times this skill has been used.
    /// </summary>
    public int UsageCount { get; init; } = 0;

    /// <summary>
    /// Gets when this skill was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets when this skill was last used.
    /// </summary>
    public DateTime LastUsed { get; init; } = DateTime.UtcNow;
}