namespace Ouroboros.Tests.Domain.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class AutonomousConfigurationTests
{
    [Fact]
    public void DefaultValues_AreReasonable()
    {
        // Act
        var config = new AutonomousConfiguration();

        // Assert
        config.Culture.Should().BeNull();
        config.PushBasedMode.Should().BeTrue();
        config.YoloMode.Should().BeFalse();
        config.AutoApproveLowRisk.Should().BeFalse();
        config.AutoApproveSelfReflection.Should().BeTrue();
        config.AutoApproveMemoryOps.Should().BeTrue();
        config.TickIntervalSeconds.Should().Be(30);
        config.MaxPendingIntentions.Should().Be(20);
        config.EnableProactiveCommunication.Should().BeTrue();
        config.EnableCodeModification.Should().BeTrue();
        config.IntentionExpiryMinutes.Should().Be(60);
    }

    [Fact]
    public void AlwaysRequireApproval_DefaultContainsCriticalCategories()
    {
        // Act
        var config = new AutonomousConfiguration();

        // Assert
        config.AlwaysRequireApproval.Should().Contain(IntentionCategory.CodeModification);
        config.AlwaysRequireApproval.Should().Contain(IntentionCategory.GoalPursuit);
    }

    [Fact]
    public void ResearchToolPriority_HasDefaultEntries()
    {
        // Act
        var config = new AutonomousConfiguration();

        // Assert
        config.ResearchToolPriority.Should().NotBeEmpty();
        config.ResearchToolPriority.Should().Contain("web_research");
    }

    [Fact]
    public void CodeToolPriority_HasDefaultEntries()
    {
        // Act
        var config = new AutonomousConfiguration();

        // Assert
        config.CodeToolPriority.Should().NotBeEmpty();
        config.CodeToolPriority.Should().Contain("code_analyze");
    }

    [Fact]
    public void GeneralToolPriority_HasDefaultEntries()
    {
        // Act
        var config = new AutonomousConfiguration();

        // Assert
        config.GeneralToolPriority.Should().NotBeEmpty();
        config.GeneralToolPriority.Should().Contain("web_research");
    }

    [Fact]
    public void WithInit_OverridesValues()
    {
        // Act
        var config = new AutonomousConfiguration
        {
            Culture = "de-DE",
            YoloMode = true,
            TickIntervalSeconds = 10,
        };

        // Assert
        config.Culture.Should().Be("de-DE");
        config.YoloMode.Should().BeTrue();
        config.TickIntervalSeconds.Should().Be(10);
    }
}
