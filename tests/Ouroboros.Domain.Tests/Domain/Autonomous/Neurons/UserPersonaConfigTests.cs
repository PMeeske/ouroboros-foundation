namespace Ouroboros.Tests.Domain.Autonomous.Neurons;

using Ouroboros.Domain.Autonomous.Neurons;

[Trait("Category", "Unit")]
public class UserPersonaConfigTests
{
    [Fact]
    public void DefaultValues_AreReasonable()
    {
        // Act
        var config = new UserPersonaConfig();

        // Assert
        config.Name.Should().Be("AutoUser");
        config.SkillLevel.Should().Be("intermediate");
        config.CommunicationStyle.Should().Be("casual");
        config.FollowUpProbability.Should().Be(0.6);
        config.ChallengeProbability.Should().Be(0.2);
        config.MessageIntervalSeconds.Should().Be(30);
        config.MaxSessionMessages.Should().Be(50);
        config.SelfDialogueMode.Should().BeFalse();
        config.SecondPersonaName.Should().Be("Ouroboros-B");
        config.ProblemSolvingMode.Should().BeFalse();
        config.Problem.Should().BeNull();
        config.DeliverableType.Should().Be("plan");
        config.UseTools.Should().BeTrue();
        config.YoloMode.Should().BeFalse();
    }

    [Fact]
    public void Traits_HasDefaultValues()
    {
        // Act
        var config = new UserPersonaConfig();

        // Assert
        config.Traits.Should().NotBeEmpty();
        config.Traits.Should().Contain("curious");
    }

    [Fact]
    public void Interests_HasDefaultValues()
    {
        // Act
        var config = new UserPersonaConfig();

        // Assert
        config.Interests.Should().NotBeEmpty();
        config.Interests.Should().Contain("artificial intelligence");
    }

    [Fact]
    public void SecondPersonaTraits_HasDefaultValues()
    {
        // Act
        var config = new UserPersonaConfig();

        // Assert
        config.SecondPersonaTraits.Should().NotBeEmpty();
        config.SecondPersonaTraits.Should().Contain("skeptical");
    }

    [Fact]
    public void WithInit_OverridesDefaults()
    {
        // Act
        var config = new UserPersonaConfig
        {
            Name = "TestUser",
            SkillLevel = "expert",
            SelfDialogueMode = true,
            YoloMode = true,
        };

        // Assert
        config.Name.Should().Be("TestUser");
        config.SkillLevel.Should().Be("expert");
        config.SelfDialogueMode.Should().BeTrue();
        config.YoloMode.Should().BeTrue();
    }
}
