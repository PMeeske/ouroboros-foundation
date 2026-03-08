using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class SkillExtensionsTests
{
    private static AgentSkill CreateSampleAgentSkill() =>
        new AgentSkill(
            Id: "skill-1",
            Name: "TestSkill",
            Description: "A test skill",
            Category: "testing",
            Preconditions: new List<string> { "pre1" },
            Effects: new List<string> { "effect1", "effect2" },
            SuccessRate: 0.9,
            UsageCount: 10,
            AverageExecutionTime: 500L,
            Tags: new List<string> { "test" });

    private static Skill CreateSampleSkill() =>
        new Skill(
            Name: "TestSkill",
            Description: "A test skill",
            Prerequisites: new List<string> { "pre1" },
            Steps: new List<PlanStep>
            {
                new PlanStep("step1", new Dictionary<string, object>(), "outcome1", 0.95)
            },
            SuccessRate: 0.9,
            UsageCount: 10,
            CreatedAt: DateTime.UtcNow.AddHours(-1),
            LastUsed: DateTime.UtcNow);

    [Fact]
    public void ToSkill_ConvertsAgentSkillToSkill()
    {
        // Arrange
        var agentSkill = CreateSampleAgentSkill();

        // Act
        var skill = agentSkill.ToSkill();

        // Assert
        skill.Name.Should().Be("TestSkill");
        skill.Description.Should().Be("A test skill");
        skill.Prerequisites.Should().Contain("pre1");
        skill.SuccessRate.Should().Be(0.9);
        skill.UsageCount.Should().Be(10);
        skill.Steps.Should().HaveCount(2); // one per effect
    }

    [Fact]
    public void ToSkill_NullAgentSkill_ThrowsArgumentNullException()
    {
        // Act
        AgentSkill? nullSkill = null;
        var act = () => nullSkill!.ToSkill();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToAgentSkill_ConvertsSkillToAgentSkill()
    {
        // Arrange
        var skill = CreateSampleSkill();

        // Act
        var agentSkill = skill.ToAgentSkill();

        // Assert
        agentSkill.Name.Should().Be("TestSkill");
        agentSkill.Description.Should().Be("A test skill");
        agentSkill.Category.Should().Be("learned");
        agentSkill.Preconditions.Should().Contain("pre1");
        agentSkill.SuccessRate.Should().Be(0.9);
        agentSkill.UsageCount.Should().Be(10);
        agentSkill.Id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ToAgentSkill_WithCustomIdAndCategory_UsesProvided()
    {
        // Arrange
        var skill = CreateSampleSkill();

        // Act
        var agentSkill = skill.ToAgentSkill(
            id: "custom-id",
            category: "custom-cat");

        // Assert
        agentSkill.Id.Should().Be("custom-id");
        agentSkill.Category.Should().Be("custom-cat");
    }

    [Fact]
    public void ToAgentSkill_WithCustomTags_UsesTags()
    {
        // Arrange
        var skill = CreateSampleSkill();
        var tags = new List<string> { "tag1", "tag2" };

        // Act
        var agentSkill = skill.ToAgentSkill(tags: tags);

        // Assert
        agentSkill.Tags.Should().Contain("tag1");
        agentSkill.Tags.Should().Contain("tag2");
    }

    [Fact]
    public void ToAgentSkill_WithoutTags_ExtractsFromNameAndDescription()
    {
        // Arrange
        var skill = CreateSampleSkill();

        // Act
        var agentSkill = skill.ToAgentSkill();

        // Assert
        agentSkill.Tags.Should().NotBeEmpty();
    }

    [Fact]
    public void ToAgentSkill_NullSkill_ThrowsArgumentNullException()
    {
        // Act
        Skill? nullSkill = null;
        var act = () => nullSkill!.ToAgentSkill();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToSkills_ConvertsCollection()
    {
        // Arrange
        var agentSkills = new List<AgentSkill>
        {
            CreateSampleAgentSkill(),
            CreateSampleAgentSkill() with { Name = "Skill2" }
        };

        // Act
        var skills = agentSkills.ToSkills();

        // Assert
        skills.Should().HaveCount(2);
        skills[0].Name.Should().Be("TestSkill");
        skills[1].Name.Should().Be("Skill2");
    }

    [Fact]
    public void ToAgentSkills_ConvertsCollection()
    {
        // Arrange
        var skills = new List<Skill>
        {
            CreateSampleSkill(),
            CreateSampleSkill() with { Name = "Skill2" }
        };

        // Act
        var agentSkills = skills.ToAgentSkills("custom-cat");

        // Assert
        agentSkills.Should().HaveCount(2);
        agentSkills[0].Category.Should().Be("custom-cat");
        agentSkills[1].Category.Should().Be("custom-cat");
    }

    [Fact]
    public void RoundTrip_AgentSkillToSkillAndBack_PreservesKeyData()
    {
        // Arrange
        var original = CreateSampleAgentSkill();

        // Act
        var skill = original.ToSkill();
        var roundTripped = skill.ToAgentSkill(id: original.Id, category: original.Category);

        // Assert
        roundTripped.Name.Should().Be(original.Name);
        roundTripped.Description.Should().Be(original.Description);
        roundTripped.SuccessRate.Should().Be(original.SuccessRate);
        roundTripped.UsageCount.Should().Be(original.UsageCount);
    }
}
