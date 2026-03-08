using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class RoutingDecisionTests
{
    [Fact]
    public void RoutingDecision_Proceed_AllPropertiesSet()
    {
        // Act
        var decision = new Ouroboros.Agent.MetaAI.RoutingDecision(
            ShouldProceed: true,
            ConfidenceLevel: 0.95,
            RecommendedStrategy: Ouroboros.Agent.MetaAI.FallbackStrategy.Retry,
            Reason: "High confidence",
            RequiresHumanOversight: false,
            AlternativeActions: new List<string>());

        // Assert
        decision.ShouldProceed.Should().BeTrue();
        decision.ConfidenceLevel.Should().Be(0.95);
        decision.RecommendedStrategy.Should().Be(Ouroboros.Agent.MetaAI.FallbackStrategy.Retry);
        decision.Reason.Should().Be("High confidence");
        decision.RequiresHumanOversight.Should().BeFalse();
        decision.AlternativeActions.Should().BeEmpty();
    }

    [Fact]
    public void RoutingDecision_NoProceed_WithAlternatives()
    {
        // Act
        var decision = new Ouroboros.Agent.MetaAI.RoutingDecision(
            ShouldProceed: false,
            ConfidenceLevel: 0.2,
            RecommendedStrategy: Ouroboros.Agent.MetaAI.FallbackStrategy.EscalateToHuman,
            Reason: "Low confidence",
            RequiresHumanOversight: true,
            AlternativeActions: new List<string> { "alt1", "alt2" });

        // Assert
        decision.ShouldProceed.Should().BeFalse();
        decision.RequiresHumanOversight.Should().BeTrue();
        decision.AlternativeActions.Should().HaveCount(2);
    }

    [Fact]
    public void FallbackStrategy_AllValuesAreDefined()
    {
        // Act
        var values = Enum.GetValues<Ouroboros.Agent.MetaAI.FallbackStrategy>();

        // Assert
        values.Should().Contain(Ouroboros.Agent.MetaAI.FallbackStrategy.Retry);
        values.Should().Contain(Ouroboros.Agent.MetaAI.FallbackStrategy.EscalateToHuman);
        values.Should().Contain(Ouroboros.Agent.MetaAI.FallbackStrategy.UseConservativeApproach);
        values.Should().Contain(Ouroboros.Agent.MetaAI.FallbackStrategy.Defer);
        values.Should().Contain(Ouroboros.Agent.MetaAI.FallbackStrategy.Abort);
        values.Should().Contain(Ouroboros.Agent.MetaAI.FallbackStrategy.RequestClarification);
    }

    [Fact]
    public void SandboxResult_Success_AllPropertiesSet()
    {
        // Arrange
        var step = new PlanStep("action", new Dictionary<string, object>(), "outcome", 0.9);

        // Act
        var result = new SandboxResult(
            true, step, new List<string> { "no-network" }, null);

        // Assert
        result.Success.Should().BeTrue();
        result.SandboxedStep.Should().Be(step);
        result.Restrictions.Should().Contain("no-network");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void SandboxResult_Failure_HasError()
    {
        // Act
        var result = new SandboxResult(
            false, null, new List<string>(), "sandboxing failed");

        // Assert
        result.Success.Should().BeFalse();
        result.SandboxedStep.Should().BeNull();
        result.Error.Should().Be("sandboxing failed");
    }

    [Fact]
    public void Permission_AllPropertiesSet()
    {
        // Act
        var permission = new Ouroboros.Agent.MetaAI.Permission(
            "filesystem", Ouroboros.Agent.MetaAI.PermissionLevel.Write, "need write access");

        // Assert
        permission.Resource.Should().Be("filesystem");
        permission.Level.Should().Be(Ouroboros.Agent.MetaAI.PermissionLevel.Write);
        permission.Reason.Should().Be("need write access");
    }

    [Fact]
    public void PermissionLevel_AllValuesAreDefined()
    {
        // Act
        var values = Enum.GetValues<Ouroboros.Agent.MetaAI.PermissionLevel>();

        // Assert
        values.Should().Contain(Ouroboros.Agent.MetaAI.PermissionLevel.None);
        values.Should().Contain(Ouroboros.Agent.MetaAI.PermissionLevel.Isolated);
        values.Should().Contain(Ouroboros.Agent.MetaAI.PermissionLevel.Read);
        values.Should().Contain(Ouroboros.Agent.MetaAI.PermissionLevel.Write);
        values.Should().Contain(Ouroboros.Agent.MetaAI.PermissionLevel.Execute);
        values.Should().Contain(Ouroboros.Agent.MetaAI.PermissionLevel.Admin);
    }

    [Fact]
    public void PermissionLevel_ReadOnly_EqualsRead()
    {
        // Assert - ReadOnly is an alias for Read
        ((int)Ouroboros.Agent.MetaAI.PermissionLevel.ReadOnly).Should()
            .Be((int)Ouroboros.Agent.MetaAI.PermissionLevel.Read);
    }
}
