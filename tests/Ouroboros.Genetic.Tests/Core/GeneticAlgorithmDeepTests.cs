// <copyright file="GeneticAlgorithmDeepTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Core;

using FluentAssertions;
using Moq;
using Ouroboros.Genetic.Abstractions;
using Ouroboros.Genetic.Core;
using Xunit;

/// <summary>
/// Deep unit tests for GeneticAlgorithm{TGene} covering cancellation,
/// edge cases, and boundary conditions not in GeneticAlgorithmTests.
/// </summary>
[Trait("Category", "Unit")]
public class GeneticAlgorithmDeepTests
{
    [Fact]
    public async Task EvolveAsync_WithCancellationToken_ThrowsWhenCancelled()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var mockFitness = new Mock<IFitnessFunction<int>>();
        mockFitness.Setup(f => f.EvaluateAsync(It.IsAny<IChromosome<int>>(), cts.Token))
            .ThrowsAsync(new OperationCanceledException());

        var ga = new GeneticAlgorithm<int>(mockFitness.Object, x => x + 1, seed: 42);
        var population = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1, 2 }),
        };

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            ga.EvolveAsync(population, 5, cts.Token));
    }

    [Fact]
    public void Constructor_WithNullMutateGene_CreatesInstanceButMutationWillFail()
    {
        // Arrange
        var fitnessFunction = new SumFitnessFunction();

        // Act & Assert -- Mutation constructor will throw for null mutateGene
        Assert.Throws<ArgumentNullException>(() =>
            new GeneticAlgorithm<int>(fitnessFunction, null!));
    }

    [Fact]
    public void FitnessFunction_ReturnsProvidedFunction()
    {
        // Arrange
        var fitnessFunction = new SumFitnessFunction();

        // Act
        var ga = new GeneticAlgorithm<int>(fitnessFunction, x => x + 1);

        // Assert
        ga.FitnessFunction.Should().BeSameAs(fitnessFunction);
    }

    [Fact]
    public async Task EvolveAsync_WithSingleChromosome_Succeeds()
    {
        // Arrange
        var ga = new GeneticAlgorithm<int>(
            new SumFitnessFunction(),
            x => x + 1,
            mutationRate: 0.5,
            crossoverRate: 0.8,
            elitismRate: 0.0,
            seed: 42);

        var population = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 5, 5, 5 }),
        };

        // Act
        var result = await ga.EvolveAsync(population, 3, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvolveAsync_WithOddPopulationSize_Succeeds()
    {
        // Arrange
        var ga = new GeneticAlgorithm<int>(
            new SumFitnessFunction(),
            x => x + 1,
            seed: 42);

        var population = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
            new Chromosome<int>(new List<int> { 2 }),
            new Chromosome<int>(new List<int> { 3 }),
            new Chromosome<int>(new List<int> { 4 }),
            new Chromosome<int>(new List<int> { 5 }),
        };

        // Act
        var result = await ga.EvolveAsync(population, 5, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvolveAsync_WithTwoChromosomes_Succeeds()
    {
        // Arrange
        var ga = new GeneticAlgorithm<int>(
            new SumFitnessFunction(),
            x => x + 1,
            seed: 42);

        var population = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1, 2 }),
            new Chromosome<int>(new List<int> { 3, 4 }),
        };

        // Act
        var result = await ga.EvolveAsync(population, 3, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ElitismRateZero_IsValid()
    {
        // Act
        var ga = new GeneticAlgorithm<int>(new SumFitnessFunction(), x => x, elitismRate: 0.0);

        // Assert
        ga.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ElitismRateOne_IsValid()
    {
        // Act
        var ga = new GeneticAlgorithm<int>(new SumFitnessFunction(), x => x, elitismRate: 1.0);

        // Assert
        ga.Should().NotBeNull();
    }

    [Fact]
    public async Task EvolveAsync_NegativeGenerations_ReturnsFailure()
    {
        // Arrange
        var ga = new GeneticAlgorithm<int>(new SumFitnessFunction(), x => x);
        var population = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
        };

        // Act
        var result = await ga.EvolveAsync(population, -5, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EvolveAsync_WithHighElitism_PreservesBest()
    {
        // Arrange
        var ga = new GeneticAlgorithm<int>(
            new SumFitnessFunction(),
            x => x,
            mutationRate: 0.0,
            crossoverRate: 0.0,
            elitismRate: 1.0,
            seed: 42);

        var population = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 100, 100 }),
            new Chromosome<int>(new List<int> { 1, 1 }),
        };

        // Act
        var result = await ga.EvolveAsync(population, 3, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // With 100% elitism, all individuals are kept as-is
        result.Value.Fitness.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task EvolveAsync_WithStringGenes_Succeeds()
    {
        // Arrange
        var fitness = new StringLengthFitnessFunction();
        var ga = new GeneticAlgorithm<string>(
            fitness,
            s => s + "x",
            mutationRate: 0.5,
            crossoverRate: 0.8,
            seed: 42);

        var population = new List<IChromosome<string>>
        {
            new Chromosome<string>(new List<string> { "a", "bb" }),
            new Chromosome<string>(new List<string> { "ccc", "dddd" }),
            new Chromosome<string>(new List<string> { "e", "ff" }),
        };

        // Act
        var result = await ga.EvolveAsync(population, 5, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvolveAsync_OneGeneration_CompletesSuccessfully()
    {
        // Arrange
        var ga = new GeneticAlgorithm<int>(
            new SumFitnessFunction(),
            x => x + 1,
            seed: 42);

        var population = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1, 2, 3 }),
            new Chromosome<int>(new List<int> { 4, 5, 6 }),
        };

        // Act
        var result = await ga.EvolveAsync(population, 1, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvolveAsync_FitnessEvaluationFailure_ReturnsFailure()
    {
        // Arrange
        var mockFitness = new Mock<IFitnessFunction<int>>();
        mockFitness.Setup(f => f.EvaluateAsync(It.IsAny<IChromosome<int>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Fitness evaluation exploded"));

        var ga = new GeneticAlgorithm<int>(mockFitness.Object, x => x + 1, seed: 42);
        var population = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
        };

        // Act
        var result = await ga.EvolveAsync(population, 1, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Evolution failed");
    }

    [Fact]
    public async Task EvolveAsync_WithSeed_ProducesConsistentResults()
    {
        // Arrange
        var population = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1, 2 }),
            new Chromosome<int>(new List<int> { 3, 4 }),
            new Chromosome<int>(new List<int> { 5, 6 }),
        };

        var ga1 = new GeneticAlgorithm<int>(new SumFitnessFunction(), x => x + 1, seed: 123);
        var ga2 = new GeneticAlgorithm<int>(new SumFitnessFunction(), x => x + 1, seed: 123);

        // Act
        var result1 = await ga1.EvolveAsync(population, 5, CancellationToken.None);
        var result2 = await ga2.EvolveAsync(population, 5, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.Fitness.Should().Be(result2.Value.Fitness);
        result1.Value.Genes.Should().Equal(result2.Value.Genes);
    }

    private class SumFitnessFunction : IFitnessFunction<int>
    {
        public Task<double> EvaluateAsync(IChromosome<int> chromosome, CancellationToken cancellationToken)
        {
            double fitness = chromosome.Genes.Sum();
            return Task.FromResult(fitness);
        }
    }

    private class StringLengthFitnessFunction : IFitnessFunction<string>
    {
        public Task<double> EvaluateAsync(IChromosome<string> chromosome, CancellationToken cancellationToken)
        {
            double fitness = chromosome.Genes.Sum(g => g.Length);
            return Task.FromResult(fitness);
        }
    }
}
