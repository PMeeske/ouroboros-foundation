using Ouroboros.Abstractions;
using Ouroboros.Domain.Environment;
using Ouroboros.Domain.Reinforcement;

namespace Ouroboros.Tests.Reinforcement;

[Trait("Category", "Unit")]
public class BanditPolicyTests
{
    private static EnvironmentState MakeState() =>
        new(new Dictionary<string, object> { ["s"] = "0" });

    private static EnvironmentAction MakeAction(string type = "action") =>
        new(type, new Dictionary<string, object>());

    [Fact]
    public async Task SelectActionAsync_WithActions_ShouldReturnSuccess()
    {
        var policy = new BanditPolicy();
        var actions = new List<EnvironmentAction> { MakeAction("a"), MakeAction("b") };

        var result = await policy.SelectActionAsync(MakeState(), actions);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SelectActionAsync_EmptyActions_ShouldReturnFailure()
    {
        var policy = new BanditPolicy();
        var result = await policy.SelectActionAsync(MakeState(), new List<EnvironmentAction>());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnSuccess()
    {
        var policy = new BanditPolicy();
        var obs = new Observation(MakeState(), 1.0, false);

        var result = await policy.UpdateAsync(MakeState(), MakeAction(), obs);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SelectAction_UntriedActions_ShouldBePrioritized()
    {
        var policy = new BanditPolicy();
        var triedAction = MakeAction("tried");
        var untriedAction = MakeAction("untried");

        // Update tried action once
        await policy.UpdateAsync(MakeState(), triedAction, new Observation(MakeState(), 0.5, false));

        // Untried should be selected (infinite UCB)
        var result = await policy.SelectActionAsync(MakeState(),
            new List<EnvironmentAction> { triedAction, untriedAction });

        result.Value.ActionType.Should().Be("untried");
    }
}
