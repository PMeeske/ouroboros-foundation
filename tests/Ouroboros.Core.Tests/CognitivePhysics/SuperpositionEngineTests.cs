// <copyright file="SuperpositionEngineTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Immutable;
using FluentAssertions;
using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.CognitivePhysics;
using Xunit;

namespace Ouroboros.Tests.CognitivePhysics;

[Trait("Category", "Unit")]
public class SuperpositionEngineTests
{
    private readonly FakeEmbeddingProvider _provider = new();
    private readonly FakeEthicsGate _gate = new();

    private SuperpositionEngine CreateEngine() => new(_provider, _gate);

    [Fact]
    public async Task Entangle_EmptyTargets_ShouldReturnEmpty()
    {
        SuperpositionEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("origin");

        ImmutableList<CognitiveBranch> branches = await engine.EntangleAsync(state, []);

        branches.Should().BeEmpty();
    }

    [Fact]
    public async Task Entangle_ShouldCreateBranchPerTarget()
    {
        SuperpositionEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("origin");
        string[] targets = ["alpha", "beta", "gamma"];

        ImmutableList<CognitiveBranch> branches = await engine.EntangleAsync(state, targets);

        branches.Should().HaveCount(3);
        branches.Select(b => b.State.Focus).Should().BeEquivalentTo(targets);
    }

    [Fact]
    public async Task Entangle_ShouldDistributeWeightsEqually()
    {
        SuperpositionEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("origin");

        ImmutableList<CognitiveBranch> branches = await engine.EntangleAsync(state, ["a", "b"]);

        branches.Should().AllSatisfy(b => b.Weight.Should().BeApproximately(0.5, 1e-10));
    }

    [Fact]
    public async Task Entangle_ShouldSetEntanglement()
    {
        SuperpositionEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("origin");
        string[] targets = ["alpha", "beta"];

        ImmutableList<CognitiveBranch> branches = await engine.EntangleAsync(state, targets);

        foreach (CognitiveBranch branch in branches)
        {
            branch.State.Entanglement.Should().Contain("alpha");
            branch.State.Entanglement.Should().Contain("beta");
        }
    }

    [Fact]
    public async Task Collapse_EmptyBranches_ShouldReturnNone()
    {
        SuperpositionEngine engine = CreateEngine();

        Option<CognitiveState> result = await engine.CollapseAsync("origin", []);

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task Collapse_ShouldSelectBestBranch()
    {
        // "close" is near origin, "far" is distant
        _provider.SetEmbedding("origin", [1.0f, 0.0f]);
        _provider.SetEmbedding("close", [0.9f, 0.1f]);
        _provider.SetEmbedding("far", [0.0f, 1.0f]);

        SuperpositionEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("origin");
        ImmutableList<CognitiveBranch> branches = await engine.EntangleAsync(state, ["close", "far"]);

        Option<CognitiveState> result = await engine.CollapseAsync("origin", branches);

        result.HasValue.Should().BeTrue();
        result.Value.Focus.Should().Be("close");
    }

    [Fact]
    public async Task Collapse_ShouldExcludeDeniedBranches()
    {
        _provider.SetEmbedding("origin", [1.0f, 0.0f]);
        _provider.SetEmbedding("denied", [0.99f, 0.01f]); // Very close but denied
        _provider.SetEmbedding("allowed", [0.0f, 1.0f]);  // Far but allowed

        _gate.SetRule("denied", EthicsGateResult.Deny("Forbidden context"));

        SuperpositionEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("origin");
        ImmutableList<CognitiveBranch> branches = await engine.EntangleAsync(state, ["denied", "allowed"]);

        Option<CognitiveState> result = await engine.CollapseAsync("origin", branches);

        result.HasValue.Should().BeTrue();
        result.Value.Focus.Should().Be("allowed");
    }

    [Fact]
    public async Task Collapse_AllDenied_ShouldReturnNone()
    {
        _provider.SetEmbedding("origin", [1.0f, 0.0f]);
        _provider.SetEmbedding("a", [0.9f, 0.1f]);
        _provider.SetEmbedding("b", [0.8f, 0.2f]);

        _gate.SetDefault(EthicsGateResult.Deny("All denied"));

        SuperpositionEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("origin");
        ImmutableList<CognitiveBranch> branches = await engine.EntangleAsync(state, ["a", "b"]);

        Option<CognitiveState> result = await engine.CollapseAsync("origin", branches);

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task Collapse_ShouldClearEntanglement()
    {
        _provider.SetEmbedding("origin", [1.0f, 0.0f]);
        _provider.SetEmbedding("target", [0.9f, 0.1f]);

        SuperpositionEngine engine = CreateEngine();
        CognitiveState state = CognitiveState.Create("origin");
        ImmutableList<CognitiveBranch> branches = await engine.EntangleAsync(state, ["target"]);

        Option<CognitiveState> result = await engine.CollapseAsync("origin", branches);

        result.Value.Entanglement.Should().BeEmpty();
    }
}
