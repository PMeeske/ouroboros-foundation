// <copyright file="AffordanceMapTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.EmbodiedInteraction;
using Xunit;

namespace Ouroboros.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class AffordanceMapTests
{
    [Fact]
    public void NewMap_ShouldBeEmpty()
    {
        // Act
        var map = new AffordanceMap();

        // Assert
        map.Count.Should().Be(0);
        map.All.Should().BeEmpty();
    }

    [Fact]
    public void Add_ShouldIncrementCount()
    {
        // Arrange
        var map = new AffordanceMap();
        var affordance = Affordance.Traversable("floor-1");

        // Act
        map.Add(affordance);

        // Assert
        map.Count.Should().Be(1);
    }

    [Fact]
    public void Add_MultipleForSameObject_ShouldTrackAll()
    {
        // Arrange
        var map = new AffordanceMap();

        // Act
        map.Add(Affordance.Graspable("obj-1"));
        map.Add(Affordance.Activatable("obj-1", "turn"));

        // Assert
        map.Count.Should().Be(2);
    }

    [Fact]
    public void GetForObject_ExistingObject_ShouldReturnAffordances()
    {
        // Arrange
        var map = new AffordanceMap();
        var affordance = Affordance.Traversable("floor-1");
        map.Add(affordance);

        // Act
        var result = map.GetForObject("floor-1");

        // Assert
        result.HasValue.Should().BeTrue();
        result.Value.Should().ContainSingle();
    }

    [Fact]
    public void GetForObject_NonExistentObject_ShouldReturnNone()
    {
        // Arrange
        var map = new AffordanceMap();

        // Act
        var result = map.GetForObject("nonexistent");

        // Assert
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void GetByType_ShouldReturnAffordancesOfType()
    {
        // Arrange
        var map = new AffordanceMap();
        map.Add(Affordance.Traversable("floor-1"));
        map.Add(Affordance.Traversable("floor-2"));
        map.Add(Affordance.Graspable("cup-1"));

        // Act
        var traversables = map.GetByType(AffordanceType.Traversable);

        // Assert
        traversables.Should().HaveCount(2);
    }

    [Fact]
    public void GetByType_NoMatches_ShouldReturnEmpty()
    {
        // Arrange
        var map = new AffordanceMap();
        map.Add(Affordance.Traversable("floor-1"));

        // Act
        var graspables = map.GetByType(AffordanceType.Graspable);

        // Assert
        graspables.Should().BeEmpty();
    }

    [Fact]
    public void Find_ShouldFilterByPredicate()
    {
        // Arrange
        var map = new AffordanceMap();
        map.Add(Affordance.Create(AffordanceType.Traversable, "f1", "walk", confidence: 0.9));
        map.Add(Affordance.Create(AffordanceType.Traversable, "f2", "walk", confidence: 0.3));
        map.Add(Affordance.Create(AffordanceType.Graspable, "o1", "grasp", confidence: 0.8));

        // Act
        var results = map.Find(a => a.Confidence > 0.5).ToList();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public void Find_WithLimit_ShouldCapResults()
    {
        // Arrange
        var map = new AffordanceMap();
        map.Add(Affordance.Create(AffordanceType.Traversable, "f1", "walk", confidence: 0.9));
        map.Add(Affordance.Create(AffordanceType.Traversable, "f2", "walk", confidence: 0.8));
        map.Add(Affordance.Create(AffordanceType.Traversable, "f3", "walk", confidence: 0.7));

        // Act
        var results = map.Find(_ => true, limit: 2).ToList();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public void Find_ShouldOrderByRiskAdjustedConfidenceDescending()
    {
        // Arrange
        var map = new AffordanceMap();
        map.Add(Affordance.Create(AffordanceType.Traversable, "f1", "walk", confidence: 0.5));
        map.Add(Affordance.Create(AffordanceType.Traversable, "f2", "walk", confidence: 0.9));
        map.Add(Affordance.Create(AffordanceType.Traversable, "f3", "walk", confidence: 0.7));

        // Act
        var results = map.Find(_ => true).ToList();

        // Assert
        results[0].Confidence.Should().Be(0.9);
        results[1].Confidence.Should().Be(0.7);
        results[2].Confidence.Should().Be(0.5);
    }

    [Fact]
    public void GetBestForAction_ExistingAction_ShouldReturnHighestConfidence()
    {
        // Arrange
        var map = new AffordanceMap();
        map.Add(Affordance.Create(AffordanceType.Traversable, "f1", "walk", confidence: 0.7));
        map.Add(Affordance.Create(AffordanceType.Traversable, "f2", "walk", confidence: 0.9));

        // Act
        var result = map.GetBestForAction("walk");

        // Assert
        result.HasValue.Should().BeTrue();
        result.Value.Confidence.Should().Be(0.9);
    }

    [Fact]
    public void GetBestForAction_CaseInsensitive_ShouldMatch()
    {
        // Arrange
        var map = new AffordanceMap();
        map.Add(Affordance.Create(AffordanceType.Graspable, "obj", "Grasp", confidence: 0.8));

        // Act
        var result = map.GetBestForAction("grasp");

        // Assert
        result.HasValue.Should().BeTrue();
    }

    [Fact]
    public void GetBestForAction_NonExistentAction_ShouldReturnNone()
    {
        // Arrange
        var map = new AffordanceMap();
        map.Add(Affordance.Traversable("floor-1"));

        // Act
        var result = map.GetBestForAction("fly");

        // Assert
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Clear_ShouldRemoveAllAffordances()
    {
        // Arrange
        var map = new AffordanceMap();
        map.Add(Affordance.Traversable("f1"));
        map.Add(Affordance.Graspable("o1"));

        // Act
        map.Clear();

        // Assert
        map.Count.Should().Be(0);
        map.All.Should().BeEmpty();
    }

    [Fact]
    public void RemoveStale_ShouldRemoveOldAffordances()
    {
        // Arrange
        var map = new AffordanceMap();
        var oldAffordance = new Affordance(
            Guid.NewGuid(),
            AffordanceType.Traversable,
            "floor-1",
            "walk",
            Array.Empty<string>(),
            1.0,
            AffordanceConstraints.None,
            0.0,
            1.0,
            DateTime.UtcNow.AddHours(-2));
        var freshAffordance = Affordance.Traversable("floor-2");

        map.Add(oldAffordance);
        map.Add(freshAffordance);

        // Act
        var removed = map.RemoveStale(TimeSpan.FromHours(1));

        // Assert
        removed.Should().Be(1);
        map.Count.Should().Be(1);
    }

    [Fact]
    public void RemoveStale_NoStaleAffordances_ShouldReturnZero()
    {
        // Arrange
        var map = new AffordanceMap();
        map.Add(Affordance.Traversable("floor-1"));

        // Act
        var removed = map.RemoveStale(TimeSpan.FromHours(1));

        // Assert
        removed.Should().Be(0);
    }

    [Fact]
    public void All_ShouldReturnAllAffordancesAcrossObjects()
    {
        // Arrange
        var map = new AffordanceMap();
        map.Add(Affordance.Traversable("floor-1"));
        map.Add(Affordance.Graspable("cup-1"));
        map.Add(Affordance.Activatable("button-1"));

        // Act
        var all = map.All.ToList();

        // Assert
        all.Should().HaveCount(3);
    }
}
