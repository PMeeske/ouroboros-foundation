// <copyright file="ChaosInjectorTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.CognitivePhysics;
using Xunit;

namespace Ouroboros.Tests.CognitivePhysics;

[Trait("Category", "Unit")]
public class ChaosInjectorTests
{
    [Fact]
    public void Inject_SufficientResources_ShouldSucceed()
    {
        ChaosInjector injector = new(new ChaosConfig(ChaosCost: 10.0, CompressionReduction: 0.3));
        CognitiveState state = CognitiveState.Create("test", 50.0);

        Result<CognitiveState> result = injector.Inject(state);

        result.IsSuccess.Should().BeTrue();
        result.Value.Resources.Should().Be(40.0);
        result.Value.Compression.Should().Be(0.7);
    }

    [Fact]
    public void Inject_InsufficientResources_ShouldFail()
    {
        ChaosInjector injector = new(new ChaosConfig(ChaosCost: 20.0));
        CognitiveState state = CognitiveState.Create("test", 5.0);

        Result<CognitiveState> result = injector.Inject(state);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Inject_ShouldIncreaseCooldown()
    {
        ChaosInjector injector = new(new ChaosConfig(InstabilityFactor: 3.0));
        CognitiveState state = CognitiveState.Create("test", 100.0) with { Cooldown = 1.0 };

        Result<CognitiveState> result = injector.Inject(state);

        result.Value.Cooldown.Should().Be(4.0);
    }

    [Fact]
    public void Inject_ShouldClampCompressionAtMinimum()
    {
        ChaosInjector injector = new(new ChaosConfig(CompressionReduction: 0.5));
        CognitiveState state = CognitiveState.Create("test", 100.0) with { Compression = 0.2 };

        Result<CognitiveState> result = injector.Inject(state);

        result.Value.Compression.Should().BeGreaterThanOrEqualTo(0.1);
    }

    [Fact]
    public void DistortDistance_ShouldApplyDistortionFactor()
    {
        ChaosInjector injector = new(new ChaosConfig(DistanceDistortion: 0.5));

        double distorted = injector.DistortDistance(0.8);

        distorted.Should().BeApproximately(0.4, 1e-10);
    }
}
