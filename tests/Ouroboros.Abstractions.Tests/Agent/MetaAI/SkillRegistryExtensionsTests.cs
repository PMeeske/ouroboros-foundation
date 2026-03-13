using Ouroboros.Abstractions;
using Ouroboros.Agent;
using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class SkillRegistryExtensionsTests
{
    private static Skill CreateSampleSkill(string name = "TestSkill") =>
        new Skill(
            Name: name,
            Description: "A test skill for testing",
            Prerequisites: new List<string> { "prereq1" },
            Steps: new List<PlanStep>
            {
                new PlanStep("step1", new Dictionary<string, object>(), "outcome1", 0.95)
            },
            SuccessRate: 0.9,
            UsageCount: 10,
            CreatedAt: DateTime.UtcNow.AddHours(-1),
            LastUsed: DateTime.UtcNow);

    private static AgentSkill CreateSampleAgentSkill(string name = "TestSkill") =>
        new AgentSkill(
            Id: "skill-1",
            Name: name,
            Description: "A test skill for testing",
            Category: "testing",
            Preconditions: new List<string> { "prereq1" },
            Effects: new List<string> { "effect1" },
            SuccessRate: 0.9,
            UsageCount: 10,
            AverageExecutionTime: 500L,
            Tags: new List<string> { "test" });


}
