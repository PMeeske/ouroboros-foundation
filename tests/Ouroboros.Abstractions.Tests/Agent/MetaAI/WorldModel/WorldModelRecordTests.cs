using Ouroboros.Agent.MetaAI.WorldModel;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI.WorldModel;

[Trait("Category", "Unit")]
public class WorldModelRecordTests
{
    [Fact]
    public void AgentAction_AllPropertiesSet()
    {
        // Arrange
        var parameters = new Dictionary<string, object> { ["target"] = "resource-a" };

        // Act
        var action = new AgentAction("move", parameters);

        // Assert
        action.Name.Should().Be("move");
        action.Parameters.Should().ContainKey("target");
    }

    [Fact]
    public void AgentAction_DefaultParameters_IsNull()
    {
        // Act
        var action = new AgentAction("noop");

        // Assert
        action.Parameters.Should().BeNull();
    }

    [Fact]
    public void WorldState_AllPropertiesSet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var features = new Dictionary<string, object> { ["position"] = "A1" };
        var ts = DateTime.UtcNow;

        // Act
        var state = new WorldState(id, features, ts);

        // Assert
        state.Id.Should().Be(id);
        state.Features.Should().ContainKey("position");
        state.Timestamp.Should().Be(ts);
    }

    [Fact]
    public void WorldTransition_AllPropertiesSet()
    {
        // Arrange
        var from = new WorldState(Guid.NewGuid(), new Dictionary<string, object>(), DateTime.UtcNow);
        var to = new WorldState(Guid.NewGuid(), new Dictionary<string, object>(), DateTime.UtcNow);
        var action = new AgentAction("move");

        // Act
        var transition = new WorldTransition(from, action, to, 1.5);

        // Assert
        transition.FromState.Should().Be(from);
        transition.Action.Should().Be(action);
        transition.ToState.Should().Be(to);
        transition.Reward.Should().Be(1.5);
    }

    [Fact]
    public void ActionPlan_AllPropertiesSet()
    {
        // Arrange
        var actions = new List<AgentAction>
        {
            new AgentAction("move", new Dictionary<string, object> { ["dir"] = "north" }),
            new AgentAction("collect")
        };

        // Act
        var plan = new ActionPlan(actions, 5.0, 0.85, 3);

        // Assert
        plan.Actions.Should().HaveCount(2);
        plan.ExpectedReward.Should().Be(5.0);
        plan.Confidence.Should().Be(0.85);
        plan.LookaheadDepth.Should().Be(3);
    }

    [Fact]
    public void LearnedWorldModel_AllPropertiesSet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var trainedAt = DateTime.UtcNow;

        // Act
        var model = new LearnedWorldModel(
            id, "EnvModel-v1", ModelArchitecture.Transformer, 0.92, 10000, trainedAt);

        // Assert
        model.Id.Should().Be(id);
        model.Name.Should().Be("EnvModel-v1");
        model.Architecture.Should().Be(ModelArchitecture.Transformer);
        model.Accuracy.Should().Be(0.92);
        model.TrainingSamples.Should().Be(10000);
        model.TrainedAt.Should().Be(trainedAt);
    }

    [Fact]
    public void ModelArchitecture_AllValuesAreDefined()
    {
        // Act
        var values = Enum.GetValues<ModelArchitecture>();

        // Assert
        values.Should().Contain(ModelArchitecture.MLP);
        values.Should().Contain(ModelArchitecture.Transformer);
        values.Should().Contain(ModelArchitecture.GNN);
        values.Should().Contain(ModelArchitecture.Hybrid);
    }

    [Fact]
    public void ModelQuality_AllPropertiesSet()
    {
        // Act
        var quality = new ModelQuality(0.95, 0.88, 0.92, 0.03, 500);

        // Assert
        quality.PredictionAccuracy.Should().Be(0.95);
        quality.RewardCorrelation.Should().Be(0.88);
        quality.TerminalAccuracy.Should().Be(0.92);
        quality.CalibrationError.Should().Be(0.03);
        quality.TestSamples.Should().Be(500);
    }

    [Fact]
    public void ModelQuality_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new ModelQuality(0.9, 0.8, 0.85, 0.05, 100);
        var b = new ModelQuality(0.9, 0.8, 0.85, 0.05, 100);

        // Assert
        a.Should().Be(b);
    }
}
