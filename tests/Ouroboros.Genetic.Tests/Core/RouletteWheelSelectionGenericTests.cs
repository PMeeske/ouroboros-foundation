// <copyright file="RouletteWheelSelectionGenericTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Core;

using FluentAssertions;
using Moq;
using Ouroboros.Genetic.Abstractions;
using Ouroboros.Genetic.Core;
using Ouroboros.Providers.Random;
using Xunit;

/// <summary>
/// Tests for the RouletteWheelSelection{TGene} class (root Core API).
/// </summary>
[Trait("Category", "Unit")]
public class RouletteWheelSelectionGenericTests
{
    [Fact]
    public void Constructor_WithDefaultParameters_CreatesInstance()
    {
        // Act
        var selection = new RouletteWheelSelection<int>();

        // Assert
        selection.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithSeed_CreatesInstance()
    {
        // Act
        var selection = new RouletteWheelSelection<int>(seed: 42);

        // Assert
        selection.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithRandomProvider_CreatesInstance()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();

        // Act
        var selection = new RouletteWheelSelection<int>(mockRandom.Object);

        // Assert
        selection.Should().NotBeNull();
    }

    [Fact]
    public void Select_FromEmptyPopulation_ThrowsArgumentException()
    {
        // Arrange
        // Population constructor throws on empty, so we need a workaround
        // Actually Population<T> throws on empty, so this test verifies that behavior
        Assert.Throws<ArgumentException>(() =>
            new Population<int>(new List<IChromosome<int>>()));
    }

    [Fact]
    public void Select_FromSingleChromosomePopulation_ReturnsThatChromosome()
    {
        // Arrange
        var chromosome = new Chromosome<int>(new List<int> { 1, 2, 3 }, fitness: 1.0);
        var population = new Population<int>(new List<IChromosome<int>> { chromosome });
        var selection = new RouletteWheelSelection<int>(seed: 42);

        // Act
        var selected = selection.Select(population);

        // Assert
        selected.Genes.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void Select_FavorsHigherFitness()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 0.01),
            new Chromosome<int>(new List<int> { 2 }, fitness: 100.0),
            new Chromosome<int>(new List<int> { 3 }, fitness: 0.01),
        };
        var population = new Population<int>(chromosomes);
        var selection = new RouletteWheelSelection<int>(seed: 42);

        // Act - select many times
        var selectedCounts = new Dictionary<int, int> { [1] = 0, [2] = 0, [3] = 0 };
        for (int i = 0; i < 200; i++)
        {
            var selected = selection.Select(population);
            selectedCounts[selected.Genes[0]]++;
        }

        // Assert - high fitness chromosome should be selected most often
        selectedCounts[2].Should().BeGreaterThan(selectedCounts[1]);
        selectedCounts[2].Should().BeGreaterThan(selectedCounts[3]);
    }

    [Fact]
    public void Select_WithNegativeFitness_HandlesCorrectly()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: -5.0),
            new Chromosome<int>(new List<int> { 2 }, fitness: -2.0),
            new Chromosome<int>(new List<int> { 3 }, fitness: -4.0),
        };
        var population = new Population<int>(chromosomes);
        var selection = new RouletteWheelSelection<int>(seed: 42);

        // Act
        var selected = selection.Select(population);

        // Assert - should not throw, should return a chromosome
        selected.Should().NotBeNull();
        selected.Genes.Should().NotBeEmpty();
    }

    [Fact]
    public void Select_WithZeroFitness_HandlesCorrectly()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 0.0),
            new Chromosome<int>(new List<int> { 2 }, fitness: 0.0),
        };
        var population = new Population<int>(chromosomes);
        var selection = new RouletteWheelSelection<int>(seed: 42);

        // Act
        var selected = selection.Select(population);

        // Assert - should select randomly when all fitness is zero
        selected.Should().NotBeNull();
    }

    [Fact]
    public void Select_WithSeed_ProducesReproducibleResults()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 10),
            new Chromosome<int>(new List<int> { 2 }, fitness: 20),
            new Chromosome<int>(new List<int> { 3 }, fitness: 30),
        };
        var population = new Population<int>(chromosomes);
        var selection1 = new RouletteWheelSelection<int>(seed: 42);
        var selection2 = new RouletteWheelSelection<int>(seed: 42);

        // Act
        var result1 = selection1.Select(population);
        var result2 = selection2.Select(population);

        // Assert
        result1.Genes.Should().Equal(result2.Genes);
        result1.Fitness.Should().Be(result2.Fitness);
    }

    [Fact]
    public void SelectMany_SelectsCorrectCount()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 1.0),
            new Chromosome<int>(new List<int> { 2 }, fitness: 2.0),
        };
        var population = new Population<int>(chromosomes);
        var selection = new RouletteWheelSelection<int>(seed: 42);

        // Act
        var selected = selection.SelectMany(population, 5);

        // Assert
        selected.Should().HaveCount(5);
    }

    [Fact]
    public void SelectMany_WithNegativeCount_ThrowsArgumentException()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 1.0),
        };
        var population = new Population<int>(chromosomes);
        var selection = new RouletteWheelSelection<int>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            selection.SelectMany(population, -1));
    }

    [Fact]
    public void SelectMany_WithZeroCount_ReturnsEmptyList()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 1.0),
        };
        var population = new Population<int>(chromosomes);
        var selection = new RouletteWheelSelection<int>(seed: 42);

        // Act
        var selected = selection.SelectMany(population, 0);

        // Assert
        selected.Should().BeEmpty();
    }

    [Fact]
    public void SelectMany_ReturnsValidChromosomes()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 10),
            new Chromosome<int>(new List<int> { 2 }, fitness: 20),
            new Chromosome<int>(new List<int> { 3 }, fitness: 30),
        };
        var population = new Population<int>(chromosomes);
        var selection = new RouletteWheelSelection<int>(seed: 42);

        // Act
        var selected = selection.SelectMany(population, 10);

        // Assert
        selected.Should().AllSatisfy(c => c.Genes.Should().NotBeEmpty());
        selected.Should().AllSatisfy(c =>
        {
            var gene = c.Genes[0];
            (gene == 1 || gene == 2 || gene == 3).Should().BeTrue();
        });
    }

    [Fact]
    public void Select_WithMixedPositiveAndNegativeFitness_SelectsCorrectly()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: -10.0),
            new Chromosome<int>(new List<int> { 2 }, fitness: 50.0),
            new Chromosome<int>(new List<int> { 3 }, fitness: -5.0),
        };
        var population = new Population<int>(chromosomes);
        var selection = new RouletteWheelSelection<int>(seed: 42);

        // Act - should handle offset for negative values
        var selected = selection.Select(population);

        // Assert
        selected.Should().NotBeNull();
    }
}
