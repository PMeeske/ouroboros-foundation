using Ouroboros.Domain.Embodied;

namespace Ouroboros.Tests.Domain.Embodied;

[Trait("Category", "Unit")]
public class EmbodiedRecordEqualityTests
{
    [Fact]
    public void ActionResult_WithExpression_ChangesReward()
    {
        var state = SensorState.Default();
        var result = new ActionResult(true, state, 1.0, false);

        var modified = result with { Reward = 5.0 };

        modified.Reward.Should().Be(5.0);
        result.Reward.Should().Be(1.0);
    }

    [Fact]
    public void ActionResult_WithExpression_ChangesEpisodeTerminated()
    {
        var state = SensorState.Default();
        var result = new ActionResult(true, state, 1.0, false);

        var modified = result with { EpisodeTerminated = true };

        modified.EpisodeTerminated.Should().BeTrue();
        result.EpisodeTerminated.Should().BeFalse();
    }

    [Fact]
    public void ActionResult_Equality_SameValues()
    {
        var state = SensorState.Default();
        var a = new ActionResult(true, state, 1.0, false);
        var b = new ActionResult(true, state, 1.0, false);

        a.Should().Be(b);
    }

    [Fact]
    public void ActionResult_Equality_DifferentReward_NotEqual()
    {
        var state = SensorState.Default();
        var a = new ActionResult(true, state, 1.0, false);
        var b = new ActionResult(true, state, 2.0, false);

        a.Should().NotBe(b);
    }

    [Fact]
    public void EmbodiedTransition_WithExpression_ChangesReward()
    {
        var before = SensorState.Default();
        var action = EmbodiedAction.NoOp();
        var after = SensorState.Default();

        var transition = new EmbodiedTransition(before, action, after, 0.5, false);
        var modified = transition with { Reward = 2.0 };

        modified.Reward.Should().Be(2.0);
        transition.Reward.Should().Be(0.5);
    }

    [Fact]
    public void EmbodiedTransition_WithExpression_ChangesTerminal()
    {
        var before = SensorState.Default();
        var action = EmbodiedAction.NoOp();
        var after = SensorState.Default();

        var transition = new EmbodiedTransition(before, action, after, 0.5, false);
        var modified = transition with { Terminal = true };

        modified.Terminal.Should().BeTrue();
        transition.Terminal.Should().BeFalse();
    }

    [Fact]
    public void EnvironmentConfig_WithExpression_ChangesSceneName()
    {
        var config = new EnvironmentConfig(
            "OldScene",
            new Dictionary<string, object>(),
            new List<string> { "move" },
            EnvironmentType.Unity);

        var modified = config with { SceneName = "NewScene" };

        modified.SceneName.Should().Be("NewScene");
        config.SceneName.Should().Be("OldScene");
    }

    [Fact]
    public void EnvironmentConfig_WithExpression_ChangesType()
    {
        var config = new EnvironmentConfig(
            "Scene",
            new Dictionary<string, object>(),
            new List<string>(),
            EnvironmentType.Unity);

        var modified = config with { Type = EnvironmentType.Simulation };

        modified.Type.Should().Be(EnvironmentType.Simulation);
        config.Type.Should().Be(EnvironmentType.Unity);
    }

    [Fact]
    public void EnvironmentHandle_WithExpression_ChangesIsRunning()
    {
        var handle = new EnvironmentHandle(Guid.NewGuid(), "Scene", EnvironmentType.Gym, true);
        var modified = handle with { IsRunning = false };

        modified.IsRunning.Should().BeFalse();
        handle.IsRunning.Should().BeTrue();
    }

    [Fact]
    public void EnvironmentInfo_WithExpression_ChangesDescription()
    {
        var info = new EnvironmentInfo(
            "Env", "Old description",
            new List<string>(), new List<string>(),
            EnvironmentType.Custom);

        var modified = info with { Description = "New description" };

        modified.Description.Should().Be("New description");
        info.Description.Should().Be("Old description");
    }

    [Fact]
    public void EnvironmentInfo_Equality_SameValues()
    {
        var actions = new List<string> { "move" };
        var observations = new List<string> { "pos" };

        var a = new EnvironmentInfo("E", "D", actions, observations, EnvironmentType.Gym);
        var b = new EnvironmentInfo("E", "D", actions, observations, EnvironmentType.Gym);

        a.Should().Be(b);
    }

    [Fact]
    public void EnvironmentType_HasFourValues()
    {
        Enum.GetValues<EnvironmentType>().Should().HaveCount(4);
    }
}
