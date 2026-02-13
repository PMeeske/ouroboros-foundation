// <copyright file="ZeroShiftOperatorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.CognitivePhysics;
using Xunit;

namespace Ouroboros.Tests.CognitivePhysics;

[Trait("Category", "Unit")]
public class ZeroShiftOperatorTests
{
    private readonly FakeEmbeddingProvider _provider = new();
    private readonly FakeEthicsGate _gate = new();

    private ZeroShiftOperator CreateOperator(ZeroShiftConfig? config = null) =>
        new(_provider, _gate, config);

    [Fact]
    public async Task Shift_EmptyTarget_ShouldFail()
    {
        ZeroShiftOperator op = CreateOperator();
        CognitiveState state = CognitiveState.Create("math");

        ZeroShiftResult result = await op.ShiftAsync(state, "");

        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("empty");
    }

    [Fact]
    public async Task Shift_CooldownActive_ShouldFail()
    {
        ZeroShiftOperator op = CreateOperator();
        CognitiveState state = CognitiveState.Create("math") with { Cooldown = 5.0 };

        ZeroShiftResult result = await op.ShiftAsync(state, "physics");

        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("Cooldown");
    }

    [Fact]
    public async Task Shift_EthicsDenied_ShouldFail()
    {
        _gate.SetRule("forbidden", EthicsGateResult.Deny("Not allowed"));
        ZeroShiftOperator op = CreateOperator();
        CognitiveState state = CognitiveState.Create("math");

        ZeroShiftResult result = await op.ShiftAsync(state, "forbidden");

        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("Ethics gate denied");
    }

    [Fact]
    public async Task Shift_EthicsUncertain_ShouldFailWithPenalty()
    {
        _gate.SetRule("risky", EthicsGateResult.Uncertain("Ambiguous domain"));
        ZeroShiftOperator op = CreateOperator(new ZeroShiftConfig(UncertaintyPenalty: 10.0));
        CognitiveState state = CognitiveState.Create("math", 50.0);

        ZeroShiftResult result = await op.ShiftAsync(state, "risky");

        result.Success.Should().BeFalse();
        result.State.Resources.Should().Be(40.0);
        result.FailureReason.Should().Contain("uncertain");
    }

    [Fact]
    public async Task Shift_EthicsUncertain_ShouldClampResourcesAtZero()
    {
        _gate.SetRule("risky", EthicsGateResult.Uncertain("Ambiguous domain"));
        ZeroShiftOperator op = CreateOperator(new ZeroShiftConfig(UncertaintyPenalty: 100.0));
        CognitiveState state = CognitiveState.Create("math", 5.0);

        ZeroShiftResult result = await op.ShiftAsync(state, "risky");

        result.Success.Should().BeFalse();
        result.State.Resources.Should().Be(0.0);
    }

    [Fact]
    public async Task Shift_InsufficientResources_ShouldFail()
    {
        // Set up embeddings that are distant (high cost)
        _provider.SetEmbedding("math", [1.0f, 0.0f]);
        _provider.SetEmbedding("art", [0.0f, 1.0f]);
        ZeroShiftOperator op = CreateOperator();
        CognitiveState state = CognitiveState.Create("math", 0.1);

        ZeroShiftResult result = await op.ShiftAsync(state, "art");

        result.Success.Should().BeFalse();
        result.FailureReason.Should().Contain("Insufficient resources");
    }

    [Fact]
    public async Task Shift_Success_ShouldUpdateState()
    {
        _provider.SetEmbedding("math", [1.0f, 0.0f]);
        _provider.SetEmbedding("physics", [0.8f, 0.6f]);
        ZeroShiftOperator op = CreateOperator(new ZeroShiftConfig(StabilityFactor: 0.5));
        CognitiveState state = CognitiveState.Create("math", 100.0);

        ZeroShiftResult result = await op.ShiftAsync(state, "physics");

        result.Success.Should().BeTrue();
        result.State.Focus.Should().Be("physics");
        result.State.History.Should().HaveCount(2);
        result.State.History.Last().Should().Be("physics");
        result.State.Resources.Should().BeLessThan(100.0);
        result.State.Cooldown.Should().BeGreaterThan(0.0);
        result.Cost.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public async Task Shift_IdenticalContext_ShouldHaveZeroCost()
    {
        _provider.SetEmbedding("math", [1.0f, 0.0f]);
        ZeroShiftOperator op = CreateOperator();
        CognitiveState state = CognitiveState.Create("math", 100.0);

        ZeroShiftResult result = await op.ShiftAsync(state, "math");

        result.Success.Should().BeTrue();
        result.Cost.Should().BeApproximately(0.0, 1e-10);
        result.State.Resources.Should().BeApproximately(100.0, 1e-10);
    }
}
