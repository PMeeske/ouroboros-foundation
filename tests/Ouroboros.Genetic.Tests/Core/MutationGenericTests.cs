// <copyright file="MutationGenericTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Core;

using FluentAssertions;
using Moq;
using Ouroboros.Core.Randomness;
using Ouroboros.Genetic.Abstractions;
using Ouroboros.Genetic.Core;
using Ouroboros.Providers.Random;
using Xunit;

/// <summary>
/// Tests for the Mutation{TGene} class (root Core API).
/// </summary>
[Trait("Category", "Unit")]
public class MutationGenericTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange & Act
        var mutation = new Mutation<int>(0.5, gene => gene + 1);

        // Assert
        mutation.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullMutateGene_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new Mutation<int>(0.1, null!));
    }

    [Fact]
    public void Constructor_WithNegativeMutationRate_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Mutation<int>(-0.1, gene => gene + 1));
    }

    [Fact]
    public void Constructor_WithMutationRateAboveOne_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Mutation<int>(1.1, gene => gene + 1));
    }

    [Fact]
    public void Constructor_WithZeroMutationRate_IsValid()
    {
        // Act
        var mutation = new Mutation<int>(0.0, gene => gene + 1);

        // Assert
        mutation.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithOneMutationRate_IsValid()
    {
        // Act
        var mutation = new Mutation<int>(1.0, gene => gene + 1);

        // Assert
        mutation.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithSeed_CreatesInstance()
    {
        // Act
        var mutation = new Mutation<int>(0.5, gene => gene + 1, seed: 42);

        // Assert
        mutation.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithRandomProvider_CreatesInstance()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();

        // Act
        var mutation = new Mutation<int>(0.5, gene => gene + 1, mockRandom.Object);

        // Assert
        mutation.Should().NotBeNull();
    }

    [Fact]
    public void Mutate_WithFullMutationRate_MutatesAllGenes()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(0.0); // Always below mutation rate
        var mutation = new Mutation<int>(1.0, gene => gene + 10, mockRandom.Object);
        var chromosome = new Chromosome<int>(new List<int> { 1, 2, 3 });

        // Act
        var result = mutation.Mutate(chromosome);

        // Assert
        result.Genes.Should().Equal(11, 12, 13);
    }

    [Fact]
    public void Mutate_WithZeroMutationRate_MutatesNoGenes()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(0.5); // Always above 0
        var mutation = new Mutation<int>(0.0, gene => gene + 10, mockRandom.Object);
        var chromosome = new Chromosome<int>(new List<int> { 1, 2, 3 });

        // Act
        var result = mutation.Mutate(chromosome);

        // Assert
        result.Genes.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void Mutate_WithPartialMutationRate_MutatesSomeGenes()
    {
        // Arrange
        int callCount = 0;
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(() =>
        {
            // Alternate: 0.1 (below 0.5), 0.9 (above 0.5), 0.1 (below 0.5)
            return callCount++ % 2 == 0 ? 0.1 : 0.9;
        });
        var mutation = new Mutation<int>(0.5, gene => gene + 100, mockRandom.Object);
        var chromosome = new Chromosome<int>(new List<int> { 1, 2, 3 });

        // Act
        var result = mutation.Mutate(chromosome);

        // Assert
        result.Genes[0].Should().Be(101); // mutated
        result.Genes[1].Should().Be(2);   // not mutated
        result.Genes[2].Should().Be(103); // mutated
    }

    [Fact]
    public void Mutate_PreservesFitness()
    {
        // Arrange
        var mutation = new Mutation<int>(1.0, gene => gene + 1, seed: 42);
        var chromosome = new Chromosome<int>(new List<int> { 1, 2, 3 }, fitness: 99.0);

        // Act
        var result = mutation.Mutate(chromosome);

        // Assert
        // WithGenes is called, which preserves fitness in the record (Chromosome with { Genes = ... })
        result.Fitness.Should().Be(99.0);
    }

    [Fact]
    public void Mutate_ReturnsNewChromosomeInstance()
    {
        // Arrange
        var mutation = new Mutation<int>(1.0, gene => gene + 1, seed: 42);
        var chromosome = new Chromosome<int>(new List<int> { 1, 2, 3 });

        // Act
        var result = mutation.Mutate(chromosome);

        // Assert
        result.Should().NotBeSameAs(chromosome);
    }

    [Fact]
    public void Mutate_WithSeed_ProducesReproducibleResults()
    {
        // Arrange
        var mutation1 = new Mutation<int>(0.5, gene => gene + 1, seed: 42);
        var mutation2 = new Mutation<int>(0.5, gene => gene + 1, seed: 42);
        var chromosome = new Chromosome<int>(new List<int> { 1, 2, 3, 4, 5 });

        // Act
        var result1 = mutation1.Mutate(chromosome);
        var result2 = mutation2.Mutate(chromosome);

        // Assert
        result1.Genes.Should().Equal(result2.Genes);
    }

    [Fact]
    public void Mutate_WithEmptyGenes_ReturnsChromosomeWithEmptyGenes()
    {
        // Arrange
        var mutation = new Mutation<int>(1.0, gene => gene + 1, seed: 42);
        var chromosome = new Chromosome<int>(new List<int>());

        // Act
        var result = mutation.Mutate(chromosome);

        // Assert
        result.Genes.Should().BeEmpty();
    }

    [Fact]
    public void MutatePopulation_MutatesAllChromosomesInPopulation()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(0.0); // Always mutate
        var mutation = new Mutation<int>(1.0, gene => gene + 100, mockRandom.Object);
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1, 2 }),
            new Chromosome<int>(new List<int> { 3, 4 }),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var result = mutation.MutatePopulation(population);

        // Assert
        result.Size.Should().Be(2);
        result.Chromosomes[0].Genes.Should().Equal(101, 102);
        result.Chromosomes[1].Genes.Should().Equal(103, 104);
    }

    [Fact]
    public void MutatePopulation_ReturnsNewPopulationInstance()
    {
        // Arrange
        var mutation = new Mutation<int>(0.0, gene => gene, seed: 42);
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var result = mutation.MutatePopulation(population);

        // Assert
        result.Should().NotBeSameAs(population);
    }

    [Fact]
    public void MutatePopulation_PreservesPopulationSize()
    {
        // Arrange
        var mutation = new Mutation<int>(0.5, gene => gene + 1, seed: 42);
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
            new Chromosome<int>(new List<int> { 2 }),
            new Chromosome<int>(new List<int> { 3 }),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var result = mutation.MutatePopulation(population);

        // Assert
        result.Size.Should().Be(3);
    }

    [Fact]
    public void Mutate_WithStringGenes_WorksCorrectly()
    {
        // Arrange
        var mutation = new Mutation<string>(1.0, gene => gene.ToUpper(), seed: 42);
        var chromosome = new Chromosome<string>(new List<string> { "hello", "world" });

        // Act
        var result = mutation.Mutate(chromosome);

        // Assert
        result.Genes.Should().Equal("HELLO", "WORLD");
    }
}
