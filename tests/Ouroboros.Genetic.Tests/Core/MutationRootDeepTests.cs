// <copyright file="MutationRootDeepTests.cs" company="Ouroboros">
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
/// Deep unit tests for the root Mutation{TGene} class covering
/// MutatePopulation edge cases, mock random, and gene types.
/// </summary>
[Trait("Category", "Unit")]
public class MutationRootDeepTests
{
    [Fact]
    public void MutatePopulation_WithSingleChromosome_Succeeds()
    {
        // Arrange
        var mutation = new Mutation<int>(1.0, gene => gene + 100, seed: 42);
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1, 2, 3 }),
        };
        var population = new Population<int>(chromosomes);

        // Act
        var result = mutation.MutatePopulation(population);

        // Assert
        result.Size.Should().Be(1);
        result.Chromosomes[0].Genes.Should().Equal(101, 102, 103);
    }

    [Fact]
    public void Mutate_WithDoubleGenes_AppliesMutationFunction()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(0.0); // Always mutate
        var mutation = new Mutation<double>(1.0, gene => gene * 2.0, mockRandom.Object);
        var chromosome = new Chromosome<double>(new List<double> { 1.5, 2.5, 3.5 });

        // Act
        var result = mutation.Mutate(chromosome);

        // Assert
        result.Genes.Should().Equal(3.0, 5.0, 7.0);
    }

    [Fact]
    public void Mutate_DoesNotModifyOriginalChromosome()
    {
        // Arrange
        var mutation = new Mutation<int>(1.0, gene => gene + 10, seed: 42);
        var original = new Chromosome<int>(new List<int> { 1, 2, 3 }, fitness: 5.0);

        // Act
        var mutated = mutation.Mutate(original);

        // Assert
        original.Genes.Should().Equal(1, 2, 3);
        original.Fitness.Should().Be(5.0);
        mutated.Genes.Should().Equal(11, 12, 13);
    }

    [Fact]
    public void Mutate_PreservesGeneCount()
    {
        // Arrange
        var mutation = new Mutation<int>(0.5, gene => gene + 1, seed: 42);
        var chromosome = new Chromosome<int>(Enumerable.Range(0, 50).ToList());

        // Act
        var result = mutation.Mutate(chromosome);

        // Assert
        result.Genes.Should().HaveCount(50);
    }

    [Fact]
    public void MutatePopulation_WithMultipleChromosomes_MutatesAll()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(0.0); // Always mutate
        var mutation = new Mutation<int>(1.0, gene => gene * 10, mockRandom.Object);
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
        result.Chromosomes[0].Genes[0].Should().Be(10);
        result.Chromosomes[1].Genes[0].Should().Be(20);
        result.Chromosomes[2].Genes[0].Should().Be(30);
    }

    [Fact]
    public void Constructor_BoundaryRateZero_NoMutation()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(0.5);
        var mutation = new Mutation<int>(0.0, gene => gene + 999, mockRandom.Object);
        var chromosome = new Chromosome<int>(new List<int> { 1, 2, 3 });

        // Act
        var result = mutation.Mutate(chromosome);

        // Assert
        result.Genes.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void Constructor_BoundaryRateOne_AllMutated()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(0.0);
        var mutation = new Mutation<int>(1.0, gene => gene + 1, mockRandom.Object);
        var chromosome = new Chromosome<int>(new List<int> { 10, 20, 30 });

        // Act
        var result = mutation.Mutate(chromosome);

        // Assert
        result.Genes.Should().Equal(11, 21, 31);
    }

    [Fact]
    public void Mutate_WithBoolGenes_TogglesCorrectly()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(0.0); // Always mutate
        var mutation = new Mutation<bool>(1.0, gene => !gene, mockRandom.Object);
        var chromosome = new Chromosome<bool>(new List<bool> { true, false, true });

        // Act
        var result = mutation.Mutate(chromosome);

        // Assert
        result.Genes.Should().Equal(false, true, false);
    }

    [Fact]
    public void MutatePopulation_DoesNotModifyOriginalPopulation()
    {
        // Arrange
        var mutation = new Mutation<int>(1.0, gene => gene + 100, seed: 42);
        var chromosomes = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1, 2 }),
        };
        var original = new Population<int>(chromosomes);

        // Act
        var mutated = mutation.MutatePopulation(original);

        // Assert
        original.Chromosomes[0].Genes.Should().Equal(1, 2);
        mutated.Chromosomes[0].Genes.Should().Equal(101, 102);
    }

    [Fact]
    public void Mutate_SingleGene_WorksCorrectly()
    {
        // Arrange
        var mockRandom = new Mock<IRandomProvider>();
        mockRandom.Setup(r => r.NextDouble()).Returns(0.0);
        var mutation = new Mutation<int>(1.0, gene => gene * 2, mockRandom.Object);
        var chromosome = new Chromosome<int>(new List<int> { 7 });

        // Act
        var result = mutation.Mutate(chromosome);

        // Assert
        result.Genes.Should().Equal(14);
    }

    [Fact]
    public void Mutate_LargeChromosome_Succeeds()
    {
        // Arrange
        var mutation = new Mutation<int>(0.5, gene => gene + 1, seed: 42);
        var genes = Enumerable.Range(0, 1000).ToList();
        var chromosome = new Chromosome<int>(genes);

        // Act
        var result = mutation.Mutate(chromosome);

        // Assert
        result.Genes.Should().HaveCount(1000);
        // Some genes should be mutated, some not
        var mutatedCount = result.Genes.Zip(genes, (m, o) => m != o).Count(d => d);
        mutatedCount.Should().BeGreaterThan(0);
        mutatedCount.Should().BeLessThan(1000);
    }
}
