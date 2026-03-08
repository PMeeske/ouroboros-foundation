// <copyright file="PopulationGenericTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Core;

using FluentAssertions;
using Moq;
using Ouroboros.Genetic.Abstractions;
using Ouroboros.Genetic.Core;
using Xunit;

/// <summary>
/// Tests for the Population{TGene} class (root Core API).
/// </summary>
[Trait("Category", "Unit")]
public class PopulationGenericTests
{
    [Fact]
    public void Constructor_WithValidChromosomes_CreatesPopulation()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1, 2, 3 }),
            new Chromosome<int>(new List<int> { 4, 5, 6 }),
        };

        // Act
        var population = new Population<int>(chromosomes);

        // Assert
        population.Size.Should().Be(2);
        population.Chromosomes.Should().HaveCount(2);
    }

    [Fact]
    public void Constructor_WithNullChromosomes_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Population<int>(null!));
    }

    [Fact]
    public void Constructor_WithEmptyChromosomes_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Population<int>(new List<IChromosome<int>>()));
    }

    [Fact]
    public void Chromosomes_ReturnsReadOnlyList()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var result = population.Chromosomes;

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public void Size_ReturnsCorrectCount()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
            new Chromosome<int>(new List<int> { 2 }),
            new Chromosome<int>(new List<int> { 3 }),
        };
        var population = new Population<int>(chromosomes);

        // Act & Assert
        population.Size.Should().Be(3);
    }

    [Fact]
    public void BestChromosome_ReturnHighestFitness()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 0.2),
            new Chromosome<int>(new List<int> { 2 }, fitness: 0.9),
            new Chromosome<int>(new List<int> { 3 }, fitness: 0.5),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var best = population.BestChromosome;

        // Assert
        best.Fitness.Should().Be(0.9);
        best.Genes.Should().Equal(2);
    }

    [Fact]
    public void BestChromosome_WithSingleChromosome_ReturnsThatChromosome()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 42 }, fitness: 1.0),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var best = population.BestChromosome;

        // Assert
        best.Fitness.Should().Be(1.0);
        best.Genes.Should().Equal(42);
    }

    [Fact]
    public void AverageFitness_ReturnsCorrectAverage()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 1.0),
            new Chromosome<int>(new List<int> { 2 }, fitness: 2.0),
            new Chromosome<int>(new List<int> { 3 }, fitness: 3.0),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var average = population.AverageFitness;

        // Assert
        average.Should().BeApproximately(2.0, 0.0001);
    }

    [Fact]
    public void AverageFitness_WithSingleChromosome_ReturnsThatFitness()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 5.0),
        };
        var population = new Population<int>(chromosomes);

        // Act & Assert
        population.AverageFitness.Should().Be(5.0);
    }

    [Fact]
    public void WithChromosomes_ReturnsNewPopulation()
    {
        // Arrange
        var original = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
        };
        var population = new Population<int>(original);

        var newChromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 10 }),
            new Chromosome<int>(new List<int> { 20 }),
        };

        // Act
        var newPopulation = population.WithChromosomes(newChromosomes);

        // Assert
        newPopulation.Size.Should().Be(2);
        population.Size.Should().Be(1); // Original unchanged
    }

    [Fact]
    public async Task EvaluateAsync_EvaluatesAllChromosomes()
    {
        // Arrange
        var mockFitness = new Mock<IFitnessFunction<int>>();
        mockFitness.Setup(f => f.EvaluateAsync(It.IsAny<IChromosome<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IChromosome<int> c, CancellationToken _) => c.Genes.Sum());

        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1, 2, 3 }),
            new Chromosome<int>(new List<int> { 10, 20 }),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var result = await population.EvaluateAsync(mockFitness.Object, CancellationToken.None);

        // Assert
        result.Size.Should().Be(2);
        result.Chromosomes[0].Fitness.Should().Be(6); // 1+2+3
        result.Chromosomes[1].Fitness.Should().Be(30); // 10+20
    }

    [Fact]
    public async Task EvaluateAsync_WithCancellationToken_PassesTokenToFitness()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var mockFitness = new Mock<IFitnessFunction<int>>();
        mockFitness.Setup(f => f.EvaluateAsync(It.IsAny<IChromosome<int>>(), cts.Token))
            .ReturnsAsync(1.0);

        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
        };
        var population = new Population<int>(chromosomes);

        // Act
        await population.EvaluateAsync(mockFitness.Object, cts.Token);

        // Assert
        mockFitness.Verify(f => f.EvaluateAsync(It.IsAny<IChromosome<int>>(), cts.Token), Times.Once);
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsNewPopulationWithUpdatedFitness()
    {
        // Arrange
        var mockFitness = new Mock<IFitnessFunction<int>>();
        mockFitness.Setup(f => f.EvaluateAsync(It.IsAny<IChromosome<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42.0);

        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 0.0),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var result = await population.EvaluateAsync(mockFitness.Object, CancellationToken.None);

        // Assert
        result.Chromosomes[0].Fitness.Should().Be(42.0);
        population.Chromosomes[0].Fitness.Should().Be(0.0); // Original unchanged
    }

    [Fact]
    public void Constructor_IsImmutableAgainstExternalModification()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
            new Chromosome<int>(new List<int> { 2 }),
        };
        var population = new Population<int>(chromosomes);

        // Act - modify original list
        chromosomes.Add(new Chromosome<int>(new List<int> { 3 }));

        // Assert - population should be unaffected
        population.Size.Should().Be(2);
    }

    [Fact]
    public void BestChromosome_WithEqualFitness_ReturnsFirst()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 5.0),
            new Chromosome<int>(new List<int> { 2 }, fitness: 5.0),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var best = population.BestChromosome;

        // Assert
        best.Fitness.Should().Be(5.0);
    }

    [Fact]
    public void AverageFitness_WithZeroFitness_ReturnsZero()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 0.0),
            new Chromosome<int>(new List<int> { 2 }, fitness: 0.0),
        };
        var population = new Population<int>(chromosomes);

        // Act & Assert
        population.AverageFitness.Should().Be(0.0);
    }

    [Fact]
    public void AverageFitness_WithNegativeFitness_CalculatesCorrectly()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: -2.0),
            new Chromosome<int>(new List<int> { 2 }, fitness: 4.0),
        };
        var population = new Population<int>(chromosomes);

        // Act & Assert
        population.AverageFitness.Should().BeApproximately(1.0, 0.0001);
    }
}
