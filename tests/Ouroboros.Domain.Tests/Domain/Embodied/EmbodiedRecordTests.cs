namespace Ouroboros.Tests.Domain.Embodied;

using Ouroboros.Domain.Embodied;

[Trait("Category", "Unit")]
public class EmbodiedRecordTests
{
    [Fact]
    public void ActionResult_Success_SetsProperties()
    {
        // Arrange
        var sensorState = SensorState.Default();

        // Act
        var result = new ActionResult(true, sensorState, 1.5, false);

        // Assert
        result.Success.Should().BeTrue();
        result.ResultingState.Should().Be(sensorState);
        result.Reward.Should().Be(1.5);
        result.EpisodeTerminated.Should().BeFalse();
        result.FailureReason.Should().BeNull();
    }

    [Fact]
    public void ActionResult_Failure_SetsFailureReason()
    {
        // Act
        var result = new ActionResult(false, SensorState.Default(), -1.0, true, "Collision");

        // Assert
        result.Success.Should().BeFalse();
        result.FailureReason.Should().Be("Collision");
        result.EpisodeTerminated.Should().BeTrue();
    }

    [Fact]
    public void EmbodiedTransition_SetsAllProperties()
    {
        // Arrange
        var before = SensorState.Default();
        var action = EmbodiedAction.Move(Vector3.UnitX);
        var after = SensorState.Default();

        // Act
        var transition = new EmbodiedTransition(before, action, after, 0.5, false);

        // Assert
        transition.StateBefore.Should().Be(before);
        transition.Action.Should().Be(action);
        transition.StateAfter.Should().Be(after);
        transition.Reward.Should().Be(0.5);
        transition.Terminal.Should().BeFalse();
    }

    [Fact]
    public void EnvironmentConfig_SetsAllProperties()
    {
        // Arrange
        var actions = new List<string> { "move", "rotate", "grab" };
        var parameters = new Dictionary<string, object> { ["gravity"] = -9.8 };

        // Act
        var config = new EnvironmentConfig("TestScene", parameters, actions, EnvironmentType.Unity);

        // Assert
        config.SceneName.Should().Be("TestScene");
        config.Parameters.Should().ContainKey("gravity");
        config.AvailableActions.Should().HaveCount(3);
        config.Type.Should().Be(EnvironmentType.Unity);
    }

    [Fact]
    public void EnvironmentHandle_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var handle = new EnvironmentHandle(id, "TestScene", EnvironmentType.Gym, true);

        // Assert
        handle.Id.Should().Be(id);
        handle.SceneName.Should().Be("TestScene");
        handle.Type.Should().Be(EnvironmentType.Gym);
        handle.IsRunning.Should().BeTrue();
    }

    [Fact]
    public void EnvironmentInfo_SetsAllProperties()
    {
        // Act
        var info = new EnvironmentInfo(
            "TestEnv", "A test environment",
            new List<string> { "move" },
            new List<string> { "position", "velocity" },
            EnvironmentType.Custom);

        // Assert
        info.Name.Should().Be("TestEnv");
        info.Description.Should().Be("A test environment");
        info.AvailableActions.Should().ContainSingle();
        info.Observations.Should().HaveCount(2);
        info.Type.Should().Be(EnvironmentType.Custom);
    }

    [Fact]
    public void Plan_SetsAllProperties()
    {
        // Arrange
        var actions = new List<EmbodiedAction> { EmbodiedAction.Move(Vector3.UnitX) };
        var states = new List<SensorState> { SensorState.Default() };

        // Act
        var plan = new Plan("Reach target", actions, states, 0.9, 10.5);

        // Assert
        plan.Goal.Should().Be("Reach target");
        plan.Actions.Should().HaveCount(1);
        plan.ExpectedStates.Should().HaveCount(1);
        plan.Confidence.Should().Be(0.9);
        plan.EstimatedReward.Should().Be(10.5);
    }

    [Theory]
    [InlineData(EnvironmentType.Unity)]
    [InlineData(EnvironmentType.Gym)]
    [InlineData(EnvironmentType.Custom)]
    [InlineData(EnvironmentType.Simulation)]
    public void EnvironmentType_AllValues_AreDefined(EnvironmentType type)
    {
        Enum.IsDefined(type).Should().BeTrue();
    }
}
