namespace Ouroboros.Tests.Domain.Environment;

using Ouroboros.Domain.Environment;

[Trait("Category", "Unit")]
public class EnvironmentRecordTests
{
    [Fact]
    public void EnvironmentAction_Constructor_SetsProperties()
    {
        // Act
        var action = new EnvironmentAction("move");

        // Assert
        action.ActionType.Should().Be("move");
        action.Parameters.Should().BeNull();
    }

    [Fact]
    public void EnvironmentAction_WithParameters_SetsProperties()
    {
        // Arrange
        var parameters = new Dictionary<string, object> { ["direction"] = "north", ["speed"] = 5 };

        // Act
        var action = new EnvironmentAction("move", parameters);

        // Assert
        action.Parameters.Should().ContainKey("direction");
        action.Parameters!["speed"].Should().Be(5);
    }

    [Fact]
    public void EnvironmentState_Constructor_SetsProperties()
    {
        // Arrange
        var stateData = new Dictionary<string, object> { ["position"] = 0, ["health"] = 100 };

        // Act
        var state = new EnvironmentState(stateData);

        // Assert
        state.StateData.Should().HaveCount(2);
        state.IsTerminal.Should().BeFalse();
    }

    [Fact]
    public void EnvironmentState_Terminal_SetsFlag()
    {
        // Act
        var state = new EnvironmentState(new Dictionary<string, object>(), IsTerminal: true);

        // Assert
        state.IsTerminal.Should().BeTrue();
    }

    [Fact]
    public void Observation_Constructor_SetsAllProperties()
    {
        // Arrange
        var state = new EnvironmentState(new Dictionary<string, object>());

        // Act
        var obs = new Observation(state, 1.5, false);

        // Assert
        obs.State.Should().Be(state);
        obs.Reward.Should().Be(1.5);
        obs.IsTerminal.Should().BeFalse();
        obs.Info.Should().BeNull();
    }

    [Fact]
    public void Observation_WithInfo_SetsInfo()
    {
        // Arrange
        var state = new EnvironmentState(new Dictionary<string, object>());
        var info = new Dictionary<string, object> { ["msg"] = "success" };

        // Act
        var obs = new Observation(state, 0, true, info);

        // Assert
        obs.Info.Should().ContainKey("msg");
    }

    [Fact]
    public void EnvironmentStep_Constructor_SetsAllProperties()
    {
        // Arrange
        var state = new EnvironmentState(new Dictionary<string, object>());
        var action = new EnvironmentAction("noop");
        var obs = new Observation(state, 0, false);
        var timestamp = DateTime.UtcNow;

        // Act
        var step = new EnvironmentStep(5, state, action, obs, timestamp);

        // Assert
        step.StepNumber.Should().Be(5);
        step.State.Should().Be(state);
        step.Action.Should().Be(action);
        step.Observation.Should().Be(obs);
        step.Timestamp.Should().Be(timestamp);
        step.Metadata.Should().BeNull();
    }

    [Fact]
    public void Episode_Duration_WithEndTime_CalculatesCorrectly()
    {
        // Arrange
        var start = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2025, 1, 1, 10, 5, 0, DateTimeKind.Utc);

        // Act
        var episode = new Episode(
            Guid.NewGuid(), "TestEnv",
            new List<EnvironmentStep>(), 5.0,
            start, end, true);

        // Assert
        episode.Duration.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void Episode_Duration_WithoutEndTime_IsNull()
    {
        // Act
        var episode = new Episode(
            Guid.NewGuid(), "TestEnv",
            new List<EnvironmentStep>(), 0, DateTime.UtcNow);

        // Assert
        episode.Duration.Should().BeNull();
    }

    [Fact]
    public void Episode_StepCount_ReturnsCorrectCount()
    {
        // Arrange
        var state = new EnvironmentState(new Dictionary<string, object>());
        var action = new EnvironmentAction("noop");
        var obs = new Observation(state, 0, false);
        var steps = new List<EnvironmentStep>
        {
            new(0, state, action, obs, DateTime.UtcNow),
            new(1, state, action, obs, DateTime.UtcNow),
            new(2, state, action, obs, DateTime.UtcNow),
        };

        // Act
        var episode = new Episode(Guid.NewGuid(), "TestEnv", steps, 3.0, DateTime.UtcNow);

        // Assert
        episode.StepCount.Should().Be(3);
    }

    [Fact]
    public void Episode_IsComplete_WithEndTime_ReturnsTrue()
    {
        // Act
        var episode = new Episode(
            Guid.NewGuid(), "TestEnv",
            new List<EnvironmentStep>(), 0,
            DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1));

        // Assert
        episode.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void Episode_IsComplete_WithoutEndTime_ReturnsFalse()
    {
        // Act
        var episode = new Episode(
            Guid.NewGuid(), "TestEnv",
            new List<EnvironmentStep>(), 0, DateTime.UtcNow);

        // Assert
        episode.IsComplete.Should().BeFalse();
    }
}
