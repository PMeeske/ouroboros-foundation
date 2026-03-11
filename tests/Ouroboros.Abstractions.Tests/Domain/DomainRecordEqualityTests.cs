using Ouroboros.Abstractions.Domain;

namespace Ouroboros.Abstractions.Tests.Domain;

/// <summary>
/// Tests record equality, ToString, and structural behavior of Domain record types.
/// These are parameterless sealed records so the main behaviors to verify are
/// structural equality and hash code consistency.
/// </summary>
[Trait("Category", "Unit")]
public class DomainRecordEqualityTests
{
    [Fact]
    public void ActionPrediction_TwoInstances_AreEqual()
    {
        // Arrange
        var a = new ActionPrediction();
        var b = new ActionPrediction();

        // Assert
        a.Should().Be(b);
        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
    }

    [Fact]
    public void ActionPrediction_GetHashCode_IsConsistent()
    {
        // Arrange
        var a = new ActionPrediction();
        var b = new ActionPrediction();

        // Assert
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void ActionPrediction_ToString_IsNotNull()
    {
        // Arrange & Act
        var str = new ActionPrediction().ToString();

        // Assert
        str.Should().NotBeNull();
        str.Should().Contain("ActionPrediction");
    }

    [Fact]
    public void AgentModel_TwoInstances_AreEqual()
    {
        // Arrange
        var a = new AgentModel();
        var b = new AgentModel();

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void AgentObservation_TwoInstances_AreEqual()
    {
        // Arrange
        var a = new AgentObservation();
        var b = new AgentObservation();

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void BeliefState_TwoInstances_AreEqual()
    {
        // Arrange
        var a = new BeliefState();
        var b = new BeliefState();

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void EmbodiedAction_TwoInstances_AreEqual()
    {
        // Arrange
        var a = new EmbodiedAction();
        var b = new EmbodiedAction();

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void EmbodiedTransition_TwoInstances_AreEqual()
    {
        // Arrange
        var a = new EmbodiedTransition();
        var b = new EmbodiedTransition();

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void IntentionPrediction_TwoInstances_AreEqual()
    {
        // Arrange
        var a = new IntentionPrediction();
        var b = new IntentionPrediction();

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void PredictedState_TwoInstances_AreEqual()
    {
        // Arrange
        var a = new PredictedState();
        var b = new PredictedState();

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void SensorState_TwoInstances_AreEqual()
    {
        // Arrange
        var a = new SensorState();
        var b = new SensorState();

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void SymbolicRule_TwoInstances_AreEqual()
    {
        // Arrange
        var a = new SymbolicRule();
        var b = new SymbolicRule();

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void TemporalEvent_Domain_TwoInstances_AreEqual()
    {
        // Arrange
        var a = new Ouroboros.Abstractions.Domain.TemporalEvent();
        var b = new Ouroboros.Abstractions.Domain.TemporalEvent();

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void AllDomainRecords_AreSealed()
    {
        // Assert
        typeof(ActionPrediction).IsSealed.Should().BeTrue();
        typeof(AgentModel).IsSealed.Should().BeTrue();
        typeof(AgentObservation).IsSealed.Should().BeTrue();
        typeof(BeliefState).IsSealed.Should().BeTrue();
        typeof(EmbodiedAction).IsSealed.Should().BeTrue();
        typeof(EmbodiedTransition).IsSealed.Should().BeTrue();
        typeof(IntentionPrediction).IsSealed.Should().BeTrue();
        typeof(PredictedState).IsSealed.Should().BeTrue();
        typeof(SensorState).IsSealed.Should().BeTrue();
        typeof(SymbolicRule).IsSealed.Should().BeTrue();
        typeof(Ouroboros.Abstractions.Domain.TemporalEvent).IsSealed.Should().BeTrue();
    }

    [Fact]
    public void AllDomainRecords_Equals_WithNull_ReturnsFalse()
    {
        // Arrange & Assert
        new ActionPrediction().Equals(null).Should().BeFalse();
        new AgentModel().Equals(null).Should().BeFalse();
        new AgentObservation().Equals(null).Should().BeFalse();
        new BeliefState().Equals(null).Should().BeFalse();
        new EmbodiedAction().Equals(null).Should().BeFalse();
        new EmbodiedTransition().Equals(null).Should().BeFalse();
        new IntentionPrediction().Equals(null).Should().BeFalse();
        new PredictedState().Equals(null).Should().BeFalse();
        new SensorState().Equals(null).Should().BeFalse();
        new SymbolicRule().Equals(null).Should().BeFalse();
        new Ouroboros.Abstractions.Domain.TemporalEvent().Equals(null).Should().BeFalse();
    }
}
