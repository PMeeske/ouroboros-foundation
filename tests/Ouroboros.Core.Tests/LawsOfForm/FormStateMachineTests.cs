using Ouroboros.Core.LawsOfForm;
using LoF = Ouroboros.Core.LawsOfForm.Form;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class FormStateMachineTests
{
    [Fact]
    public void Constructor_InitialState_IsCertain()
    {
        var sut = new FormStateMachine<string>("Follower");

        sut.IsCertain.Should().BeTrue();
        sut.IsIndeterminate.Should().BeFalse();
        sut.CurrentForm.Should().Be(LoF.Mark);
        sut.CurrentState.HasValue.Should().BeTrue();
        sut.CurrentState.Value.Should().Be("Follower");
        sut.OscillationPhase.Should().Be(0.0);
    }

    [Fact]
    public void Constructor_History_ContainsInitialEntry()
    {
        var sut = new FormStateMachine<string>("Init");

        sut.History.Should().HaveCount(1);
        sut.History[0].Reason.Should().Be("Initial state");
    }

    [Fact]
    public void TransitionTo_NewState_UpdatesState()
    {
        var sut = new FormStateMachine<string>("Follower");

        sut.TransitionTo("Leader", "Won election");

        sut.IsCertain.Should().BeTrue();
        sut.CurrentState.Value.Should().Be("Leader");
        sut.History.Should().HaveCount(2);
    }

    [Fact]
    public void EnterIndeterminateState_ValidPhase_BecomesIndeterminate()
    {
        var sut = new FormStateMachine<string>("Follower");

        sut.EnterIndeterminateState(0.5, "Election in progress");

        sut.IsIndeterminate.Should().BeTrue();
        sut.IsCertain.Should().BeFalse();
        sut.CurrentForm.Should().Be(LoF.Imaginary);
        sut.OscillationPhase.Should().Be(0.5);
        sut.CurrentState.HasValue.Should().BeFalse();
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void EnterIndeterminateState_InvalidPhase_ThrowsArgumentOutOfRange(double phase)
    {
        var sut = new FormStateMachine<string>("Follower");

        var act = () => sut.EnterIndeterminateState(phase, "bad phase");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ResolveState_FromIndeterminate_ReturnsToCertain()
    {
        var sut = new FormStateMachine<string>("Follower");
        sut.EnterIndeterminateState(0.5, "Election");

        sut.ResolveState("Leader", "Election won");

        sut.IsCertain.Should().BeTrue();
        sut.CurrentState.Value.Should().Be("Leader");
    }

    [Fact]
    public void ResolveState_FromCertain_ThrowsInvalidOperation()
    {
        var sut = new FormStateMachine<string>("Follower");

        var act = () => sut.ResolveState("Leader", "Not indeterminate");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpdatePhase_InIndeterminateState_UpdatesPhase()
    {
        var sut = new FormStateMachine<string>("Follower");
        sut.EnterIndeterminateState(0.3, "Election");

        sut.UpdatePhase(0.7);

        sut.OscillationPhase.Should().Be(0.7);
    }

    [Fact]
    public void UpdatePhase_InCertainState_ThrowsInvalidOperation()
    {
        var sut = new FormStateMachine<string>("Follower");

        var act = () => sut.UpdatePhase(0.5);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void UpdatePhase_InvalidPhase_ThrowsArgumentOutOfRange(double phase)
    {
        var sut = new FormStateMachine<string>("Follower");
        sut.EnterIndeterminateState(0.5, "Election");

        var act = () => sut.UpdatePhase(phase);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void WhenCertain_InCertainState_ExecutesAction()
    {
        var sut = new FormStateMachine<string>("Leader");

        var result = sut.WhenCertain(state => state.ToUpper());

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("LEADER");
    }

    [Fact]
    public void WhenCertain_InIndeterminateState_ReturnsNone()
    {
        var sut = new FormStateMachine<string>("Follower");
        sut.EnterIndeterminateState(0.5, "Election");

        var result = sut.WhenCertain(state => state.ToUpper());

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void SampleAt_InIndeterminateState_ReturnsOneOfTwoStates()
    {
        var sut = new FormStateMachine<string>("Follower");
        sut.EnterIndeterminateState(0.5, "Election");

        var result = sut.SampleAt("Leader", "Follower", 0.1);

        result.Should().BeOneOf("Leader", "Follower");
    }

    [Fact]
    public void SampleAt_InCertainState_ThrowsInvalidOperation()
    {
        var sut = new FormStateMachine<string>("Leader");

        var act = () => sut.SampleAt("Leader", "Follower", 0.1);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToString_CertainState_ContainsStateName()
    {
        var sut = new FormStateMachine<string>("Leader");

        sut.ToString().Should().Contain("Leader");
        sut.ToString().Should().Contain("Certain");
    }

    [Fact]
    public void ToString_IndeterminateState_ContainsPhase()
    {
        var sut = new FormStateMachine<string>("Follower");
        sut.EnterIndeterminateState(0.5, "Election");

        sut.ToString().Should().Contain("Indeterminate");
        sut.ToString().Should().Contain("0.50");
    }
}
