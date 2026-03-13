using Ouroboros.Domain.Environment;

namespace Ouroboros.Tests.Environment;

[Trait("Category", "Unit")]
public class EnvironmentStepTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var state = new EnvironmentState(new Dictionary<string, object> { ["pos"] = "0" });
        var action = new EnvironmentAction("move", new Dictionary<string, object>());
        var obs = new Observation(state, 1.0, false);
        var timestamp = DateTime.UtcNow;

        var step = new EnvironmentStep(1, state, action, obs, timestamp);

        step.StepNumber.Should().Be(1);
        step.State.Should().Be(state);
        step.Action.Should().Be(action);
        step.Observation.Should().Be(obs);
        step.Timestamp.Should().Be(timestamp);
        step.Metadata.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMetadata_ShouldSetMetadata()
    {
        var meta = new Dictionary<string, object> { ["info"] = "test" };
        var step = new EnvironmentStep(
            1,
            new EnvironmentState(new Dictionary<string, object>()),
            new EnvironmentAction("a", new Dictionary<string, object>()),
            new Observation(new EnvironmentState(new Dictionary<string, object>()), 0, false),
            DateTime.UtcNow,
            meta);

        step.Metadata.Should().ContainKey("info");
    }
}
