// <copyright file="EvolutionaryAdapterTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.CognitivePhysics;
using Xunit;

namespace Ouroboros.Tests.CognitivePhysics;

[Trait("Category", "Unit")]
public class EvolutionaryAdapterTests
{
    [Fact]
    public void OnSuccess_ShouldDecreaseCompression()
    {
        EvolutionaryAdapter adapter = new(new EvolutionaryConfig(LearningRate: 0.1));
        CognitiveState state = CognitiveState.Create("test") with { Compression = 0.8 };

        CognitiveState result = adapter.OnSuccess(state, 1.0);

        result.Compression.Should().BeApproximately(0.7, 1e-10);
    }

    [Fact]
    public void OnSuccess_ShouldScaleByCoherenceScore()
    {
        EvolutionaryAdapter adapter = new(new EvolutionaryConfig(LearningRate: 0.1));
        CognitiveState state = CognitiveState.Create("test") with { Compression = 0.8 };

        CognitiveState result = adapter.OnSuccess(state, 0.5);

        result.Compression.Should().BeApproximately(0.75, 1e-10);
    }

    [Fact]
    public void OnSuccess_ShouldClampAtMinimum()
    {
        EvolutionaryAdapter adapter = new(new EvolutionaryConfig(LearningRate: 0.5, MinCompression: 0.1));
        CognitiveState state = CognitiveState.Create("test") with { Compression = 0.2 };

        CognitiveState result = adapter.OnSuccess(state, 1.0);

        result.Compression.Should().Be(0.1);
    }

    [Fact]
    public void OnFailure_ShouldIncreaseCompression()
    {
        EvolutionaryAdapter adapter = new(new EvolutionaryConfig(PenaltyFactor: 0.1));
        CognitiveState state = CognitiveState.Create("test") with { Compression = 0.5 };

        CognitiveState result = adapter.OnFailure(state);

        result.Compression.Should().BeApproximately(0.6, 1e-10);
    }

    [Fact]
    public void OnFailure_ShouldClampAtMaximum()
    {
        EvolutionaryAdapter adapter = new(new EvolutionaryConfig(PenaltyFactor: 0.5, MaxCompression: 1.0));
        CognitiveState state = CognitiveState.Create("test") with { Compression = 0.9 };

        CognitiveState result = adapter.OnFailure(state);

        result.Compression.Should().Be(1.0);
    }

    [Fact]
    public void OnSuccess_CoherenceOutOfRange_ShouldClamp()
    {
        EvolutionaryAdapter adapter = new(new EvolutionaryConfig(LearningRate: 0.1));
        CognitiveState state = CognitiveState.Create("test") with { Compression = 0.5 };

        CognitiveState result = adapter.OnSuccess(state, 2.0); // Above 1.0

        // Should clamp coherence to 1.0, so compression = 0.5 - 0.1*1.0 = 0.4
        result.Compression.Should().BeApproximately(0.4, 1e-10);
    }
}
