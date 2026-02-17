namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Requirements for grasping an object.
/// </summary>
/// <param name="MinApproachDistance">Minimum approach distance.</param>
/// <param name="MaxApproachDistance">Maximum approach distance.</param>
/// <param name="ForceRange">Required grip force range.</param>
public sealed record GripRequirement(
    double MinApproachDistance,
    double MaxApproachDistance,
    (double Min, double Max)? ForceRange);