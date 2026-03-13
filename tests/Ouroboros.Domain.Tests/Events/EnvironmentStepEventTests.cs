using Ouroboros.Domain.Environment;
using Ouroboros.Domain.Events;

namespace Ouroboros.Tests.Events;

[Trait("Category", "Unit")]
public class EnvironmentStepEventTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var id = Guid.NewGuid();
        var episodeId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var envState = new EnvironmentState(new Dictionary<string, object>());
        var action = new EnvironmentAction("move", new Dictionary<string, object>());
        var obs = new Observation(envState, 1.0, false);
        var step = new EnvironmentStep(1, envState, action, obs, timestamp);

        var evt = new EnvironmentStepEvent(id, episodeId, step, timestamp);

        evt.Id.Should().Be(id);
        evt.EpisodeId.Should().Be(episodeId);
        evt.Step.Should().Be(step);
        evt.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void Kind_ShouldBeEnvironmentStep()
    {
        var envState = new EnvironmentState(new Dictionary<string, object>());
        var action = new EnvironmentAction("noop", new Dictionary<string, object>());
        var obs = new Observation(envState, 0, false);
        var step = new EnvironmentStep(0, envState, action, obs, DateTime.UtcNow);

        var evt = new EnvironmentStepEvent(Guid.NewGuid(), Guid.NewGuid(), step, DateTime.UtcNow);

        evt.Kind.Should().Be("EnvironmentStep");
    }

    [Fact]
    public void RecordEquality_SameValues_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var episodeId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var envState = new EnvironmentState(new Dictionary<string, object>());
        var action = new EnvironmentAction("a", new Dictionary<string, object>());
        var obs = new Observation(envState, 0, false);
        var step = new EnvironmentStep(1, envState, action, obs, timestamp);

        var evt1 = new EnvironmentStepEvent(id, episodeId, step, timestamp);
        var evt2 = new EnvironmentStepEvent(id, episodeId, step, timestamp);

        evt1.Should().Be(evt2);
    }
}
