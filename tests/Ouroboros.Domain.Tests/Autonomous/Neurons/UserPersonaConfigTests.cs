namespace Ouroboros.Tests.Autonomous.Neurons;

using Ouroboros.Domain.Autonomous.Neurons;

[Trait("Category", "Unit")]
public class UserPersonaConfigTests
{
    [Fact]
    public void Constructor_DefaultValues_AreExpected()
    {
        var config = new UserPersonaConfig();

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
        var config = new UserPersonaConfig();
        config.Traits.Should().Contain("curious");
        config.Traits.Should().Contain("thoughtful");
    }

    [Fact]
    public void Interests_HasDefaultValues()
    {
        var config = new UserPersonaConfig();
        config.Interests.Should().NotBeEmpty();
        config.Interests.Should().Contain("artificial intelligence");
    }

    [Fact]
    public void SecondPersonaTraits_HasDefaultValues()
    {
        var config = new UserPersonaConfig();
        config.SecondPersonaTraits.Should().Contain("skeptical");
        config.SecondPersonaTraits.Should().Contain("pragmatic");
    }

    [Fact]
    public void WithExpression_CreatesModifiedCopy()
    {
        var original = new UserPersonaConfig();
        var modified = original with
        {
            Name = "TestUser",
            SkillLevel = "expert",
            ProblemSolvingMode = true,
            Problem = "Build a REST API",
            YoloMode = true
        };

        modified.Name.Should().Be("TestUser");
        modified.SkillLevel.Should().Be("expert");
        modified.ProblemSolvingMode.Should().BeTrue();
        modified.Problem.Should().Be("Build a REST API");
        modified.YoloMode.Should().BeTrue();

        // Original unchanged
        original.Name.Should().Be("AutoUser");
        original.ProblemSolvingMode.Should().BeFalse();
    }

    [Fact]
    public void SelfDialogueMode_ConfiguredCorrectly()
    {
        var config = new UserPersonaConfig
        {
            SelfDialogueMode = true,
            SecondPersonaName = "Ouroboros-Critic"
        };

        config.SelfDialogueMode.Should().BeTrue();
        config.SecondPersonaName.Should().Be("Ouroboros-Critic");
    }

    [Fact]
    public void ProblemSolvingMode_WithAllOptions()
    {
        var config = new UserPersonaConfig
        {
            ProblemSolvingMode = true,
            Problem = "Implement rate limiter",
            DeliverableType = "code",
            UseTools = true,
            YoloMode = true,
            MaxSessionMessages = 20
        };

        config.ProblemSolvingMode.Should().BeTrue();
        config.Problem.Should().Be("Implement rate limiter");
        config.DeliverableType.Should().Be("code");
        config.UseTools.Should().BeTrue();
        config.YoloMode.Should().BeTrue();
        config.MaxSessionMessages.Should().Be(20);
    }
}
