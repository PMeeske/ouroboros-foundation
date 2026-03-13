// <copyright file="EthicalHomeostasisEngineTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Core.Ethics;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.Ethics;

/// <summary>
/// Tests for the EthicalHomeostasisEngine which maintains ethical tensions
/// across traditions without premature collapse.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class EthicalHomeostasisEngineTests
{
    private readonly EthicalHomeostasisEngine _engine;

    public EthicalHomeostasisEngineTests()
    {
        var framework = EthicsFrameworkFactory.CreateDefault();
        _engine = new EthicalHomeostasisEngine(framework);
    }

    // --- RegisterTension ---

    [Fact]
    public void RegisterTension_WithValidInput_ReturnsTension()
    {
        // Arrange & Act
        var tension = _engine.RegisterTension(
            "Autonomy vs. safety",
            new[] { "kantian", "ahimsa" },
            0.5);

        // Assert
        tension.Should().NotBeNull();
        tension.Description.Should().Be("Autonomy vs. safety");
        tension.TraditionsInvolved.Should().Contain("kantian");
        tension.TraditionsInvolved.Should().Contain("ahimsa");
        tension.Intensity.Should().Be(0.5);
    }

    [Fact]
    public void RegisterTension_AddsToActiveTensions()
    {
        // Arrange
        _engine.ActiveTensions.Should().BeEmpty();

        // Act
        _engine.RegisterTension("Test tension", new[] { "kantian" }, 0.3);

        // Assert
        _engine.ActiveTensions.Should().HaveCount(1);
    }

    [Fact]
    public void RegisterTension_ClampsIntensityAboveOne()
    {
        // Act
        var tension = _engine.RegisterTension("High", new[] { "kantian" }, 2.0);

        // Assert
        tension.Intensity.Should().Be(1.0);
    }

    [Fact]
    public void RegisterTension_ClampsIntensityBelowZero()
    {
        // Act
        var tension = _engine.RegisterTension("Low", new[] { "kantian" }, -0.5);

        // Assert
        tension.Intensity.Should().Be(0.0);
    }

    [Fact]
    public void RegisterTension_RecordsEventInHistory()
    {
        // Act
        _engine.RegisterTension("History test", new[] { "ubuntu" }, 0.4);

        // Assert
        _engine.EventHistory.Should().HaveCount(1);
        _engine.EventHistory[0].EventType.Should().Be("TensionRegistered");
    }

    // --- TryResolveTension ---

    [Fact]
    public void TryResolveTension_ResolvableTension_ReturnsTrue()
    {
        // Arrange
        var tension = _engine.RegisterTension(
            "Resolvable disagreement", new[] { "kantian", "ubuntu" },
            0.3, isResolvable: true);

        // Act
        var result = _engine.TryResolveTension(tension.Id);

        // Assert
        result.Should().BeTrue();
        _engine.ActiveTensions.Should().BeEmpty();
    }

    [Fact]
    public void TryResolveTension_IrresolvableTension_ReturnsFalse()
    {
        // Arrange
        var tension = _engine.RegisterTension(
            "Fundamental paradox", new[] { "nagarjuna", "levinas" },
            0.8, isResolvable: false);

        // Act
        var result = _engine.TryResolveTension(tension.Id);

        // Assert
        result.Should().BeFalse();
        _engine.ActiveTensions.Should().HaveCount(1);
    }

    [Fact]
    public void TryResolveTension_NonExistentId_ReturnsFalse()
    {
        // Act
        var result = _engine.TryResolveTension("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryResolveTension_RecordsEventInHistory()
    {
        // Arrange
        var tension = _engine.RegisterTension(
            "Resolvable", new[] { "kantian" }, 0.2, isResolvable: true);

        // Act
        _engine.TryResolveTension(tension.Id);

        // Assert
        _engine.EventHistory.Should().HaveCount(2); // Register + Resolve
        _engine.EventHistory[1].EventType.Should().Be("TensionResolved");
    }

    // --- EvaluateCertainty ---

    [Fact]
    public void EvaluateCertainty_NoTensions_ReturnsMark()
    {
        // Act
        var form = _engine.EvaluateCertainty();

        // Assert
        form.IsMark().Should().BeTrue();
    }

    [Fact]
    public void EvaluateCertainty_WithIrresolvableTension_ReturnsImaginary()
    {
        // Arrange
        _engine.RegisterTension(
            "Irresolvable paradox", new[] { "nagarjuna" },
            0.3, isResolvable: false);

        // Act
        var form = _engine.EvaluateCertainty();

        // Assert
        form.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void EvaluateCertainty_WithLowIntensityResolvableTensions_ReturnsMark()
    {
        // Arrange
        _engine.RegisterTension(
            "Minor tension", new[] { "kantian" },
            0.2, isResolvable: true);

        // Act
        var form = _engine.EvaluateCertainty();

        // Assert
        form.IsMark().Should().BeTrue();
    }

    [Fact]
    public void EvaluateCertainty_WithHighIntensityResolvableTensions_ReturnsImaginary()
    {
        // Arrange
        _engine.RegisterTension("High A", new[] { "kantian" }, 0.8, isResolvable: true);

        // Act
        var form = _engine.EvaluateCertainty();

        // Assert
        form.IsImaginary().Should().BeTrue();
    }

    // --- IsPrematureResolution ---

    [Fact]
    public void IsPrematureResolution_IrresolvableTension_ReturnsTrue()
    {
        // Arrange
        var tension = _engine.RegisterTension(
            "Paradox", new[] { "nagarjuna" },
            0.5, isResolvable: false);

        // Act
        var result = _engine.IsPrematureResolution(tension.Id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPrematureResolution_ResolvableTension_ReturnsFalse()
    {
        // Arrange
        var tension = _engine.RegisterTension(
            "Simple disagreement", new[] { "kantian" },
            0.3, isResolvable: true);

        // Act
        var result = _engine.IsPrematureResolution(tension.Id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsPrematureResolution_NonExistentId_ReturnsFalse()
    {
        // Act
        var result = _engine.IsPrematureResolution("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    // --- TakeSnapshot ---

    [Fact]
    public void TakeSnapshot_Empty_ReturnsStableSnapshot()
    {
        // Act
        var snapshot = _engine.TakeSnapshot();

        // Assert
        snapshot.OverallBalance.Should().Be(1.0);
        snapshot.ActiveTensions.Should().BeEmpty();
        snapshot.UnresolvedParadoxCount.Should().Be(0);
        snapshot.IsStable.Should().BeTrue();
    }

    [Fact]
    public void TakeSnapshot_WithTensions_ReflectsState()
    {
        // Arrange
        _engine.RegisterTension("T1", new[] { "kantian" }, 0.6, isResolvable: false);
        _engine.RegisterTension("T2", new[] { "ubuntu" }, 0.4, isResolvable: true);

        // Act
        var snapshot = _engine.TakeSnapshot();

        // Assert
        snapshot.ActiveTensions.Should().HaveCount(2);
        snapshot.UnresolvedParadoxCount.Should().Be(1);
        snapshot.OverallBalance.Should().BeLessThan(1.0);
    }

    // --- TraditionWeights ---

    [Fact]
    public void TraditionWeights_Initialized_AllEqual()
    {
        // Act
        var weights = _engine.TraditionWeights;

        // Assert
        weights.Should().HaveCount(5);
        weights.Values.Should().AllSatisfy(w => w.Should().Be(1.0));
        weights.Should().ContainKey("kantian");
        weights.Should().ContainKey("ubuntu");
        weights.Should().ContainKey("ahimsa");
        weights.Should().ContainKey("nagarjuna");
        weights.Should().ContainKey("levinas");
    }

    // --- Thread safety ---

    [Fact]
    public void ConcurrentRegisterAndResolve_DoesNotThrow()
    {
        // Act & Assert
        var exceptions = new List<Exception>();

        Parallel.For(0, 50, i =>
        {
            try
            {
                var t = _engine.RegisterTension(
                    $"Tension {i}", new[] { "kantian" },
                    0.3, isResolvable: true);
                _engine.TryResolveTension(t.Id);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lock (exceptions) exceptions.Add(ex);
            }
        });

        exceptions.Should().BeEmpty();
    }
}
