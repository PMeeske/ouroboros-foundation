namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Constraints on how an affordance can be used.
/// </summary>
/// <param name="MinApproachDistance">Minimum distance to approach from.</param>
/// <param name="MaxApproachDistance">Maximum distance to approach from.</param>
/// <param name="RequiredOrientation">Required agent orientation (if any).</param>
/// <param name="ForceRange">Required force range (min, max) in Newtons.</param>
/// <param name="TimeConstraint">Time window for action (if any).</param>
/// <param name="CustomConstraints">Additional custom constraints.</param>
public sealed record AffordanceConstraints(
    double? MinApproachDistance,
    double? MaxApproachDistance,
    (double X, double Y, double Z)? RequiredOrientation,
    (double Min, double Max)? ForceRange,
    TimeSpan? TimeConstraint,
    IReadOnlyDictionary<string, object>? CustomConstraints)
{
    /// <summary>
    /// No constraints.
    /// </summary>
    public static AffordanceConstraints None => new(null, null, null, null, null, null);
}