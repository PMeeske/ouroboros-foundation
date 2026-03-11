using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class ExperienceFactoryAdditionalTests
{
    [Fact]
    public void FromExecution_SuccessWithNullFinalOutput_UsesSuccessOutcome()
    {
        // Arrange
        var plan = new Plan("goal", new List<PlanStep>
        {
            new PlanStep("step1", new Dictionary<string, object>(), "expected", 0.9)
        }, new Dictionary<string, double>(), DateTime.UtcNow);

        var execution = new PlanExecutionResult(
            plan, new List<StepResult>(), true, null,
            new Dictionary<string, object>(), TimeSpan.Zero);
        var verification = new PlanVerificationResult(
            execution, true, 1.0, new List<string>(),
            new List<string>(), DateTime.UtcNow);

        // Act
        var experience = ExperienceFactory.FromExecution("goal", execution, verification);

        // Assert
        experience.Outcome.Should().Be("success");
        experience.Success.Should().BeTrue();
    }

    [Fact]
    public void FromExecution_WithCustomTags_UsesProvidedTags()
    {
        // Arrange
        var plan = new Plan("goal", new List<PlanStep>(),
            new Dictionary<string, double>(), DateTime.UtcNow);
        var execution = new PlanExecutionResult(
            plan, new List<StepResult>(), true, "output",
            new Dictionary<string, object>(), TimeSpan.Zero);
        var verification = new PlanVerificationResult(
            execution, true, 1.0, new List<string>(),
            new List<string>(), DateTime.UtcNow);
        var tags = new List<string> { "custom-tag1", "custom-tag2" };

        // Act
        var experience = ExperienceFactory.FromExecution(
            "goal", execution, verification, tags);

        // Assert
        experience.Tags.Should().BeEquivalentTo(tags);
    }

    [Fact]
    public void FromExecution_WithCustomMetadata_SetsMetadata()
    {
        // Arrange
        var plan = new Plan("goal", new List<PlanStep>(),
            new Dictionary<string, double>(), DateTime.UtcNow);
        var execution = new PlanExecutionResult(
            plan, new List<StepResult>(), true, "output",
            new Dictionary<string, object>(), TimeSpan.Zero);
        var verification = new PlanVerificationResult(
            execution, true, 1.0, new List<string>(),
            new List<string>(), DateTime.UtcNow);
        var metadata = new Dictionary<string, object> { ["key"] = "value" };

        // Act
        var experience = ExperienceFactory.FromExecution(
            "goal", execution, verification, metadata: metadata);

        // Assert
        experience.Metadata.Should().ContainKey("key");
    }

    [Fact]
    public void Simple_WhitespaceGoal_ReturnsEmptyTags()
    {
        // Act
        var experience = ExperienceFactory.Simple(
            "   ", "action", "outcome", true);

        // Assert
        experience.Tags.Should().BeEmpty();
    }

    [Fact]
    public void Simple_GoalWithShortWords_FiltersWordsUnder4Chars()
    {
        // Act - "a to be or" are all <= 3 chars
        var experience = ExperienceFactory.Simple(
            "a to be or this", "action", "outcome", true);

        // Assert
        experience.Tags.Should().Contain("this");
        experience.Tags.Should().NotContain("to");
        experience.Tags.Should().NotContain("be");
    }

    [Fact]
    public void Simple_GoalWithDuplicateWords_ReturnsDistinctTags()
    {
        // Act
        var experience = ExperienceFactory.Simple(
            "important important important task", "action", "outcome", true);

        // Assert
        experience.Tags.Where(t => t == "important").Should().HaveCount(1);
    }

    [Fact]
    public void Simple_GoalWithSpecialSeparators_SplitsCorrectly()
    {
        // Act
        var experience = ExperienceFactory.Simple(
            "analyze-data:processing;important_task", "action", "outcome", true);

        // Assert
        experience.Tags.Should().Contain("analyze");
        experience.Tags.Should().Contain("data");
        experience.Tags.Should().Contain("processing");
        experience.Tags.Should().Contain("important");
        experience.Tags.Should().Contain("task");
    }

    [Fact]
    public void Simple_GoalWithManyWords_LimitsToTenTags()
    {
        // Arrange - create a goal with more than 10 unique long words
        var words = Enumerable.Range(1, 20).Select(i => $"word{i:D5}");
        var goal = string.Join(" ", words);

        // Act
        var experience = ExperienceFactory.Simple(goal, "action", "outcome", true);

        // Assert
        experience.Tags.Should().HaveCountLessOrEqualTo(10);
    }

    [Fact]
    public void Simple_Success_VerificationVerifiedIsTrue()
    {
        // Act
        var experience = ExperienceFactory.Simple(
            "goal", "action", "outcome", true);

        // Assert
        experience.Verification.Verified.Should().BeTrue();
        experience.Verification.QualityScore.Should().Be(1.0);
    }

    [Fact]
    public void Simple_Failure_VerificationQualityScoreIsZero()
    {
        // Act
        var experience = ExperienceFactory.Simple(
            "goal", "action", "outcome", false);

        // Assert
        experience.Verification.QualityScore.Should().Be(0.0);
    }

    [Fact]
    public void Simple_ContextMatchesGoal()
    {
        // Act
        var experience = ExperienceFactory.Simple(
            "my goal", "action", "outcome", true);

        // Assert
        experience.Context.Should().Be("my goal");
    }
}
