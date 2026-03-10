using Ouroboros.Domain.Environment;

namespace Ouroboros.Tests.Domain.Environment;

[Trait("Category", "Unit")]
public class EnvironmentRecordEqualityTests
{
    [Fact]
    public void EnvironmentAction_Equality_SameValues()
    {
        var a = new EnvironmentAction("move");
        var b = new EnvironmentAction("move");

        a.Should().Be(b);
    }

    [Fact]
    public void EnvironmentAction_Equality_DifferentType_NotEqual()
    {
        var a = new EnvironmentAction("move");
        var b = new EnvironmentAction("jump");

        a.Should().NotBe(b);
    }

    [Fact]
    public void EnvironmentAction_WithExpression_ChangesActionType()
    {
        var action = new EnvironmentAction("move");
        var modified = action with { ActionType = "jump" };

        modified.ActionType.Should().Be("jump");
        action.ActionType.Should().Be("move");
    }

    [Fact]
    public void EnvironmentAction_WithExpression_AddsParameters()
    {
        var action = new EnvironmentAction("move");
        var parameters = new Dictionary<string, object> { ["speed"] = 5 };
        var modified = action with { Parameters = parameters };

        modified.Parameters.Should().ContainKey("speed");
        action.Parameters.Should().BeNull();
    }

    [Fact]
    public void EnvironmentState_Equality_SameValues()
    {
        var data = new Dictionary<string, object> { ["key"] = "value" };
        var a = new EnvironmentState(data, false);
        var b = new EnvironmentState(data, false);

        a.Should().Be(b);
    }

    [Fact]
    public void EnvironmentState_WithExpression_ChangesIsTerminal()
    {
        var state = new EnvironmentState(new Dictionary<string, object>());
        var modified = state with { IsTerminal = true };

        modified.IsTerminal.Should().BeTrue();
        state.IsTerminal.Should().BeFalse();
    }

    [Fact]
    public void Observation_Equality_SameValues()
    {
        var state = new EnvironmentState(new Dictionary<string, object>());
        var a = new Observation(state, 1.5, false);
        var b = new Observation(state, 1.5, false);

        a.Should().Be(b);
    }

    [Fact]
    public void Observation_WithExpression_ChangesReward()
    {
        var state = new EnvironmentState(new Dictionary<string, object>());
        var obs = new Observation(state, 1.0, false);
        var modified = obs with { Reward = 5.0 };

        modified.Reward.Should().Be(5.0);
        obs.Reward.Should().Be(1.0);
    }

    [Fact]
    public void Observation_WithExpression_ChangesIsTerminal()
    {
        var state = new EnvironmentState(new Dictionary<string, object>());
        var obs = new Observation(state, 0, false);
        var modified = obs with { IsTerminal = true };

        modified.IsTerminal.Should().BeTrue();
    }

    [Fact]
    public void EnvironmentStep_Equality_SameValues()
    {
        var state = new EnvironmentState(new Dictionary<string, object>());
        var action = new EnvironmentAction("noop");
        var obs = new Observation(state, 0, false);
        var timestamp = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new EnvironmentStep(1, state, action, obs, timestamp);
        var b = new EnvironmentStep(1, state, action, obs, timestamp);

        a.Should().Be(b);
    }

    [Fact]
    public void EnvironmentStep_WithExpression_ChangesStepNumber()
    {
        var state = new EnvironmentState(new Dictionary<string, object>());
        var action = new EnvironmentAction("noop");
        var obs = new Observation(state, 0, false);

        var step = new EnvironmentStep(1, state, action, obs, DateTime.UtcNow);
        var modified = step with { StepNumber = 42 };

        modified.StepNumber.Should().Be(42);
        step.StepNumber.Should().Be(1);
    }

    [Fact]
    public void EnvironmentStep_WithMetadata_SetsMetadata()
    {
        var state = new EnvironmentState(new Dictionary<string, object>());
        var action = new EnvironmentAction("noop");
        var obs = new Observation(state, 0, false);
        var metadata = new Dictionary<string, object> { ["debug"] = true };

        var step = new EnvironmentStep(0, state, action, obs, DateTime.UtcNow, metadata);

        step.Metadata.Should().ContainKey("debug");
        step.Metadata!["debug"].Should().Be(true);
    }

    [Fact]
    public void EnvironmentStep_GetHashCode_EqualRecords_HaveSameHash()
    {
        var state = new EnvironmentState(new Dictionary<string, object>());
        var action = new EnvironmentAction("noop");
        var obs = new Observation(state, 0, false);
        var timestamp = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new EnvironmentStep(1, state, action, obs, timestamp);
        var b = new EnvironmentStep(1, state, action, obs, timestamp);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
