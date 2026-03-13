// <copyright file="UniformCrossoverRootDeepTests.cs" company="Ouroboros">
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
/// Deep unit tests for the root UniformCrossover{TGene} class covering
/// fitness preservation, mock random, and edge cases.
/// </summary>
[Trait("Category", "Unit")]
public class UniformCrossoverRootDeepTests
{
    [Fact]
    public void Crossover_PreservesParentFitness_InOffspring()
    {
        // Arrange - crossover preserves parent's fitness via WithGenes
        var crossover = new UniformCrossover<int>(1.0, seed: 42);
        var parent1 = new Chromosome<int>(new List<int> { 1, 2 }, fitness: 10.0);
        var parent2 = new Chromosome<int>(new List<int> { 3, 4 }, fitness: 20.0);

        // Act
        var (offspring1, offspring2) = crossover.Crossover(parent1, parent2);

        // Assert -- WithGenes uses 'with { Genes = ... }' which preserves Fitness
        offspring1.Fitness.Should().Be(10.0);
        offspring2.Fitness.Should().Be(20.0);
    }

    [Fact]
    public void Crossover_NoCrossover_PreservesParentsExactly()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(0.99); // Above crossover rate
        var crossover = new UniformCrossover<int>(0.5, mockRandom.Object);
        var parent1 = new Chromosome<int>(new List<int> { 1, 2, 3 }, fitness: 5.0);
        var parent2 = new Chromosome<int>(new List<int> { 4, 5, 6 }, fitness: 10.0);

        // Act
        var (offspring1, offspring2) = crossover.Crossover(parent1, parent2);

        // Assert
        offspring1.Genes.Should().Equal(1, 2, 3);
        offspring2.Genes.Should().Equal(4, 5, 6);
    }

    [Fact]
    public void Crossover_WithAllGenesFromParent1_ProducesCorrectOffspring()
    {
        // Arrange -- mock always returns < 0.5, so all genes come from parent1 for offspring1
        int callCount = 0;
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(() =>
        {
            callCount++;
            if (callCount == 1) return 0.0; // crossover decision: always crossover
            return 0.1; // gene selection: always < 0.5, so parent1 for offspring1
        });

        var crossover = new UniformCrossover<int>(1.0, mockRandom.Object);
        var parent1 = new Chromosome<int>(new List<int> { 10, 20, 30 });
        var parent2 = new Chromosome<int>(new List<int> { 40, 50, 60 });

        // Act
        var (offspring1, offspring2) = crossover.Crossover(parent1, parent2);

        // Assert
        offspring1.Genes.Should().Equal(10, 20, 30);
        offspring2.Genes.Should().Equal(40, 50, 60);
    }

    [Fact]
    public void Crossover_WithAllGenesFromParent2_ProducesSwappedOffspring()
    {
        // Arrange -- mock always returns >= 0.5, so all genes come from parent2 for offspring1
        int callCount = 0;
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(() =>
        {
            callCount++;
            if (callCount == 1) return 0.0; // crossover decision: always crossover
            return 0.9; // gene selection: always >= 0.5, so parent2 for offspring1
        });

        var crossover = new UniformCrossover<int>(1.0, mockRandom.Object);
        var parent1 = new Chromosome<int>(new List<int> { 10, 20 });
        var parent2 = new Chromosome<int>(new List<int> { 40, 50 });

        // Act
        var (offspring1, offspring2) = crossover.Crossover(parent1, parent2);

        // Assert
        offspring1.Genes.Should().Equal(40, 50);
        offspring2.Genes.Should().Equal(10, 20);
    }

    [Fact]
    public void Crossover_DoubleGenes_WorksCorrectly()
    {
        // Arrange
        var crossover = new UniformCrossover<double>(1.0, seed: 42);
        var parent1 = new Chromosome<double>(new List<double> { 1.1, 2.2, 3.3 });
        var parent2 = new Chromosome<double>(new List<double> { 4.4, 5.5, 6.6 });

        // Act
        var (offspring1, offspring2) = crossover.Crossover(parent1, parent2);

        // Assert
        offspring1.Genes.Should().HaveCount(3);
        offspring2.Genes.Should().HaveCount(3);
        // Each gene should come from one parent
        for (int i = 0; i < 3; i++)
        {
            var g = offspring1.Genes[i];
            (g == parent1.Genes[i] || g == parent2.Genes[i]).Should().BeTrue();
        }
    }

    [Fact]
    public void Crossover_OffspringGeneCountMatchesParent()
    {
        // Arrange
        var crossover = new UniformCrossover<int>(1.0, seed: 42);
        var genes = Enumerable.Range(0, 100).ToList();
        var parent1 = new Chromosome<int>(genes);
        var parent2 = new Chromosome<int>(genes.Select(x => x + 100).ToList());

        // Act
        var (offspring1, offspring2) = crossover.Crossover(parent1, parent2);

        // Assert
        offspring1.Genes.Should().HaveCount(100);
        offspring2.Genes.Should().HaveCount(100);
    }

    [Fact]
    public void Crossover_WithBoundaryRateZero_AlwaysReturnsParents()
    {
        // Arrange
        var crossover = new UniformCrossover<int>(0.0);
        var parent1 = new Chromosome<int>(new List<int> { 1 });
        var parent2 = new Chromosome<int>(new List<int> { 2 });

        // Act -- run multiple times
        for (int i = 0; i < 10; i++)
        {
            var (o1, o2) = crossover.Crossover(parent1, parent2);
            o1.Genes[0].Should().Be(1);
            o2.Genes[0].Should().Be(2);
        }
    }

    [Fact]
    public void Crossover_WithBoundaryRateOne_AlwaysCrosses()
    {
        // Arrange
        var crossover = new UniformCrossover<int>(1.0, seed: 42);
        var parent1 = new Chromosome<int>(new List<int> { 1, 2 });
        var parent2 = new Chromosome<int>(new List<int> { 3, 4 });

        // Act
        var (offspring1, _) = crossover.Crossover(parent1, parent2);

        // Assert -- genes from either parent
        for (int i = 0; i < 2; i++)
        {
            var g1 = offspring1.Genes[i];
            (g1 == parent1.Genes[i] || g1 == parent2.Genes[i]).Should().BeTrue();
        }
    }

    [Fact]
    public void Crossover_WithEmptyGenes_ReturnsEmptyOffspring()
    {
        // Arrange
        var crossover = new UniformCrossover<int>(1.0, seed: 42);
        var parent1 = new Chromosome<int>(new List<int>());
        var parent2 = new Chromosome<int>(new List<int>());

        // Act
        var (offspring1, offspring2) = crossover.Crossover(parent1, parent2);

        // Assert
        offspring1.Genes.Should().BeEmpty();
        offspring2.Genes.Should().BeEmpty();
    }

    [Fact]
    public void Crossover_DifferentLengths_Throws()
    {
        // Arrange
        var crossover = new UniformCrossover<int>(1.0, seed: 42);
        var parent1 = new Chromosome<int>(new List<int> { 1, 2, 3 });
        var parent2 = new Chromosome<int>(new List<int> { 4, 5 });

        // Act & Assert
        Assert.Throws<ArgumentException>(() => crossover.Crossover(parent1, parent2));
    }
}
