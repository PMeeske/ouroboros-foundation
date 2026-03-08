// <copyright file="EvolutionRouletteWheelSelectionDeepTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Core;

using FluentAssertions;
using Ouroboros.Genetic.Core;
using Ouroboros.Tests.Genetic;
using Xunit;

/// <summary>
/// Deep unit tests for EvolutionRouletteWheelSelection{TChromosome} covering
/// statistical bias, constructor variants, and edge cases.
/// </summary>
[Trait("Category", "Unit")]
public class EvolutionRouletteWheelSelectionDeepTests
{
    [Fact]
    public void Constructor_WithDefaultProvider_CreatesInstance()
    {
        // Act
        var selection = new EvolutionRouletteWheelSelection<SimpleChromosome>();

        // Assert
        selection.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithSeed_CreatesInstance()
    {
        // Act
        var selection = new EvolutionRouletteWheelSelection<SimpleChromosome>(seed: 42);

        // Assert
        selection.Should().NotBeNull();
    }

    [Fact]
    public void Select_SingleChromosome_ReturnsThatChromosome()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(42.0, fitness: 1.0),
        });
        var selection = new EvolutionRouletteWheelSelection<SimpleChromosome>(seed: 42);

        // Act
        var result = selection.Select(population);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(42.0);
    }

    [Fact]
    public void Select_AllEqualFitness_SelectsAny()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0, fitness: 5.0),
            new SimpleChromosome(2.0, fitness: 5.0),
            new SimpleChromosome(3.0, fitness: 5.0),
        });
        var selection = new EvolutionRouletteWheelSelection<SimpleChromosome>(seed: 42);

        // Act
        var result = selection.Select(population);

        // Assert
        result.IsSuccess.Should().BeTrue();
        (result.Value.Value == 1.0 || result.Value.Value == 2.0 || result.Value.Value == 3.0)
            .Should().BeTrue();
    }

    [Fact]
    public void Select_MixedPositiveAndNegativeFitness_Succeeds()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0, fitness: -10.0),
            new SimpleChromosome(2.0, fitness: 50.0),
            new SimpleChromosome(3.0, fitness: -5.0),
        });
        var selection = new EvolutionRouletteWheelSelection<SimpleChromosome>(seed: 42);

        // Act
        var result = selection.Select(population);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Select_StronglyBiasedFitness_PrefersHighFitness()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0, fitness: 0.001),
            new SimpleChromosome(2.0, fitness: 1000.0),
            new SimpleChromosome(3.0, fitness: 0.001),
        });
        var selection = new EvolutionRouletteWheelSelection<SimpleChromosome>(seed: 42);

        // Act
        var highCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var result = selection.Select(population);
            if (result.Value.Value == 2.0) highCount++;
        }

        // Assert
        highCount.Should().BeGreaterThan(80);
    }

    [Fact]
    public void SelectMany_ZeroCount_ReturnsEmptyList()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0, fitness: 1.0),
        });
        var selection = new EvolutionRouletteWheelSelection<SimpleChromosome>(seed: 42);

        // Act
        var result = selection.SelectMany(population, 0);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public void SelectMany_CorrectCount_ReturnsExpectedNumber()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0, fitness: 0.5),
            new SimpleChromosome(2.0, fitness: 0.5),
        });
        var selection = new EvolutionRouletteWheelSelection<SimpleChromosome>(seed: 42);

        // Act
        var result = selection.SelectMany(population, 10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(10);
    }

    [Fact]
    public void SelectMany_FromEmptyPopulation_ReturnsFailure()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(Array.Empty<SimpleChromosome>());
        var selection = new EvolutionRouletteWheelSelection<SimpleChromosome>(seed: 42);

        // Act
        var result = selection.SelectMany(population, 5);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Select_WithSeed_IsDeterministic()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0, fitness: 10.0),
            new SimpleChromosome(2.0, fitness: 20.0),
            new SimpleChromosome(3.0, fitness: 30.0),
        });
        var s1 = new EvolutionRouletteWheelSelection<SimpleChromosome>(seed: 42);
        var s2 = new EvolutionRouletteWheelSelection<SimpleChromosome>(seed: 42);

        // Act
        var r1 = s1.Select(population);
        var r2 = s2.Select(population);

        // Assert
        r1.Value.Value.Should().Be(r2.Value.Value);
    }

    [Fact]
    public void Select_AllZeroFitness_StillSelects()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0, fitness: 0.0),
            new SimpleChromosome(2.0, fitness: 0.0),
        });
        var selection = new EvolutionRouletteWheelSelection<SimpleChromosome>(seed: 42);

        // Act
        var result = selection.Select(population);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Select_AllNegativeFitness_StillSelects()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0, fitness: -100.0),
            new SimpleChromosome(2.0, fitness: -50.0),
        });
        var selection = new EvolutionRouletteWheelSelection<SimpleChromosome>(seed: 42);

        // Act
        var result = selection.Select(population);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
