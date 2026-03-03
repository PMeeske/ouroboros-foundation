using FluentAssertions;
using Ouroboros.Domain.Environment;
using Ouroboros.Domain.Reinforcement;
using Xunit;

namespace Ouroboros.Tests.Domain.Reinforcement;

[Trait("Category", "Unit")]
public class EpsilonGreedyPolicyTests
{
    private static EnvironmentState CreateState(Dictionary<string, object>? data = null)
    {
        return new EnvironmentState(data ?? new Dictionary<string, object> { ["x"] = 1 });
    }

    private static EnvironmentAction CreateAction(string type, Dictionary<string, object>? parameters = null)
    {
        return new EnvironmentAction(type, parameters);
    }

    private static Observation CreateObservation(double reward, bool terminal = false)
    {
        return new Observation(
            CreateState(new Dictionary<string, object> { ["x"] = 2 }),
            reward,
            terminal);
    }

    // ===== Constructor =====

    [Fact]
    public void Constructor_ValidEpsilon_ShouldNotThrow()
    {
        var policy = new EpsilonGreedyPolicy(0.5);

        // should not throw
        policy.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_EpsilonBelowZero_ShouldThrow()
    {
        var act = () => new EpsilonGreedyPolicy(-0.1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_EpsilonAboveOne_ShouldThrow()
    {
        var act = () => new EpsilonGreedyPolicy(1.1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_EpsilonAtBoundaries_ShouldNotThrow()
    {
        var p0 = new EpsilonGreedyPolicy(0.0);
        var p1 = new EpsilonGreedyPolicy(1.0);

        p0.Should().NotBeNull();
        p1.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithSeed_ShouldNotThrow()
    {
        var policy = new EpsilonGreedyPolicy(0.5, seed: 42);

        policy.Should().NotBeNull();
    }

    // ===== SelectActionAsync =====

    [Fact]
    public async Task SelectActionAsync_WithNoActions_ShouldFail()
    {
        var policy = new EpsilonGreedyPolicy(0.1, seed: 42);
        var state = CreateState();

        var result = await policy.SelectActionAsync(state, Array.Empty<EnvironmentAction>());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No available actions");
    }

    [Fact]
    public async Task SelectActionAsync_WithNullActions_ShouldFail()
    {
        var policy = new EpsilonGreedyPolicy(0.1, seed: 42);
        var state = CreateState();

        var result = await policy.SelectActionAsync(state, null!);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task SelectActionAsync_WithSingleAction_ShouldReturnThatAction()
    {
        var policy = new EpsilonGreedyPolicy(0.1, seed: 42);
        var state = CreateState();
        var actions = new[] { CreateAction("move") };

        var result = await policy.SelectActionAsync(state, actions);

        result.IsSuccess.Should().BeTrue();
        result.Value.ActionType.Should().Be("move");
    }

    [Fact]
    public async Task SelectActionAsync_ShouldReturnOneOfAvailableActions()
    {
        var policy = new EpsilonGreedyPolicy(0.5, seed: 42);
        var state = CreateState();
        var actions = new[] { CreateAction("left"), CreateAction("right"), CreateAction("up") };

        var result = await policy.SelectActionAsync(state, actions);

        result.IsSuccess.Should().BeTrue();
        actions.Select(a => a.ActionType).Should().Contain(result.Value.ActionType);
    }

    [Fact]
    public async Task SelectActionAsync_WithZeroEpsilon_ShouldAlwaysExploit()
    {
        var policy = new EpsilonGreedyPolicy(0.0, seed: 42);
        var state = CreateState();
        var actions = new[] { CreateAction("a"), CreateAction("b") };

        // Train one action to have higher Q-value
        await policy.UpdateAsync(state, actions[0], CreateObservation(10.0));

        var results = new List<string>();
        for (int i = 0; i < 20; i++)
        {
            var result = await policy.SelectActionAsync(state, actions);
            results.Add(result.Value.ActionType);
        }

        // With epsilon=0, should always exploit (choose action with highest Q-value)
        results.Should().OnlyContain(a => a == "a");
    }

    // ===== UpdateAsync =====

    [Fact]
    public async Task UpdateAsync_ShouldSucceed()
    {
        var policy = new EpsilonGreedyPolicy(0.1, seed: 42);
        var state = CreateState();
        var action = CreateAction("move");
        var obs = CreateObservation(1.0);

        var result = await policy.UpdateAsync(state, action, obs);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_TerminalState_ShouldHandle()
    {
        var policy = new EpsilonGreedyPolicy(0.1, seed: 42);
        var state = CreateState();
        var action = CreateAction("move");
        var obs = CreateObservation(1.0, terminal: true);

        var result = await policy.UpdateAsync(state, action, obs);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldAffectFutureSelections()
    {
        var policy = new EpsilonGreedyPolicy(0.0, seed: 42); // epsilon=0 always exploits
        var state = CreateState();
        var actions = new[] { CreateAction("bad"), CreateAction("good") };

        // Make "good" action have high Q-value
        for (int i = 0; i < 10; i++)
        {
            await policy.UpdateAsync(state, actions[1], CreateObservation(10.0));
            await policy.UpdateAsync(state, actions[0], CreateObservation(-1.0));
        }

        var result = await policy.SelectActionAsync(state, actions);

        result.Value.ActionType.Should().Be("good");
    }
}

[Trait("Category", "Unit")]
public class BanditPolicyTests
{
    private static EnvironmentState CreateState(Dictionary<string, object>? data = null)
    {
        return new EnvironmentState(data ?? new Dictionary<string, object> { ["x"] = 1 });
    }

    private static EnvironmentAction CreateAction(string type, Dictionary<string, object>? parameters = null)
    {
        return new EnvironmentAction(type, parameters);
    }

    private static Observation CreateObservation(double reward, bool terminal = false)
    {
        return new Observation(
            CreateState(new Dictionary<string, object> { ["x"] = 2 }),
            reward,
            terminal);
    }

    [Fact]
    public void Constructor_DefaultExplorationFactor_ShouldNotThrow()
    {
        var policy = new BanditPolicy();

        policy.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_CustomExplorationFactor_ShouldNotThrow()
    {
        var policy = new BanditPolicy(explorationFactor: 2.0);

        policy.Should().NotBeNull();
    }

    [Fact]
    public async Task SelectActionAsync_WithNoActions_ShouldFail()
    {
        var policy = new BanditPolicy();
        var state = CreateState();

        var result = await policy.SelectActionAsync(state, Array.Empty<EnvironmentAction>());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No available actions");
    }

    [Fact]
    public async Task SelectActionAsync_WithNullActions_ShouldFail()
    {
        var policy = new BanditPolicy();
        var state = CreateState();

        var result = await policy.SelectActionAsync(state, null!);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task SelectActionAsync_UntriedActions_ShouldPrioritize()
    {
        var policy = new BanditPolicy();
        var state = CreateState();
        var actions = new[] { CreateAction("tried"), CreateAction("untried") };

        // Try one action
        await policy.UpdateAsync(state, actions[0], CreateObservation(1.0));

        // Untried action should get priority (infinite UCB)
        var result = await policy.SelectActionAsync(state, actions);

        result.IsSuccess.Should().BeTrue();
        result.Value.ActionType.Should().Be("untried");
    }

    [Fact]
    public async Task SelectActionAsync_ShouldReturnOneOfAvailableActions()
    {
        var policy = new BanditPolicy();
        var state = CreateState();
        var actions = new[] { CreateAction("a"), CreateAction("b") };

        var result = await policy.SelectActionAsync(state, actions);

        result.IsSuccess.Should().BeTrue();
        actions.Select(a => a.ActionType).Should().Contain(result.Value.ActionType);
    }

    [Fact]
    public async Task UpdateAsync_ShouldSucceed()
    {
        var policy = new BanditPolicy();
        var state = CreateState();
        var action = CreateAction("move");
        var obs = CreateObservation(1.0);

        var result = await policy.UpdateAsync(state, action, obs);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_MultipleTimes_ShouldUpdateStats()
    {
        var policy = new BanditPolicy();
        var state = CreateState();
        var actions = new[] { CreateAction("good"), CreateAction("bad") };

        // Train good action with high rewards
        for (int i = 0; i < 20; i++)
        {
            await policy.UpdateAsync(state, actions[0], CreateObservation(10.0));
            await policy.UpdateAsync(state, actions[1], CreateObservation(-1.0));
        }

        // After many trials, UCB should favor the good action
        var result = await policy.SelectActionAsync(state, actions);

        result.IsSuccess.Should().BeTrue();
        result.Value.ActionType.Should().Be("good");
    }

    [Fact]
    public async Task SelectActionAsync_ActionWithParameters_ShouldGenerateUniqueKey()
    {
        var policy = new BanditPolicy();
        var state = CreateState();
        var action1 = CreateAction("move", new Dictionary<string, object> { ["dir"] = "left" });
        var action2 = CreateAction("move", new Dictionary<string, object> { ["dir"] = "right" });

        await policy.UpdateAsync(state, action1, CreateObservation(10.0));

        // action2 is untried, should get priority
        var result = await policy.SelectActionAsync(state, new[] { action1, action2 });

        result.IsSuccess.Should().BeTrue();
        result.Value.Parameters!["dir"].Should().Be("right");
    }
}
