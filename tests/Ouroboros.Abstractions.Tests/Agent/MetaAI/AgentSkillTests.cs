using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class AgentSkillTests
{
    [Fact]
    public void AgentSkill_RecordCreation_AllPropertiesSet()
    {
        // Arrange
        var preconditions = new List<string> { "has-access", "is-trained" };
        var effects = new List<string> { "data-processed", "report-generated" };
        var tags = new List<string> { "data", "analysis" };

        // Act
        var skill = new AgentSkill(
            Id: "skill-1",
            Name: "DataAnalysis",
            Description: "Analyzes data sets",
            Category: "analysis",
            Preconditions: preconditions,
            Effects: effects,
            SuccessRate: 0.85,
            UsageCount: 100,
            AverageExecutionTime: 5000L,
            Tags: tags);

        // Assert
        skill.Id.Should().Be("skill-1");
        skill.Name.Should().Be("DataAnalysis");
        skill.Description.Should().Be("Analyzes data sets");
        skill.Category.Should().Be("analysis");
        skill.Preconditions.Should().HaveCount(2);
        skill.Effects.Should().HaveCount(2);
        skill.SuccessRate.Should().Be(0.85);
        skill.UsageCount.Should().Be(100);
        skill.AverageExecutionTime.Should().Be(5000L);
        skill.Tags.Should().Contain("data");
    }

    [Fact]
    public void AgentSkill_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var preconditions = new List<string> { "pre" };
        var effects = new List<string> { "eff" };
        var tags = new List<string> { "tag" };

        var a = new AgentSkill("id", "name", "desc", "cat", preconditions, effects, 0.9, 10, 100, tags);
        var b = new AgentSkill("id", "name", "desc", "cat", preconditions, effects, 0.9, 10, 100, tags);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void AgentSkill_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new AgentSkill(
            "id", "name", "desc", "cat",
            new List<string>(), new List<string>(),
            0.5, 5, 500, new List<string>());

        // Act
        var modified = original with { SuccessRate = 0.99, UsageCount = 50 };

        // Assert
        modified.SuccessRate.Should().Be(0.99);
        modified.UsageCount.Should().Be(50);
        modified.Name.Should().Be("name");
    }

    [Fact]
    public void AgentSkill_ToString_ContainsKeyInfo()
    {
        // Arrange
        var skill = new AgentSkill(
            "id", "TestSkill", "desc", "cat",
            new List<string>(), new List<string>(),
            0.9, 10, 100, new List<string>());

        // Act
        var str = skill.ToString();

        // Assert
        str.Should().Contain("TestSkill");
    }
}
