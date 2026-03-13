using Ouroboros.Abstractions;
using Ouroboros.Domain.Environment;
using Ouroboros.Domain.Reinforcement;

namespace Ouroboros.Tests.Reinforcement;

[Trait("Category", "Unit")]
public class EpsilonGreedyPolicyTests
{
    private static EnvironmentState MakeState(string key = "pos", string value = "0") =>
        new(new Dictionary<string, object> { [key] = value });

    private static EnvironmentAction MakeAction(string type = "move") =>
        new(type, new Dictionary<string, object>());

    [Fact]
    public void Constructor_ValidEpsilon_ShouldNotThrow()
    {
        var act = () => new EpsilonGreedyPolicy(0.5);
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_InvalidEpsilon_ShouldThrow()
    {
        var act = () => new EpsilonGreedyPolicy(1.5);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_NegativeEpsilon_ShouldThrow()
    {
        var act = () => new EpsilonGreedyPolicy(-0.1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task SelectActionAsync_WithActions_ShouldReturnSuccess()
    {
        var policy = new EpsilonGreedyPolicy(0.1, seed: 42);
        var state = MakeState();
        var actions = new List<EnvironmentAction> { MakeAction("a"), MakeAction("b") };

        var result = await policy.SelectActionAsync(state, actions);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SelectActionAsync_EmptyActions_ShouldReturnFailure()
    {
        var policy = new EpsilonGreedyPolicy(0.1, seed: 42);
        var result = await policy.SelectActionAsync(MakeState(), new List<EnvironmentAction>());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnSuccess()
    {
        var policy = new EpsilonGreedyPolicy(0.1, seed: 42);
        var obs = new Observation(MakeState("next", "1"), 1.0, false);

        var result = await policy.UpdateAsync(MakeState(), MakeAction(), obs);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SelectAction_AfterUpdates_ShouldPreferHighRewardActions()
    {
        var policy = new EpsilonGreedyPolicy(0.0, seed: 42); // pure greedy
        var state = MakeState();
        var goodAction = MakeAction("good");
        var badAction = MakeAction("bad");

        // Train: good action gets high reward
        for (int i = 0; i < 10; i++)
        {
            await policy.UpdateAsync(state, goodAction, new Observation(state, 10.0, false));
            await policy.UpdateAsync(state, badAction, new Observation(state, -1.0, false));
        }

        var result = await policy.SelectActionAsync(state, new List<EnvironmentAction> { goodAction, badAction });

        result.Value.ActionType.Should().Be("good");
    }
}
