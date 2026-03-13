using Ouroboros.Domain.Embodied;

namespace Ouroboros.Tests.Embodied;

[Trait("Category", "Unit")]
public class EnvironmentInfoTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var actions = new List<string> { "move", "jump", "shoot" };
        var observations = new List<string> { "position", "velocity" };
        var info = new EnvironmentInfo("TestEnv", "A test environment", actions, observations, EnvironmentType.Gym);

        info.Name.Should().Be("TestEnv");
        info.Description.Should().Be("A test environment");
        info.AvailableActions.Should().HaveCount(3);
        info.Observations.Should().HaveCount(2);
        info.Type.Should().Be(EnvironmentType.Gym);
    }
}
