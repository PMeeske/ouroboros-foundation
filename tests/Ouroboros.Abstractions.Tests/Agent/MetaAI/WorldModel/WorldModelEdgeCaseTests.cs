using Ouroboros.Agent.MetaAI.WorldModel;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI.WorldModel;

/// <summary>
/// Additional edge case and equality tests for WorldModel types
/// covering record equality, with-expressions, and boundary conditions.
/// </summary>
[Trait("Category", "Unit")]
public class WorldModelEdgeCaseTests
{
    [Fact]
    public void AgentAction_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var parameters = new Dictionary<string, object> { ["key"] = "value" };
        var a = new AgentAction("move", parameters);
        var b = new AgentAction("move", parameters);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void AgentAction_RecordEquality_DifferentNames_AreNotEqual()
    {
        // Arrange
        var a = new AgentAction("move");
        var b = new AgentAction("jump");

        // Assert
        a.Should().NotBe(b);
    }

    [Fact]
    public void AgentAction_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new AgentAction("move", new Dictionary<string, object> { ["dir"] = "north" });

        // Act
        var modified = original with { Name = "jump" };

        // Assert
        modified.Name.Should().Be("jump");
        modified.Parameters.Should().BeSameAs(original.Parameters);
    }

    [Fact]
    public void AgentAction_NullParameters_DefaultIsNull()
    {
        // Act
        var action = new AgentAction("noop");

        // Assert
        action.Parameters.Should().BeNull();
    }

    [Fact]
    public void WorldState_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var features = new Dictionary<string, object>();
        var ts = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new WorldState(id, features, ts);
        var b = new WorldState(id, features, ts);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void WorldState_RecordEquality_DifferentIds_AreNotEqual()
    {
        // Arrange
        var features = new Dictionary<string, object>();
        var ts = DateTime.UtcNow;

        var a = new WorldState(Guid.NewGuid(), features, ts);
        var b = new WorldState(Guid.NewGuid(), features, ts);

        // Assert
        a.Should().NotBe(b);
    }

    [Fact]
    public void WorldState_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new WorldState(
            Guid.NewGuid(),
            new Dictionary<string, object> { ["pos"] = "A1" },
            DateTime.UtcNow);

        // Act
        var newTs = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var modified = original with { Timestamp = newTs };

        // Assert
        modified.Timestamp.Should().Be(newTs);
        modified.Id.Should().Be(original.Id);
        modified.Features.Should().BeSameAs(original.Features);
    }

    [Fact]
    public void WorldTransition_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var from = new WorldState(Guid.NewGuid(), new Dictionary<string, object>(), DateTime.UtcNow);
        var to = new WorldState(Guid.NewGuid(), new Dictionary<string, object>(), DateTime.UtcNow);
        var action = new AgentAction("move");

        var a = new WorldTransition(from, action, to, 1.0);
        var b = new WorldTransition(from, action, to, 1.0);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void WorldTransition_WithExpression_ChangesReward()
    {
        // Arrange
        var from = new WorldState(Guid.NewGuid(), new Dictionary<string, object>(), DateTime.UtcNow);
        var to = new WorldState(Guid.NewGuid(), new Dictionary<string, object>(), DateTime.UtcNow);
        var action = new AgentAction("move");
        var original = new WorldTransition(from, action, to, 1.0);

        // Act
        var modified = original with { Reward = -1.0 };

        // Assert
        modified.Reward.Should().Be(-1.0);
        modified.FromState.Should().Be(from);
        modified.ToState.Should().Be(to);
        modified.Action.Should().Be(action);
    }

    [Fact]
    public void WorldTransition_NegativeReward_IsAllowed()
    {
        // Arrange
        var from = new WorldState(Guid.NewGuid(), new Dictionary<string, object>(), DateTime.UtcNow);
        var to = new WorldState(Guid.NewGuid(), new Dictionary<string, object>(), DateTime.UtcNow);

        // Act
        var transition = new WorldTransition(from, new AgentAction("penalty"), to, -100.0);

        // Assert
        transition.Reward.Should().Be(-100.0);
    }

    [Fact]
    public void ActionPlan_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var actions = new List<AgentAction> { new AgentAction("move") };

        var a = new ActionPlan(actions, 5.0, 0.9, 3);
        var b = new ActionPlan(actions, 5.0, 0.9, 3);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void ActionPlan_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new ActionPlan(
            new List<AgentAction> { new AgentAction("act1") },
            10.0, 0.7, 5);

        // Act
        var modified = original with { Confidence = 0.99, LookaheadDepth = 10 };

        // Assert
        modified.Confidence.Should().Be(0.99);
        modified.LookaheadDepth.Should().Be(10);
        modified.ExpectedReward.Should().Be(10.0);
        modified.Actions.Should().BeSameAs(original.Actions);
    }

    [Fact]
    public void ActionPlan_EmptyActions_IsAllowed()
    {
        // Act
        var plan = new ActionPlan(new List<AgentAction>(), 0.0, 0.0, 0);

        // Assert
        plan.Actions.Should().BeEmpty();
        plan.ExpectedReward.Should().Be(0.0);
    }

    [Fact]
    public void LearnedWorldModel_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var trainedAt = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new LearnedWorldModel(id, "Model-1", ModelArchitecture.Transformer, 0.95, 5000, trainedAt);
        var b = new LearnedWorldModel(id, "Model-1", ModelArchitecture.Transformer, 0.95, 5000, trainedAt);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void LearnedWorldModel_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new LearnedWorldModel(
            Guid.NewGuid(), "Model-1", ModelArchitecture.MLP, 0.8, 1000, DateTime.UtcNow);

        // Act
        var modified = original with { Accuracy = 0.95, Architecture = ModelArchitecture.Transformer };

        // Assert
        modified.Accuracy.Should().Be(0.95);
        modified.Architecture.Should().Be(ModelArchitecture.Transformer);
        modified.Name.Should().Be("Model-1");
        modified.TrainingSamples.Should().Be(1000);
    }

    [Fact]
    public void ModelQuality_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new ModelQuality(0.9, 0.85, 0.88, 0.05, 100);

        // Act
        var modified = original with { PredictionAccuracy = 0.99, TestSamples = 500 };

        // Assert
        modified.PredictionAccuracy.Should().Be(0.99);
        modified.TestSamples.Should().Be(500);
        modified.RewardCorrelation.Should().Be(0.85);
        modified.CalibrationError.Should().Be(0.05);
    }

    [Fact]
    public void ModelQuality_ZeroValues_AreAllowed()
    {
        // Act
        var quality = new ModelQuality(0.0, 0.0, 0.0, 0.0, 0);

        // Assert
        quality.PredictionAccuracy.Should().Be(0.0);
        quality.TestSamples.Should().Be(0);
    }

    [Fact]
    public void ModelArchitecture_AllValues_CanBeCastToInt()
    {
        // Assert
        ((int)ModelArchitecture.MLP).Should().Be(0);
        ((int)ModelArchitecture.Transformer).Should().Be(1);
        ((int)ModelArchitecture.GNN).Should().Be(2);
        ((int)ModelArchitecture.Hybrid).Should().Be(3);
    }

    [Fact]
    public void ModelArchitecture_EnumCount_IsFour()
    {
        // Act
        var values = Enum.GetValues<ModelArchitecture>();

        // Assert
        values.Should().HaveCount(4);
    }
}
