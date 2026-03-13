// <copyright file="RouletteWheelSelectionRootDeepTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Core;

using FluentAssertions;
using Moq;
using Ouroboros.Genetic.Abstractions;
using Ouroboros.Genetic.Core;
using Ouroboros.Providers.Random;
using Xunit;

/// <summary>
/// Deep unit tests for the root RouletteWheelSelection{TGene} class covering
/// mock random, statistical behavior, and boundary conditions.
/// </summary>
[Trait("Category", "Unit")]
public class RouletteWheelSelectionRootDeepTests
{
    [Fact]
    public void Select_WithMockRandom_SelectsExpectedChromosome()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        // For a population of [10, 20, 30] fitness, total=60
        // If random returns 0.0, spinValue=0.0, first chromosome selected
        mockRandom.Setup(r => r.NextDouble()).Returns(0.0);

        var selection = new RouletteWheelSelection<int>(mockRandom.Object);
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 10),
            new Chromosome<int>(new List<int> { 2 }, fitness: 20),
            new Chromosome<int>(new List<int> { 3 }, fitness: 30),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var selected = selection.Select(population);

        // Assert
        selected.Genes[0].Should().Be(1);
    }

    [Fact]
    public void Select_WithMockRandomHigh_SelectsLastChromosome()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        // Returns 0.99 * totalFitness which should land on last chromosome
        mockRandom.Setup(r => r.NextDouble()).Returns(0.99);

        var selection = new RouletteWheelSelection<int>(mockRandom.Object);
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 10),
            new Chromosome<int>(new List<int> { 2 }, fitness: 20),
            new Chromosome<int>(new List<int> { 3 }, fitness: 30),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var selected = selection.Select(population);

        // Assert
        selected.Genes[0].Should().Be(3);
    }

    [Fact]
    public void Select_WithZeroTotalFitness_SelectsRandomly()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.Next(It.IsAny<int>())).Returns(1);
        mockRandom.Setup(r => r.NextDouble()).Returns(0.5);

        var selection = new RouletteWheelSelection<int>(mockRandom.Object);
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 0.0),
            new Chromosome<int>(new List<int> { 2 }, fitness: 0.0),
            new Chromosome<int>(new List<int> { 3 }, fitness: 0.0),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var selected = selection.Select(population);

        // Assert -- should select index 1 (as mocked)
        selected.Genes[0].Should().Be(2);
    }

    [Fact]
    public void SelectMany_WithSeed_IsDeterministic()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 10),
            new Chromosome<int>(new List<int> { 2 }, fitness: 20),
        };
        var population = new Population<int>(chromosomes);
        var s1 = new RouletteWheelSelection<int>(seed: 42);
        var s2 = new RouletteWheelSelection<int>(seed: 42);

        // Act
        var r1 = s1.SelectMany(population, 5);
        var r2 = s2.SelectMany(population, 5);

        // Assert
        for (int i = 0; i < 5; i++)
        {
            r1[i].Genes.Should().Equal(r2[i].Genes);
        }
    }

    [Fact]
    public void SelectMany_WithLargeCount_Succeeds()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 1.0),
        };
        var population = new Population<int>(chromosomes);
        var selection = new RouletteWheelSelection<int>(seed: 42);

        // Act
        var result = selection.SelectMany(population, 100);

        // Assert
        result.Should().HaveCount(100);
        result.Should().AllSatisfy(c => c.Genes[0].Should().Be(1));
    }

    [Fact]
    public void Select_NegativeFitness_HandlesOffset()
    {
        // Arrange
        var selection = new RouletteWheelSelection<int>(seed: 42);
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: -10.0),
            new Chromosome<int>(new List<int> { 2 }, fitness: -1.0),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var selected = selection.Select(population);

        // Assert -- should not throw
        selected.Should().NotBeNull();
    }

    [Fact]
    public void Select_LargeFitnessDifference_PrefersHighFitness()
    {
        // Arrange
        var selection = new RouletteWheelSelection<int>(seed: 42);
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 0.001),
            new Chromosome<int>(new List<int> { 2 }, fitness: 10000.0),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var highCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var selected = selection.Select(population);
            if (selected.Genes[0] == 2) highCount++;
        }

        // Assert
        highCount.Should().BeGreaterThan(90);
    }

    [Fact]
    public void SelectMany_WithZeroCount_ReturnsEmpty()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 1.0),
        };
        var population = new Population<int>(chromosomes);
        var selection = new RouletteWheelSelection<int>(seed: 42);

        // Act
        var result = selection.SelectMany(population, 0);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Select_SingleChromosome_AlwaysReturnsThat()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 99 }, fitness: 5.0),
        };
        var population = new Population<int>(chromosomes);
        var selection = new RouletteWheelSelection<int>(seed: 42);

        // Act
        var results = Enumerable.Range(0, 10).Select(_ => selection.Select(population)).ToList();

        // Assert
        results.Should().AllSatisfy(c => c.Genes[0].Should().Be(99));
    }

    [Fact]
    public void Select_WithMixedPositiveAndNegative_Succeeds()
    {
        // Arrange
        var selection = new RouletteWheelSelection<int>(seed: 42);
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: -100.0),
            new Chromosome<int>(new List<int> { 2 }, fitness: 200.0),
            new Chromosome<int>(new List<int> { 3 }, fitness: -50.0),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var selected = selection.Select(population);

        // Assert
        selected.Should().NotBeNull();
    }
}
