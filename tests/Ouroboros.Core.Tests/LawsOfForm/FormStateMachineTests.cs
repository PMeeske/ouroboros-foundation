// <copyright file="FormStateMachineTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for <see cref="FormStateMachine{TState}"/> which models states
/// with three-valued logic including indeterminate states.
/// </summary>
[Trait("Category", "Unit")]
public class FormStateMachineTests
{
    private enum ServerRole { Follower, Candidate, Leader }

    // ──────────── Constructor / Initial State ────────────

    [Fact]
    public void Constructor_SetsInitialCertainState()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);

        machine.IsCertain.Should().BeTrue();
        machine.IsIndeterminate.Should().BeFalse();
        machine.CurrentForm.Should().Be(Form.Mark);
        machine.CurrentState.HasValue.Should().BeTrue();
        machine.CurrentState.Value.Should().Be(ServerRole.Follower);
        machine.OscillationPhase.Should().Be(0.0);
    }

    [Fact]
    public void Constructor_RecordsInitialTransitionInHistory()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);

        machine.History.Should().HaveCount(1);
        machine.History[0].Form.Should().Be(Form.Mark);
        machine.History[0].Reason.Should().Be("Initial state");
    }

    // ──────────── TransitionTo ────────────

    [Fact]
    public void TransitionTo_UpdatesCurrentState()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);

        machine.TransitionTo(ServerRole.Leader, "Won election");

        machine.IsCertain.Should().BeTrue();
        machine.CurrentState.Value.Should().Be(ServerRole.Leader);
        machine.CurrentForm.Should().Be(Form.Mark);
    }

    [Fact]
    public void TransitionTo_ResetsOscillationPhase()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);
        machine.EnterIndeterminateState(0.7, "Election");
        machine.TransitionTo(ServerRole.Leader, "Won");

        machine.OscillationPhase.Should().Be(0.0);
    }

    [Fact]
    public void TransitionTo_AddsToHistory()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);
        machine.TransitionTo(ServerRole.Leader, "Won election");

        machine.History.Should().HaveCount(2);
        machine.History[1].Reason.Should().Be("Won election");
    }

    // ──────────── EnterIndeterminateState ────────────

    [Fact]
    public void EnterIndeterminateState_SetsImaginaryForm()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);

        machine.EnterIndeterminateState(0.5, "Election in progress");

        machine.IsIndeterminate.Should().BeTrue();
        machine.IsCertain.Should().BeFalse();
        machine.CurrentForm.Should().Be(Form.Imaginary);
        machine.CurrentState.HasValue.Should().BeFalse();
        machine.OscillationPhase.Should().Be(0.5);
    }

    [Fact]
    public void EnterIndeterminateState_PhaseBelowZero_ThrowsArgumentOutOfRangeException()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);

        Action act = () => machine.EnterIndeterminateState(-0.1, "Bad phase");

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("phase");
    }

    [Fact]
    public void EnterIndeterminateState_PhaseAboveOne_ThrowsArgumentOutOfRangeException()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);

        Action act = () => machine.EnterIndeterminateState(1.1, "Bad phase");

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("phase");
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void EnterIndeterminateState_BoundaryPhaseValues_AreValid(double phase)
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);

        machine.EnterIndeterminateState(phase, "Boundary test");

        machine.OscillationPhase.Should().Be(phase);
    }

    [Fact]
    public void EnterIndeterminateState_AddsToHistory()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);
        machine.EnterIndeterminateState(0.5, "Election");

        machine.History.Should().HaveCount(2);
        machine.History[1].Form.Should().Be(Form.Imaginary);
        machine.History[1].Reason.Should().Be("Election");
    }

    // ──────────── ResolveState ────────────

    [Fact]
    public void ResolveState_FromIndeterminate_TransitionsToCertain()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);
        machine.EnterIndeterminateState(0.5, "Election");

        machine.ResolveState(ServerRole.Leader, "Election completed");

        machine.IsCertain.Should().BeTrue();
        machine.CurrentState.Value.Should().Be(ServerRole.Leader);
    }

    [Fact]
    public void ResolveState_WhenNotIndeterminate_ThrowsInvalidOperationException()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);

        Action act = () => machine.ResolveState(ServerRole.Leader, "Not in election");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Can only resolve from an indeterminate state");
    }

    [Fact]
    public void ResolveState_RecordsResolvedReasonInHistory()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);
        machine.EnterIndeterminateState(0.5, "Election");
        machine.ResolveState(ServerRole.Leader, "Won majority");

        var lastTransition = machine.History[^1];
        lastTransition.Reason.Should().Contain("Resolved");
        lastTransition.Reason.Should().Contain("Won majority");
    }

    // ──────────── UpdatePhase ────────────

    [Fact]
    public void UpdatePhase_WhenIndeterminate_UpdatesOscillationPhase()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);
        machine.EnterIndeterminateState(0.3, "Election");

        machine.UpdatePhase(0.8);

        machine.OscillationPhase.Should().Be(0.8);
    }

    [Fact]
    public void UpdatePhase_WhenNotIndeterminate_ThrowsInvalidOperationException()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);

        Action act = () => machine.UpdatePhase(0.5);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Can only update phase in indeterminate state");
    }

    [Fact]
    public void UpdatePhase_InvalidPhase_ThrowsArgumentOutOfRangeException()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);
        machine.EnterIndeterminateState(0.5, "Election");

        Action actLow = () => machine.UpdatePhase(-0.1);
        Action actHigh = () => machine.UpdatePhase(1.1);

        actLow.Should().Throw<ArgumentOutOfRangeException>();
        actHigh.Should().Throw<ArgumentOutOfRangeException>();
    }

    // ──────────── WhenCertain ────────────

    [Fact]
    public void WhenCertain_InCertainState_ExecutesAction()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Leader);

        var result = machine.WhenCertain(state => state.ToString());

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("Leader");
    }

    [Fact]
    public void WhenCertain_InIndeterminateState_ReturnsNone()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);
        machine.EnterIndeterminateState(0.5, "Election");

        var result = machine.WhenCertain(state => state.ToString());

        result.HasValue.Should().BeFalse();
    }

    // ──────────── SampleAt ────────────

    [Fact]
    public void SampleAt_WhenIndeterminate_ReturnsOneOfTwoStates()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);
        machine.EnterIndeterminateState(0.5, "Election");

        var sampled = machine.SampleAt(ServerRole.Leader, ServerRole.Follower, 0.0);

        sampled.Should().BeOneOf(ServerRole.Leader, ServerRole.Follower);
    }

    [Fact]
    public void SampleAt_WhenNotIndeterminate_ThrowsInvalidOperationException()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);

        Action act = () => machine.SampleAt(ServerRole.Leader, ServerRole.Follower, 0.0);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Sampling only applies to indeterminate states");
    }

    [Fact]
    public void SampleAt_DifferentTimeSteps_CanProduceDifferentResults()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);
        machine.EnterIndeterminateState(0.0, "Election");

        var results = new HashSet<ServerRole>();
        for (double t = 0.0; t < 2.0; t += 0.1)
        {
            results.Add(machine.SampleAt(ServerRole.Leader, ServerRole.Follower, t));
        }

        // With phase 0.0 and varying time, should oscillate
        results.Should().HaveCountGreaterThan(1);
    }

    // ──────────── ToString ────────────

    [Fact]
    public void ToString_WhenCertain_ContainsStateName()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Leader);

        machine.ToString().Should().Contain("Certain");
        machine.ToString().Should().Contain("Leader");
    }

    [Fact]
    public void ToString_WhenIndeterminate_ContainsPhase()
    {
        var machine = new FormStateMachine<ServerRole>(ServerRole.Follower);
        machine.EnterIndeterminateState(0.75, "Election");

        machine.ToString().Should().Contain("Indeterminate");
        machine.ToString().Should().Contain("0.75");
    }

    // ──────────── String state type ────────────

    [Fact]
    public void WorksWithStringStateType()
    {
        var machine = new FormStateMachine<string>("initial");

        machine.TransitionTo("active", "Activated");
        machine.CurrentState.Value.Should().Be("active");

        machine.EnterIndeterminateState(0.5, "Uncertain");
        machine.IsIndeterminate.Should().BeTrue();

        machine.ResolveState("final", "Resolved");
        machine.CurrentState.Value.Should().Be("final");
    }
}
