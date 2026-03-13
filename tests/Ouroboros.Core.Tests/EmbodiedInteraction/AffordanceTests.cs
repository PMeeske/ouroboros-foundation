// <copyright file="AffordanceTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.EmbodiedInteraction;
using Xunit;

namespace Ouroboros.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class AffordanceTests
{
    // -- AffordanceConstraints --

    [Fact]
    public void AffordanceConstraints_None_ShouldHaveAllNulls()
    {
        // Act
        var constraints = AffordanceConstraints.None;

        // Assert
        constraints.MinApproachDistance.Should().BeNull();
        constraints.MaxApproachDistance.Should().BeNull();
        constraints.RequiredOrientation.Should().BeNull();
        constraints.ForceRange.Should().BeNull();
        constraints.TimeConstraint.Should().BeNull();
        constraints.CustomConstraints.Should().BeNull();
    }

    [Fact]
    public void AffordanceConstraints_Constructor_ShouldSetAllProperties()
    {
        // Act
        var constraints = new AffordanceConstraints(
            0.1, 0.5,
            (1.0, 0.0, 0.0),
            (5.0, 20.0),
            TimeSpan.FromSeconds(10),
            new Dictionary<string, object> { ["grip"] = "power" });

        // Assert
        constraints.MinApproachDistance.Should().Be(0.1);
        constraints.MaxApproachDistance.Should().Be(0.5);
        constraints.RequiredOrientation.Should().Be((1.0, 0.0, 0.0));
        constraints.ForceRange.Should().Be((5.0, 20.0));
        constraints.TimeConstraint.Should().Be(TimeSpan.FromSeconds(10));
        constraints.CustomConstraints.Should().ContainKey("grip");
    }

    // -- GripRequirement --

    [Fact]
    public void GripRequirement_ShouldInitializeAllProperties()
    {
        // Act
        var grip = new GripRequirement(0.1, 0.5, (5.0, 20.0));

        // Assert
        grip.MinApproachDistance.Should().Be(0.1);
        grip.MaxApproachDistance.Should().Be(0.5);
        grip.ForceRange.Should().Be((5.0, 20.0));
    }

    [Fact]
    public void GripRequirement_NullForceRange_ShouldBeAllowed()
    {
        // Act
        var grip = new GripRequirement(0.2, 0.8, null);

        // Assert
        grip.ForceRange.Should().BeNull();
    }

    // -- Affordance.Create --

    [Fact]
    public void Create_ShouldSetDefaultValues()
    {
        // Act
        var affordance = Affordance.Create(
            AffordanceType.Traversable, "floor-1", "walk");

        // Assert
        affordance.Id.Should().NotBe(Guid.Empty);
        affordance.Type.Should().Be(AffordanceType.Traversable);
        affordance.TargetObjectId.Should().Be("floor-1");
        affordance.ActionVerb.Should().Be("walk");
        affordance.RequiredCapabilities.Should().BeEmpty();
        affordance.Confidence.Should().Be(1.0);
        affordance.Constraints.Should().Be(AffordanceConstraints.None);
        affordance.RiskLevel.Should().Be(0.0);
        affordance.EnergyRequired.Should().Be(1.0);
        affordance.DetectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_WithCustomParameters_ShouldApplyThem()
    {
        // Arrange
        var caps = new List<string> { "manipulator", "gripper" };

        // Act
        var affordance = Affordance.Create(
            AffordanceType.Graspable, "obj-1", "grasp",
            caps, 0.8, AffordanceConstraints.None, 0.2, 3.0);

        // Assert
        affordance.RequiredCapabilities.Should().BeEquivalentTo(caps);
        affordance.Confidence.Should().Be(0.8);
        affordance.RiskLevel.Should().Be(0.2);
        affordance.EnergyRequired.Should().Be(3.0);
    }

    // -- Affordance.Traversable --

    [Fact]
    public void Traversable_ShouldCreateCorrectly()
    {
        // Act
        var affordance = Affordance.Traversable("surface-1", 0.95);

        // Assert
        affordance.Type.Should().Be(AffordanceType.Traversable);
        affordance.TargetObjectId.Should().Be("surface-1");
        affordance.ActionVerb.Should().Be("walk");
        affordance.Confidence.Should().Be(0.95);
    }

    // -- Affordance.Graspable --

    [Fact]
    public void Graspable_ShouldCreateWithManipulatorCapabilities()
    {
        // Act
        var affordance = Affordance.Graspable("obj-1");

        // Assert
        affordance.Type.Should().Be(AffordanceType.Graspable);
        affordance.ActionVerb.Should().Be("grasp");
        affordance.RequiredCapabilities.Should().Contain("manipulator");
        affordance.RequiredCapabilities.Should().Contain("gripper");
    }

    [Fact]
    public void Graspable_WithGripRequirement_ShouldSetConstraints()
    {
        // Arrange
        var grip = new GripRequirement(0.05, 0.3, (10.0, 50.0));

        // Act
        var affordance = Affordance.Graspable("obj-2", grip: grip);

        // Assert
        affordance.Constraints.MinApproachDistance.Should().Be(0.05);
        affordance.Constraints.MaxApproachDistance.Should().Be(0.3);
        affordance.Constraints.ForceRange.Should().Be((10.0, 50.0));
    }

    // -- Affordance.Activatable --

    [Fact]
    public void Activatable_ShouldCreateWithDefaultPress()
    {
        // Act
        var affordance = Affordance.Activatable("button-1");

        // Assert
        affordance.Type.Should().Be(AffordanceType.Activatable);
        affordance.ActionVerb.Should().Be("press");
    }

    [Fact]
    public void Activatable_WithCustomAction_ShouldUseIt()
    {
        // Act
        var affordance = Affordance.Activatable("lever-1", "pull");

        // Assert
        affordance.ActionVerb.Should().Be("pull");
    }

    // -- Affordance.CanBeUsedBy --

    [Fact]
    public void CanBeUsedBy_AllCapabilitiesPresent_ShouldReturnTrue()
    {
        // Arrange
        var affordance = Affordance.Graspable("obj-1");
        var agentCaps = new HashSet<string> { "manipulator", "gripper", "locomotion" };

        // Act & Assert
        affordance.CanBeUsedBy(agentCaps).Should().BeTrue();
    }

    [Fact]
    public void CanBeUsedBy_MissingCapability_ShouldReturnFalse()
    {
        // Arrange
        var affordance = Affordance.Graspable("obj-1");
        var agentCaps = new HashSet<string> { "manipulator" }; // missing "gripper"

        // Act & Assert
        affordance.CanBeUsedBy(agentCaps).Should().BeFalse();
    }

    [Fact]
    public void CanBeUsedBy_NoRequirements_ShouldReturnTrue()
    {
        // Arrange
        var affordance = Affordance.Traversable("floor-1");
        var agentCaps = new HashSet<string>();

        // Act & Assert
        affordance.CanBeUsedBy(agentCaps).Should().BeTrue();
    }

    // -- RiskAdjustedConfidence --

    [Fact]
    public void RiskAdjustedConfidence_NoRisk_ShouldEqualConfidence()
    {
        // Arrange
        var affordance = Affordance.Create(
            AffordanceType.Traversable, "s1", "walk", confidence: 0.9, riskLevel: 0.0);

        // Act & Assert
        affordance.RiskAdjustedConfidence.Should().Be(0.9);
    }

    [Fact]
    public void RiskAdjustedConfidence_WithRisk_ShouldReduce()
    {
        // Arrange
        var affordance = Affordance.Create(
            AffordanceType.Hazardous, "s1", "cross", confidence: 1.0, riskLevel: 0.5);

        // Act & Assert
        affordance.RiskAdjustedConfidence.Should().Be(0.5);
    }

    [Fact]
    public void RiskAdjustedConfidence_FullRisk_ShouldBeZero()
    {
        // Arrange
        var affordance = Affordance.Create(
            AffordanceType.Hazardous, "s1", "cross", confidence: 1.0, riskLevel: 1.0);

        // Act & Assert
        affordance.RiskAdjustedConfidence.Should().Be(0.0);
    }
}
