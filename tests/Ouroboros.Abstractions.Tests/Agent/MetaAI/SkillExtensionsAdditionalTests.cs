using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class SkillExtensionsAdditionalTests
{
    [Fact]
    public void ToAgentSkill_ZeroUsageCount_AverageExecutionTimeIsZero()
    {
        // Arrange
        var skill = new Skill(
            "TestSkill", "desc", new List<string>(),
            new List<PlanStep>(), 0.0, 0,
            DateTime.UtcNow, DateTime.UtcNow);

        // Act
        var agentSkill = skill.ToAgentSkill();

        // Assert
        agentSkill.AverageExecutionTime.Should().Be(0L);
    }

    [Fact]
    public void ToSkill_AgentSkillWithNoEffects_ProducesEmptySteps()
    {
        // Arrange
        var agentSkill = new AgentSkill(
            "id", "name", "desc", "cat",
            new List<string>(), new List<string>(),
            0.9, 10, 500, new List<string>());

        // Act
        var skill = agentSkill.ToSkill();

        // Assert
        skill.Steps.Should().BeEmpty();
    }

    [Fact]
    public void ToAgentSkill_ExtractsTagsFromNameAndDescription()
    {
        // Arrange - long enough words in name + description
        var skill = new Skill(
            "DataProcessing", "Handles complex transformations",
            new List<string>(), new List<PlanStep>(),
            0.9, 5, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);

        // Act
        var agentSkill = skill.ToAgentSkill();

        // Assert
        agentSkill.Tags.Should().NotBeEmpty();
        // Words over 3 chars should be present
        agentSkill.Tags.Should().Contain("handles");
        agentSkill.Tags.Should().Contain("complex");
        agentSkill.Tags.Should().Contain("transformations");
    }

    [Fact]
    public void ToAgentSkill_Effects_MappedFromStepExpectedOutcomes()
    {
        // Arrange
        var skill = new Skill(
            "name", "desc", new List<string>(),
            new List<PlanStep>
            {
                new PlanStep("step1", new Dictionary<string, object>(), "outcome1", 0.9),
                new PlanStep("step2", new Dictionary<string, object>(), "outcome2", 0.8)
            },
            0.9, 10, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow);

        // Act
        var agentSkill = skill.ToAgentSkill();

        // Assert
        agentSkill.Effects.Should().HaveCount(2);
        agentSkill.Effects.Should().Contain("outcome1");
        agentSkill.Effects.Should().Contain("outcome2");
    }

    [Fact]
    public void ToSkills_EmptyCollection_ReturnsEmptyList()
    {
        // Arrange
        var agentSkills = new List<AgentSkill>();

        // Act
        var skills = agentSkills.ToSkills();

        // Assert
        skills.Should().BeEmpty();
    }

    [Fact]
    public void ToAgentSkills_EmptyCollection_ReturnsEmptyList()
    {
        // Arrange
        var skills = new List<Skill>();

        // Act
        var agentSkills = skills.ToAgentSkills();

        // Assert
        agentSkills.Should().BeEmpty();
    }

    [Fact]
    public void ToAgentSkills_DefaultCategory_IsLearned()
    {
        // Arrange
        var skills = new List<Skill>
        {
            new Skill("s", "d", new List<string>(), new List<PlanStep>(),
                0.9, 1, DateTime.UtcNow, DateTime.UtcNow)
        };

        // Act
        var agentSkills = skills.ToAgentSkills();

        // Assert
        agentSkills[0].Category.Should().Be("learned");
    }
}
