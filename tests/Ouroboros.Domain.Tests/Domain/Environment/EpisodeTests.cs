using Ouroboros.Domain.Environment;

namespace Ouroboros.Tests.Domain.Environment;

[Trait("Category", "Unit")]
public sealed class EpisodeTests
{
    private static EnvironmentState CreateState(bool terminal = false) =>
        new(new Dictionary<string, object> { ["pos"] = 0 }, terminal);

    private static EnvironmentAction CreateAction(string type = "move") =>
        new(type);

    private static Observation CreateObservation(double reward = 0.0, bool terminal = false) =>
        new(CreateState(terminal), reward, terminal);

    private static EnvironmentStep CreateStep(int num) =>
        new(num, CreateState(), CreateAction(), CreateObservation(), DateTime.UtcNow);

    [Fact]
    public void Episode_StepCount_Returns_Count()
    {
        List<EnvironmentStep> steps = new() { CreateStep(0), CreateStep(1), CreateStep(2) };
        Episode episode = new(Guid.NewGuid(), "test_env", steps, 1.5, DateTime.UtcNow);

        episode.StepCount.Should().Be(3);
    }

    [Fact]
    public void Episode_IsComplete_False_When_No_EndTime()
    {
        Episode episode = new(Guid.NewGuid(), "test_env", new List<EnvironmentStep>(), 0, DateTime.UtcNow);

        episode.IsComplete.Should().BeFalse();
        episode.Duration.Should().BeNull();
    }

    [Fact]
    public void Episode_IsComplete_True_When_EndTime_Set()
    {
        DateTime start = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime end = new(2026, 1, 1, 0, 5, 0, DateTimeKind.Utc);

        Episode episode = new(Guid.NewGuid(), "env", new List<EnvironmentStep>(), 0, start, end);

        episode.IsComplete.Should().BeTrue();
        episode.Duration.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void Episode_Success_Default_False()
    {
        Episode episode = new(Guid.NewGuid(), "env", new List<EnvironmentStep>(), 0, DateTime.UtcNow);

        episode.Success.Should().BeFalse();
    }

    [Fact]
    public void Episode_Success_Can_Be_True()
    {
        Episode episode = new(Guid.NewGuid(), "env", new List<EnvironmentStep>(), 10.0, DateTime.UtcNow, DateTime.UtcNow, Success: true);

        episode.Success.Should().BeTrue();
    }

    [Fact]
    public void Episode_Metadata_Default_Null()
    {
        Episode episode = new(Guid.NewGuid(), "env", new List<EnvironmentStep>(), 0, DateTime.UtcNow);

        episode.Metadata.Should().BeNull();
    }

    [Fact]
    public void Episode_Metadata_Can_Be_Set()
    {
        Dictionary<string, object> meta = new() { ["algorithm"] = "PPO" };
        Episode episode = new(Guid.NewGuid(), "env", new List<EnvironmentStep>(), 0, DateTime.UtcNow, Metadata: meta);

        episode.Metadata.Should().ContainKey("algorithm");
    }

    [Fact]
    public void Episode_TotalReward_Set()
    {
        Episode episode = new(Guid.NewGuid(), "env", new List<EnvironmentStep>(), 42.5, DateTime.UtcNow);

        episode.TotalReward.Should().Be(42.5);
    }

    [Fact]
    public void EnvironmentStep_Properties()
    {
        EnvironmentState state = CreateState();
        EnvironmentAction action = CreateAction("jump");
        Observation obs = CreateObservation(1.0);
        DateTime now = DateTime.UtcNow;

        EnvironmentStep step = new(5, state, action, obs, now);

        step.StepNumber.Should().Be(5);
        step.State.Should().Be(state);
        step.Action.ActionType.Should().Be("jump");
        step.Observation.Reward.Should().Be(1.0);
        step.Metadata.Should().BeNull();
    }

    [Fact]
    public void EnvironmentStep_Metadata_Can_Be_Set()
    {
        Dictionary<string, object> meta = new() { ["exploration"] = true };
        EnvironmentStep step = new(0, CreateState(), CreateAction(), CreateObservation(), DateTime.UtcNow, meta);

        step.Metadata.Should().ContainKey("exploration");
    }

    [Fact]
    public void EnvironmentState_IsTerminal_Default_False()
    {
        EnvironmentState state = new(new Dictionary<string, object>());
        state.IsTerminal.Should().BeFalse();
    }

    [Fact]
    public void EnvironmentState_IsTerminal_Can_Be_True()
    {
        EnvironmentState state = new(new Dictionary<string, object>(), true);
        state.IsTerminal.Should().BeTrue();
    }

    [Fact]
    public void EnvironmentAction_Parameters_Default_Null()
    {
        EnvironmentAction action = new("noop");
        action.Parameters.Should().BeNull();
    }

    [Fact]
    public void Observation_Properties()
    {
        EnvironmentState state = CreateState();
        Observation obs = new(state, 0.5, false, new Dictionary<string, object> { ["info"] = "ok" });

        obs.State.Should().Be(state);
        obs.Reward.Should().Be(0.5);
        obs.IsTerminal.Should().BeFalse();
        obs.Info.Should().ContainKey("info");
    }
}
