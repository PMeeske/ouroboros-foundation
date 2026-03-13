// <copyright file="UniformCrossoverGenericTests.cs" company="Ouroboros">
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
/// Tests for the UniformCrossover{TGene} class (root Core API).
/// </summary>
[Trait("Category", "Unit")]
public class UniformCrossoverGenericTests
{
    [Fact]
    public void Constructor_WithDefaultParameters_CreatesInstance()
    {
        // Act
        var crossover = new UniformCrossover<int>();

        // Assert
        crossover.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithValidCrossoverRate_CreatesInstance()
    {
        // Act
        var crossover = new UniformCrossover<int>(crossoverRate: 0.5);

        // Assert
        crossover.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNegativeCrossoverRate_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new UniformCrossover<int>(crossoverRate: -0.1));
    }

    [Fact]
    public void Constructor_WithCrossoverRateAboveOne_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new UniformCrossover<int>(crossoverRate: 1.1));
    }

    [Fact]
    public void Constructor_WithZeroCrossoverRate_IsValid()
    {
        // Act
        var crossover = new UniformCrossover<int>(crossoverRate: 0.0);

        // Assert
        crossover.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithOneCrossoverRate_IsValid()
    {
        // Act
        var crossover = new UniformCrossover<int>(crossoverRate: 1.0);

        // Assert
        crossover.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithSeed_CreatesInstance()
    {
        // Act
        var crossover = new UniformCrossover<int>(0.5, seed: 42);

        // Assert
        crossover.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithRandomProvider_CreatesInstance()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();

        // Act
        var crossover = new UniformCrossover<int>(0.5, mockRandom.Object);

        // Assert
        crossover.Should().NotBeNull();
    }

    [Fact]
    public void Crossover_WithDifferentGeneLengths_ThrowsArgumentException()
    {
        // Arrange
        var crossover = new UniformCrossover<int>(1.0, seed: 42);
        var parent1 = new Chromosome<int>(new List<int> { 1, 2, 3 });
        var parent2 = new Chromosome<int>(new List<int> { 4, 5 });

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            crossover.Crossover(parent1, parent2));
    }

    [Fact]
    public void Crossover_WithZeroRate_ReturnsParentCopies()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(0.5); // Above 0 rate, so no crossover
        var crossover = new UniformCrossover<int>(0.0, mockRandom.Object);
        var parent1 = new Chromosome<int>(new List<int> { 1, 2, 3 });
        var parent2 = new Chromosome<int>(new List<int> { 4, 5, 6 });

        // Act
        var (offspring1, offspring2) = crossover.Crossover(parent1, parent2);

        // Assert
        offspring1.Genes.Should().Equal(1, 2, 3);
        offspring2.Genes.Should().Equal(4, 5, 6);
    }

    [Fact]
    public void Crossover_WithFullRate_ProducesOffspring()
    {
        // Arrange
        int callCount = 0;
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(() =>
        {
            callCount++;
            // First call: crossover decision (0.0 < 1.0, so crossover occurs)
            if (callCount == 1) return 0.0;
            // Subsequent calls: gene selection (alternate between parents)
            return callCount % 2 == 0 ? 0.3 : 0.7;
        });
        var crossover = new UniformCrossover<int>(1.0, mockRandom.Object);
        var parent1 = new Chromosome<int>(new List<int> { 1, 2, 3 });
        var parent2 = new Chromosome<int>(new List<int> { 4, 5, 6 });

        // Act
        var (offspring1, offspring2) = crossover.Crossover(parent1, parent2);

        // Assert
        offspring1.Genes.Should().HaveCount(3);
        offspring2.Genes.Should().HaveCount(3);
        // Each gene should come from one of the parents
        for (int i = 0; i < 3; i++)
        {
            var g1 = offspring1.Genes[i];
            var g2 = offspring2.Genes[i];
            (g1 == parent1.Genes[i] || g1 == parent2.Genes[i]).Should().BeTrue();
            (g2 == parent1.Genes[i] || g2 == parent2.Genes[i]).Should().BeTrue();
        }
    }

    [Fact]
    public void Crossover_GenesAreComplementary()
    {
        // Arrange: when crossover occurs, offspring1 and offspring2 should be complementary
        var crossover = new UniformCrossover<int>(1.0, seed: 42);
        var parent1 = new Chromosome<int>(new List<int> { 1, 2, 3, 4, 5 });
        var parent2 = new Chromosome<int>(new List<int> { 10, 20, 30, 40, 50 });

        // Act
        var (offspring1, offspring2) = crossover.Crossover(parent1, parent2);

        // Assert: for each position, offspring genes come from different parents
        for (int i = 0; i < 5; i++)
        {
            var g1 = offspring1.Genes[i];
            var g2 = offspring2.Genes[i];
            // One from parent1, one from parent2
            if (g1 == parent1.Genes[i])
            {
                g2.Should().Be(parent2.Genes[i]);
            }
            else
            {
                g1.Should().Be(parent2.Genes[i]);
                g2.Should().Be(parent1.Genes[i]);
            }
        }
    }

    [Fact]
    public void Crossover_WithSeed_ProducesReproducibleResults()
    {
        // Arrange
        var parent1 = new Chromosome<int>(new List<int> { 1, 2, 3, 4, 5 });
        var parent2 = new Chromosome<int>(new List<int> { 10, 20, 30, 40, 50 });
        var crossover1 = new UniformCrossover<int>(0.8, seed: 42);
        var crossover2 = new UniformCrossover<int>(0.8, seed: 42);

        // Act
        var result1 = crossover1.Crossover(parent1, parent2);
        var result2 = crossover2.Crossover(parent1, parent2);

        // Assert
        result1.offspring1.Genes.Should().Equal(result2.offspring1.Genes);
        result1.offspring2.Genes.Should().Equal(result2.offspring2.Genes);
    }

    [Fact]
    public void Crossover_WithEmptyGenes_ReturnsEmptyOffspring()
    {
        // Arrange
        var crossover = new UniformCrossover<int>(1.0, seed: 42);
        var parent1 = new Chromosome<int>(new List<int>());
        var parent2 = new Chromosome<int>(new List<int>());

        // Act - crossover rate check will trigger but no genes to swap
        // Actually with 1.0 rate the mockRandom returns value < 1.0 triggering crossover,
        // but there are no genes to iterate, so empty list returned
        var (offspring1, offspring2) = crossover.Crossover(parent1, parent2);

        // Assert
        offspring1.Genes.Should().BeEmpty();
        offspring2.Genes.Should().BeEmpty();
    }

    [Fact]
    public void Crossover_WithSingleGene_Works()
    {
        // Arrange
        var crossover = new UniformCrossover<int>(1.0, seed: 42);
        var parent1 = new Chromosome<int>(new List<int> { 1 });
        var parent2 = new Chromosome<int>(new List<int> { 2 });

        // Act
        var (offspring1, offspring2) = crossover.Crossover(parent1, parent2);

        // Assert
        offspring1.Genes.Should().HaveCount(1);
        offspring2.Genes.Should().HaveCount(1);
        // The gene must come from one parent
        (offspring1.Genes[0] == 1 || offspring1.Genes[0] == 2).Should().BeTrue();
    }

    [Fact]
    public void Crossover_WithStringGenes_Works()
    {
        // Arrange
        var crossover = new UniformCrossover<string>(1.0, seed: 42);
        var parent1 = new Chromosome<string>(new List<string> { "a", "b" });
        var parent2 = new Chromosome<string>(new List<string> { "x", "y" });

        // Act
        var (offspring1, offspring2) = crossover.Crossover(parent1, parent2);

        // Assert
        offspring1.Genes.Should().HaveCount(2);
        offspring2.Genes.Should().HaveCount(2);
        foreach (var gene in offspring1.Genes)
        {
            (gene == "a" || gene == "b" || gene == "x" || gene == "y").Should().BeTrue();
        }
    }
}
