using Ouroboros.Agent.MetaAI.WorldModel;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI.WorldModel;

[Trait("Category", "Unit")]
public class WorldModelAdditionalTests
{
    [Fact]
    public void WorldTransition_ZeroReward_IsAllowed()
    {
        // Arrange
        var from = new WorldState(Guid.NewGuid(), new Dictionary<string, object>(), DateTime.UtcNow);
        var to = new WorldState(Guid.NewGuid(), new Dictionary<string, object>(), DateTime.UtcNow);

        // Act
        var transition = new WorldTransition(from, new AgentAction("noop"), to, 0.0);

        // Assert
        transition.Reward.Should().Be(0.0);
    }

    [Fact]
    public void LearnedWorldModel_WithExpression_UpdatesTrainingSamples()
    {
        // Arrange
        var original = new LearnedWorldModel(
            Guid.NewGuid(), "model", ModelArchitecture.MLP, 0.8, 1000, DateTime.UtcNow);

        // Act
        var modified = original with { TrainingSamples = 5000 };

        // Assert
        modified.TrainingSamples.Should().Be(5000);
        modified.Name.Should().Be("model");
    }

    [Fact]
    public void AgentAction_WithParameters_ParametersAreAccessible()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            ["speed"] = 10.0,
            ["direction"] = "north"
        };

        // Act
        var action = new AgentAction("move", parameters);

        // Assert
        action.Parameters!["speed"].Should().Be(10.0);
        action.Parameters!["direction"].Should().Be("north");
    }

    [Fact]
    public void ActionPlan_WithZeroConfidence_IsValid()
    {
        // Act
        var plan = new ActionPlan(new List<AgentAction>(), 0.0, 0.0, 0);

        // Assert
        plan.Confidence.Should().Be(0.0);
        plan.LookaheadDepth.Should().Be(0);
    }

    [Fact]
    public void WorldState_WithEmptyFeatures_IsValid()
    {
        // Act
        var state = new WorldState(Guid.NewGuid(), new Dictionary<string, object>(), DateTime.UtcNow);

        // Assert
        state.Features.Should().BeEmpty();
    }
}
