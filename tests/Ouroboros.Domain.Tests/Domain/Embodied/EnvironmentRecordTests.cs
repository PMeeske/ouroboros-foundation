using Ouroboros.Domain.Embodied;

namespace Ouroboros.Tests.Domain.Embodied;

[Trait("Category", "Unit")]
public sealed class EnvironmentRecordTests
{
    [Fact]
    public void EnvironmentHandle_Creation()
    {
        Guid id = Guid.NewGuid();
        EnvironmentHandle handle = new(id, "TestScene", EnvironmentType.Simulated, true);

        handle.Id.Should().Be(id);
        handle.SceneName.Should().Be("TestScene");
        handle.Type.Should().Be(EnvironmentType.Simulated);
        handle.IsRunning.Should().BeTrue();
    }

    [Fact]
    public void EnvironmentConfig_Creation()
    {
        Dictionary<string, object> parameters = new() { ["gravity"] = 9.81 };
        List<string> actions = new() { "move", "jump" };

        EnvironmentConfig config = new("TestScene", parameters, actions, EnvironmentType.Virtual);

        config.SceneName.Should().Be("TestScene");
        config.Parameters.Should().ContainKey("gravity");
        config.AvailableActions.Should().HaveCount(2);
        config.Type.Should().Be(EnvironmentType.Virtual);
    }

    [Fact]
    public void EnvironmentInfo_Creation()
    {
        List<string> actions = new() { "pick", "place" };
        List<string> observations = new() { "position", "velocity" };

        EnvironmentInfo info = new("Robotics", "Robot arm simulation", actions, observations, EnvironmentType.Physical);

        info.Name.Should().Be("Robotics");
        info.Description.Should().Contain("Robot");
        info.AvailableActions.Should().HaveCount(2);
        info.Observations.Should().HaveCount(2);
        info.Type.Should().Be(EnvironmentType.Physical);
    }

    [Fact]
    public void EmbodiedTransition_Creation()
    {
        SensorState before = new(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1), new Dictionary<string, double>());
        SensorState after = new(new Vector3(1, 0, 0), new Quaternion(0, 0, 0, 1), new Dictionary<string, double>());
        EmbodiedAction action = new("move_right", new Dictionary<string, object>());

        EmbodiedTransition transition = new(before, action, after, 0.5, false);

        transition.StateBefore.Should().Be(before);
        transition.StateAfter.Should().Be(after);
        transition.Action.Should().Be(action);
        transition.Reward.Should().Be(0.5);
        transition.Terminal.Should().BeFalse();
    }

    [Fact]
    public void ActionResult_Success()
    {
        SensorState state = new(new Vector3(1, 1, 1), new Quaternion(0, 0, 0, 1), new Dictionary<string, double>());

        ActionResult result = new(true, state, 1.0, false);

        result.Success.Should().BeTrue();
        result.Reward.Should().Be(1.0);
        result.EpisodeTerminated.Should().BeFalse();
        result.FailureReason.Should().BeNull();
    }

    [Fact]
    public void ActionResult_Failure()
    {
        SensorState state = new(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 1), new Dictionary<string, double>());

        ActionResult result = new(false, state, -1.0, true, "Collision detected");

        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be("Collision detected");
        result.EpisodeTerminated.Should().BeTrue();
    }

    [Fact]
    public void EnvironmentHandle_Equality()
    {
        Guid id = Guid.NewGuid();
        EnvironmentHandle a = new(id, "Scene", EnvironmentType.Simulated, true);
        EnvironmentHandle b = new(id, "Scene", EnvironmentType.Simulated, true);

        a.Should().Be(b);
    }

    [Fact]
    public void EnvironmentHandle_Inequality()
    {
        EnvironmentHandle a = new(Guid.NewGuid(), "Scene", EnvironmentType.Simulated, true);
        EnvironmentHandle b = new(Guid.NewGuid(), "Scene", EnvironmentType.Simulated, true);

        a.Should().NotBe(b);
    }
}
