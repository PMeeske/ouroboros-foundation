using Ouroboros.Domain.MultiAgent;

namespace Ouroboros.Tests.MultiAgent;

[Trait("Category", "Unit")]
public class AgentCapabilitiesTests
{
    private static AgentId CreateAgentId(string name = "agent") => new(Guid.NewGuid(), name);

    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var id = CreateAgentId("test-agent");
        var skills = new List<string> { "coding", "analysis" };
        var scores = new Dictionary<string, double> { ["coding"] = 0.9, ["analysis"] = 0.8 };

        var caps = new AgentCapabilities(id, skills, scores, 0.5, true);

        caps.Id.Should().Be(id);
        caps.Skills.Should().HaveCount(2);
        caps.PerformanceScores.Should().HaveCount(2);
        caps.CurrentLoad.Should().Be(0.5);
        caps.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void With_ShouldCreateModifiedCopy()
    {
        var caps = new AgentCapabilities(
            CreateAgentId(), new List<string>(), new Dictionary<string, double>(), 0.1, true);

        var modified = caps with { CurrentLoad = 0.9, IsAvailable = false };

        modified.CurrentLoad.Should().Be(0.9);
        modified.IsAvailable.Should().BeFalse();
        caps.CurrentLoad.Should().Be(0.1);
    }
}
