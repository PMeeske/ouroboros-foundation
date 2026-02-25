// <copyright file="CognitiveStateTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.CognitivePhysics;
using Xunit;

namespace Ouroboros.Tests.CognitivePhysics;

[Trait("Category", "Unit")]
public class CognitiveStateTests
{
    [Fact]
    public void Create_ShouldInitializeWithDefaults()
    {
        CognitiveState state = CognitiveState.Create("mathematics", 50.0);

        state.Focus.Should().Be("mathematics");
        state.Resources.Should().Be(50.0);
        state.Compression.Should().Be(1.0);
        state.History.Should().ContainSingle().Which.Should().Be("mathematics");
        state.Cooldown.Should().Be(0.0);
        state.Entanglement.Should().BeEmpty();
    }

    [Fact]
    public void Create_DefaultResources_ShouldBe100()
    {
        CognitiveState state = CognitiveState.Create("test");
        state.Resources.Should().Be(100.0);
    }

    [Fact]
    public void Tick_ShouldDecrementCooldown()
    {
        CognitiveState state = CognitiveState.Create("test") with { Cooldown = 5.0 };

        CognitiveState ticked = state.Tick(2.0);

        ticked.Cooldown.Should().Be(3.0);
    }

    [Fact]
    public void Tick_ShouldFloorCooldownAtZero()
    {
        CognitiveState state = CognitiveState.Create("test") with { Cooldown = 1.0 };

        CognitiveState ticked = state.Tick(5.0);

        ticked.Cooldown.Should().Be(0.0);
    }

    [Fact]
    public void Tick_DefaultElapsed_ShouldBeOne()
    {
        CognitiveState state = CognitiveState.Create("test") with { Cooldown = 3.0 };

        CognitiveState ticked = state.Tick();

        ticked.Cooldown.Should().Be(2.0);
    }

    [Fact]
    public void Record_WithExpression_ShouldProduceNewInstance()
    {
        CognitiveState original = CognitiveState.Create("alpha");
        CognitiveState modified = original with { Focus = "beta" };

        modified.Focus.Should().Be("beta");
        original.Focus.Should().Be("alpha");
    }
}
