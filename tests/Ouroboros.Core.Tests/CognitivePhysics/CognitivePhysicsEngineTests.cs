// <copyright file="CognitivePhysicsEngineTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using FluentAssertions;
using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.CognitivePhysics;
using Xunit;

namespace Ouroboros.Tests.CognitivePhysics;

[Trait("Category", "Unit")]
public class CognitivePhysicsEngineTests
{
    private readonly FakeEmbeddingProvider _provider = new();
    private readonly FakeEthicsGate _gate = new();

    private CognitivePhysicsEngine CreateEngine(CognitivePhysicsConfig? config = null) =>
        new(_provider, _gate, config);

    [Fact]
    public async Task ShiftStep_Success_ShouldReturnSuccessResult()
    {
        _provider.SetEmbedding("math", [1.0f, 0.0f]);
        _provider.SetEmbedding("physics", [0.9f, 0.1f]);
        CognitivePhysicsEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("math", 100.0);

        Result<CognitiveState> result = await engine.ShiftStep("physics")(state);

        result.IsSuccess.Should().BeTrue();
        result.Value.Focus.Should().Be("physics");
    }

    [Fact]
    public async Task ShiftStep_Failure_ShouldReturnFailureResult()
    {
        _gate.SetRule("forbidden", EthicsGateResult.Deny("Nope"));
        CognitivePhysicsEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("math");

        Result<CognitiveState> result = await engine.ShiftStep("forbidden")(state);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ChaosStep_ShouldApplyChaos()
    {
        CognitivePhysicsEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("test", 100.0);

        Result<CognitiveState> result = await engine.ChaosStep()(state);

        result.IsSuccess.Should().BeTrue();
        result.Value.Resources.Should().BeLessThan(100.0);
    }

    [Fact]
    public async Task EntangleStep_ShouldCreateBranches()
    {
        CognitivePhysicsEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("origin");

        ImmutableList<CognitiveBranch> branches =
            await engine.EntangleStep(["alpha", "beta"])(state);

        branches.Should().HaveCount(2);
    }

    [Fact]
    public async Task CollapseStep_ShouldReturnBestBranch()
    {
        _provider.SetEmbedding("origin", [1.0f, 0.0f]);
        _provider.SetEmbedding("near", [0.95f, 0.05f]);
        _provider.SetEmbedding("far", [0.0f, 1.0f]);
        CognitivePhysicsEngine engine = CreateEngine();

        CognitiveState state = CognitiveState.Create("origin");
        ImmutableList<CognitiveBranch> branches =
            await engine.EntangleStep(["near", "far"])(state);
        Option<CognitiveState> collapsed =
            await engine.CollapseStep("origin")(branches);

        collapsed.HasValue.Should().BeTrue();
        collapsed.Value.Focus.Should().Be("near");
    }

    [Fact]
    public async Task TickStep_ShouldDecrementCooldown()
    {
        CognitivePhysicsEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("test") with { Cooldown = 3.0 };

        CognitiveState result = await engine.TickStep(2.0)(state);

        result.Cooldown.Should().Be(1.0);
    }

    [Fact]
    public async Task ExecuteTrajectory_AllSuccessful_ShouldTraverseAll()
    {
        _provider.SetEmbedding("a", [1.0f, 0.0f, 0.0f]);
        _provider.SetEmbedding("b", [0.9f, 0.1f, 0.0f]);
        _provider.SetEmbedding("c", [0.8f, 0.2f, 0.0f]);
        CognitivePhysicsEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("a", 100.0);

        Result<CognitiveState> result = await engine.ExecuteTrajectoryAsync(state, ["b", "c"]);

        result.IsSuccess.Should().BeTrue();
        result.Value.Focus.Should().Be("c");
        result.Value.History.Should().Contain("b");
        result.Value.History.Should().Contain("c");
    }

    [Fact]
    public async Task ExecuteTrajectory_FailureMidway_ShouldReturnFailure()
    {
        _provider.SetEmbedding("a", [1.0f, 0.0f]);
        _provider.SetEmbedding("b", [0.9f, 0.1f]);
        _gate.SetRule("blocked", EthicsGateResult.Deny("No"));
        CognitivePhysicsEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("a", 100.0);

        Result<CognitiveState> result = await engine.ExecuteTrajectoryAsync(state, ["b", "blocked"]);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteTrajectory_EmptyTargets_ShouldReturnOriginalState()
    {
        CognitivePhysicsEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("origin", 100.0);

        Result<CognitiveState> result = await engine.ExecuteTrajectoryAsync(state, []);

        result.IsSuccess.Should().BeTrue();
        result.Value.Focus.Should().Be("origin");
    }

    [Fact]
    public async Task AdaptOnSuccessStep_ShouldImproveCompression()
    {
        CognitivePhysicsEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("test") with { Compression = 0.8 };

        CognitiveState result = await engine.AdaptOnSuccessStep(0.8)(state);

        result.Compression.Should().BeLessThan(0.8);
    }

    [Fact]
    public async Task AdaptOnFailureStep_ShouldDegradeCompression()
    {
        CognitivePhysicsEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("test") with { Compression = 0.5 };

        CognitiveState result = await engine.AdaptOnFailureStep()(state);

        result.Compression.Should().BeGreaterThan(0.5);
    }
}
