// <copyright file="Affordance.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Core.EmbodiedInteraction;

using Ouroboros.Core.Monads;

/// <summary>
/// Represents an affordance - a potential for action that an object or surface offers.
/// Based on J.J. Gibson's ecological psychology concept of affordances.
/// </summary>
/// <param name="Id">Unique identifier for the affordance.</param>
/// <param name="Type">Classification of the affordance.</param>
/// <param name="TargetObjectId">ID of the object offering the affordance.</param>
/// <param name="ActionVerb">The action verb (grasp, push, sit, etc.).</param>
/// <param name="RequiredCapabilities">Capabilities agent needs to use this affordance.</param>
/// <param name="Confidence">Confidence in affordance detection (0.0-1.0).</param>
/// <param name="Constraints">Physical constraints on the action.</param>
/// <param name="RiskLevel">Estimated risk of using this affordance (0.0-1.0).</param>
/// <param name="EnergyRequired">Estimated energy/effort required.</param>
/// <param name="DetectedAt">When the affordance was detected.</param>
public sealed record Affordance(
    Guid Id,
    AffordanceType Type,
    string TargetObjectId,
    string ActionVerb,
    IReadOnlyList<string> RequiredCapabilities,
    double Confidence,
    AffordanceConstraints Constraints,
    double RiskLevel,
    double EnergyRequired,
    DateTime DetectedAt)
{
    /// <summary>
    /// Creates a new affordance with auto-generated ID and timestamp.
    /// </summary>
    public static Affordance Create(
        AffordanceType type,
        string targetObjectId,
        string actionVerb,
        IReadOnlyList<string>? requiredCapabilities = null,
        double confidence = 1.0,
        AffordanceConstraints? constraints = null,
        double riskLevel = 0.0,
        double energyRequired = 1.0) =>
        new(
            Guid.NewGuid(),
            type,
            targetObjectId,
            actionVerb,
            requiredCapabilities ?? Array.Empty<string>(),
            confidence,
            constraints ?? AffordanceConstraints.None,
            riskLevel,
            energyRequired,
            DateTime.UtcNow);

    /// <summary>
    /// Creates a traversable surface affordance.
    /// </summary>
    public static Affordance Traversable(string surfaceId, double confidence = 1.0) =>
        Create(AffordanceType.Traversable, surfaceId, "walk", confidence: confidence);

    /// <summary>
    /// Creates a graspable object affordance.
    /// </summary>
    public static Affordance Graspable(
        string objectId,
        double confidence = 1.0,
        GripRequirement? grip = null) =>
        Create(
            AffordanceType.Graspable,
            objectId,
            "grasp",
            new[] { "manipulator", "gripper" },
            confidence,
            new AffordanceConstraints(
                MinApproachDistance: grip?.MinApproachDistance ?? 0.1,
                MaxApproachDistance: grip?.MaxApproachDistance ?? 0.5,
                RequiredOrientation: null,
                ForceRange: grip?.ForceRange,
                TimeConstraint: null,
                CustomConstraints: null));

    /// <summary>
    /// Creates an activatable object affordance (button, lever, etc.).
    /// </summary>
    public static Affordance Activatable(string objectId, string activationType = "press") =>
        Create(AffordanceType.Activatable, objectId, activationType);

    /// <summary>
    /// Checks if the agent has the required capabilities.
    /// </summary>
    public bool CanBeUsedBy(IReadOnlySet<string> agentCapabilities)
    {
        foreach (var required in RequiredCapabilities)
        {
            if (!agentCapabilities.Contains(required))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns a risk-adjusted confidence score.
    /// </summary>
    public double RiskAdjustedConfidence => Confidence * (1.0 - RiskLevel);
}