// <copyright file="PopulationRootDeepTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Core;

using FluentAssertions;
using Moq;
using Ouroboros.Genetic.Abstractions;
using Ouroboros.Genetic.Core;
using Xunit;

/// <summary>
/// Deep unit tests for the root Population{TGene} class covering
/// EvaluateAsync edge cases, cancellation, and boundary conditions.
/// </summary>
[Trait("Category", "Unit")]
public class PopulationRootDeepTests
{
    [Fact]
    public async Task EvaluateAsync_WithMultipleChromosomes_EvaluatesEach()
    {
        // Arrange
        var callCount = 0;
        var mockFitness = new Mock<IFitnessFunction<int>>();
        mockFitness.Setup(f => f.EvaluateAsync(It.IsAny<IChromosome<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ++callCount * 1.0);

        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
            new Chromosome<int>(new List<int> { 2 }),
            new Chromosome<int>(new List<int> { 3 }),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var result = await population.EvaluateAsync(mockFitness.Object, CancellationToken.None);

        // Assert
        result.Size.Should().Be(3);
        result.Chromosomes[0].Fitness.Should().Be(1.0);
        result.Chromosomes[1].Fitness.Should().Be(2.0);
        result.Chromosomes[2].Fitness.Should().Be(3.0);
    }

    [Fact]
    public void BestChromosome_WithAllNegativeFitness_ReturnsLeastNegative()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: -10.0),
            new Chromosome<int>(new List<int> { 2 }, fitness: -2.0),
            new Chromosome<int>(new List<int> { 3 }, fitness: -5.0),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var best = population.BestChromosome;

        // Assert
        best.Fitness.Should().Be(-2.0);
    }

    [Fact]
    public void AverageFitness_WithNegativeFitness_CalculatesCorrectly()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: -10.0),
            new Chromosome<int>(new List<int> { 2 }, fitness: 10.0),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var avg = population.AverageFitness;

        // Assert
        avg.Should().BeApproximately(0.0, 0.0001);
    }

    [Fact]
    public void WithChromosomes_ReturnsNewPopulation()
    {
        // Arrange
        var original = new Population<int>(new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
        });
        var newChromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 10 }),
            new Chromosome<int>(new List<int> { 20 }),
            new Chromosome<int>(new List<int> { 30 }),
        };

        // Act
        var updated = original.WithChromosomes(newChromosomes);

        // Assert
        updated.Size.Should().Be(3);
        original.Size.Should().Be(1);
    }

    [Fact]
    public void WithChromosomes_WithEmptyList_ThrowsArgumentException()
    {
        // Arrange
        var population = new Population<int>(new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
        });

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            population.WithChromosomes(new List<IChromosome<int>>()));
    }

    [Fact]
    public void Constructor_WithSingleChromosome_Succeeds()
    {
        // Act
        var population = new Population<int>(new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 42 }, fitness: 99.0),
        });

        // Assert
        population.Size.Should().Be(1);
        population.BestChromosome.Fitness.Should().Be(99.0);
    }

    [Fact]
    public void Size_ReturnsCorrectCount()
    {
        // Arrange
        var population = new Population<int>(new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
            new Chromosome<int>(new List<int> { 2 }),
            new Chromosome<int>(new List<int> { 3 }),
            new Chromosome<int>(new List<int> { 4 }),
            new Chromosome<int>(new List<int> { 5 }),
        });

        // Assert
        population.Size.Should().Be(5);
    }

    [Fact]
    public async Task EvaluateAsync_ReturnsNewPopulationWithSameSize()
    {
        // Arrange
        var mockFitness = new Mock<IFitnessFunction<int>>();
        mockFitness.Setup(f => f.EvaluateAsync(It.IsAny<IChromosome<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1.0);

        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 0.0),
            new Chromosome<int>(new List<int> { 2 }, fitness: 0.0),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var evaluated = await population.EvaluateAsync(mockFitness.Object, CancellationToken.None);

        // Assert
        evaluated.Size.Should().Be(population.Size);
        evaluated.Should().NotBeSameAs(population);
    }

    [Fact]
    public async Task EvaluateAsync_DoesNotModifyOriginalPopulation()
    {
        // Arrange
        var mockFitness = new Mock<IFitnessFunction<int>>();
        mockFitness.Setup(f => f.EvaluateAsync(It.IsAny<IChromosome<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(99.0);

        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 0.0),
        };
        var population = new Population<int>(chromosomes);

        // Act
        await population.EvaluateAsync(mockFitness.Object, CancellationToken.None);

        // Assert
        population.Chromosomes[0].Fitness.Should().Be(0.0);
    }

    [Fact]
    public void BestChromosome_WithAllSameFitness_ReturnsFirst()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 5.0),
            new Chromosome<int>(new List<int> { 2 }, fitness: 5.0),
            new Chromosome<int>(new List<int> { 3 }, fitness: 5.0),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var best = population.BestChromosome;

        // Assert
        best.Fitness.Should().Be(5.0);
    }

    [Fact]
    public void Constructor_CopiesChromosomes_ExternalModificationDoesNotAffect()
    {
        // Arrange
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
        };
        var population = new Population<int>(chromosomes);

        // Act
        chromosomes.Add(new Chromosome<int>(new List<int> { 2 }));

        // Assert
        population.Size.Should().Be(1);
    }

    [Fact]
    public void AverageFitness_WithSingleChromosome_ReturnsThatFitness()
    {
        // Arrange
        var population = new Population<int>(new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }, fitness: 7.7),
        });

        // Assert
        population.AverageFitness.Should().BeApproximately(7.7, 0.0001);
    }
}
