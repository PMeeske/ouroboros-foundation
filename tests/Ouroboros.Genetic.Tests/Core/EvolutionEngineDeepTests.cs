// <copyright file="EvolutionEngineDeepTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Core;

using FluentAssertions;
using Ouroboros.Genetic.Abstractions;
using Ouroboros.Genetic.Core;
using Ouroboros.Tests.Genetic;
using Xunit;

/// <summary>
/// Deep unit tests for EvolutionEngine{TChromosome} covering failure propagation,
/// edge cases, and scenarios not in EvolutionEngineTests.
/// </summary>
[Trait("Category", "Unit")]
public class EvolutionEngineDeepTests
{
    [Fact]
    public void Constructor_WithNullFitnessFunction_ThrowsArgumentNullException()
    {
        // Arrange
        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Success(new SimpleChromosome(15.0));
        Func<SimpleChromosome, Ouroboros.Providers.Random.IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) => Result<SimpleChromosome>.Success(new SimpleChromosome(c.Value + 1));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new EvolutionEngine<SimpleChromosome>(null!, crossoverFunc, mutationFunc));
    }

    [Fact]
    public void Constructor_WithNullCrossoverFunc_ThrowsArgumentNullException()
    {
        // Arrange
        var fitness = new TargetValueFitnessFunction(50.0);
        Func<SimpleChromosome, Ouroboros.Providers.Random.IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) => Result<SimpleChromosome>.Success(new SimpleChromosome(c.Value + 1));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new EvolutionEngine<SimpleChromosome>(fitness, null!, mutationFunc));
    }

    [Fact]
    public void Constructor_WithNullMutationFunc_ThrowsArgumentNullException()
    {
        // Arrange
        var fitness = new TargetValueFitnessFunction(50.0);
        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Success(new SimpleChromosome(15.0));

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new EvolutionEngine<SimpleChromosome>(fitness, crossoverFunc, null!));
    }

    [Fact]
    public void Constructor_ElitismRateExactlyZero_IsValid()
    {
        // Arrange
        var fitness = new TargetValueFitnessFunction(50.0);
        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Success(new SimpleChromosome(15.0));
        Func<SimpleChromosome, Ouroboros.Providers.Random.IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) => Result<SimpleChromosome>.Success(new SimpleChromosome(c.Value));

        // Act
        var engine = new EvolutionEngine<SimpleChromosome>(fitness, crossoverFunc, mutationFunc, elitismRate: 0.0);

        // Assert
        engine.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ElitismRateExactlyOne_IsValid()
    {
        // Arrange
        var fitness = new TargetValueFitnessFunction(50.0);
        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Success(new SimpleChromosome(15.0));
        Func<SimpleChromosome, Ouroboros.Providers.Random.IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) => Result<SimpleChromosome>.Success(new SimpleChromosome(c.Value));

        // Act
        var engine = new EvolutionEngine<SimpleChromosome>(fitness, crossoverFunc, mutationFunc, elitismRate: 1.0);

        // Assert
        engine.Should().NotBeNull();
    }

    [Fact]
    public async Task EvolveAsync_CrossoverReturnsFailure_PropagatesError()
    {
        // Arrange
        var fitness = new TargetValueFitnessFunction(50.0);
        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Failure("Crossover exploded");
        Func<SimpleChromosome, Ouroboros.Providers.Random.IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) => Result<SimpleChromosome>.Success(new SimpleChromosome(c.Value));

        var engine = new EvolutionEngine<SimpleChromosome>(
            fitness, crossoverFunc, mutationFunc,
            crossoverRate: 1.0,
            elitismRate: 0.0,
            seed: 42);

        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(10.0),
            new SimpleChromosome(20.0),
            new SimpleChromosome(30.0),
        });

        // Act
        var result = await engine.EvolveAsync(population, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Crossover failed");
    }

    [Fact]
    public async Task EvolveAsync_MutationReturnsFailure_PropagatesError()
    {
        // Arrange
        var fitness = new TargetValueFitnessFunction(50.0);
        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Success(new SimpleChromosome(p1.Value));
        Func<SimpleChromosome, Ouroboros.Providers.Random.IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) => Result<SimpleChromosome>.Failure("Mutation broke");

        var engine = new EvolutionEngine<SimpleChromosome>(
            fitness, crossoverFunc, mutationFunc,
            mutationRate: 1.0,
            elitismRate: 0.0,
            seed: 42);

        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(10.0),
            new SimpleChromosome(20.0),
        });

        // Act
        var result = await engine.EvolveAsync(population, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Mutation failed");
    }

    [Fact]
    public async Task EvolveAsync_FitnessReturnsFailure_PropagatesError()
    {
        // Arrange
        var fitness = new FailingFitnessFunction();
        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Success(new SimpleChromosome(15.0));
        Func<SimpleChromosome, Ouroboros.Providers.Random.IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) => Result<SimpleChromosome>.Success(new SimpleChromosome(c.Value + 1));

        var engine = new EvolutionEngine<SimpleChromosome>(fitness, crossoverFunc, mutationFunc);
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(10.0),
        });

        // Act
        var result = await engine.EvolveAsync(population, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("fitness evaluation failed");
    }

    [Fact]
    public async Task EvolveAsync_WithSingleChromosome_Succeeds()
    {
        // Arrange
        var fitness = new TargetValueFitnessFunction(50.0);
        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Success(new SimpleChromosome(p1.Value * ratio + p2.Value * (1 - ratio)));
        Func<SimpleChromosome, Ouroboros.Providers.Random.IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) => Result<SimpleChromosome>.Success(new SimpleChromosome(c.Value + (random.NextDouble() - 0.5) * 2));

        var engine = new EvolutionEngine<SimpleChromosome>(
            fitness, crossoverFunc, mutationFunc,
            elitismRate: 0.0,
            seed: 42);

        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(10.0),
        });

        // Act
        var result = await engine.EvolveAsync(population, 3);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Size.Should().Be(1);
    }

    [Fact]
    public void GetBest_WithPopulation_ReturnsBestChromosome()
    {
        // Arrange
        var fitness = new TargetValueFitnessFunction(50.0);
        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Success(new SimpleChromosome(15.0));
        Func<SimpleChromosome, Ouroboros.Providers.Random.IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) => Result<SimpleChromosome>.Success(new SimpleChromosome(c.Value));

        var engine = new EvolutionEngine<SimpleChromosome>(fitness, crossoverFunc, mutationFunc);
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(10.0, fitness: 0.3),
            new SimpleChromosome(50.0, fitness: 0.9),
            new SimpleChromosome(30.0, fitness: 0.5),
        });

        // Act
        var best = engine.GetBest(population);

        // Assert
        best.HasValue.Should().BeTrue();
        best.Value!.Fitness.Should().Be(0.9);
        best.Value.Value.Should().Be(50.0);
    }

    [Fact]
    public void GetBest_WithEmptyPopulation_ReturnsNone()
    {
        // Arrange
        var fitness = new TargetValueFitnessFunction(50.0);
        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Success(new SimpleChromosome(15.0));
        Func<SimpleChromosome, Ouroboros.Providers.Random.IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) => Result<SimpleChromosome>.Success(new SimpleChromosome(c.Value));

        var engine = new EvolutionEngine<SimpleChromosome>(fitness, crossoverFunc, mutationFunc);
        var population = new EvolutionPopulation<SimpleChromosome>(Array.Empty<SimpleChromosome>());

        // Act
        var best = engine.GetBest(population);

        // Assert
        best.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task EvolveAsync_WithSeed_IsReproducible()
    {
        // Arrange
        var fitness = new TargetValueFitnessFunction(50.0);
        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Success(new SimpleChromosome(p1.Value * ratio + p2.Value * (1 - ratio)));
        Func<SimpleChromosome, Ouroboros.Providers.Random.IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) => Result<SimpleChromosome>.Success(new SimpleChromosome(c.Value + (random.NextDouble() - 0.5) * 5));

        var engine1 = new EvolutionEngine<SimpleChromosome>(fitness, crossoverFunc, mutationFunc, seed: 99);
        var engine2 = new EvolutionEngine<SimpleChromosome>(fitness, crossoverFunc, mutationFunc, seed: 99);

        var chromosomes = Enumerable.Range(0, 10).Select(i => new SimpleChromosome(i * 5.0)).ToList();
        var pop1 = new EvolutionPopulation<SimpleChromosome>(chromosomes);
        var pop2 = new EvolutionPopulation<SimpleChromosome>(chromosomes);

        // Act
        var result1 = await engine1.EvolveAsync(pop1, 10);
        var result2 = await engine2.EvolveAsync(pop2, 10);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result1.Value.GetAverageFitness().Should().Be(result2.Value.GetAverageFitness());
    }

    [Fact]
    public async Task EvolveAsync_PreservesPopulationSize()
    {
        // Arrange
        var fitness = new TargetValueFitnessFunction(50.0);
        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Success(new SimpleChromosome(p1.Value * ratio + p2.Value * (1 - ratio)));
        Func<SimpleChromosome, Ouroboros.Providers.Random.IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) => Result<SimpleChromosome>.Success(new SimpleChromosome(c.Value + random.NextDouble()));

        var engine = new EvolutionEngine<SimpleChromosome>(
            fitness, crossoverFunc, mutationFunc,
            elitismRate: 0.2,
            seed: 42);

        var chromosomes = Enumerable.Range(0, 15).Select(i => new SimpleChromosome(i * 3.0)).ToList();
        var population = new EvolutionPopulation<SimpleChromosome>(chromosomes);

        // Act
        var result = await engine.EvolveAsync(population, 5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Size.Should().Be(15);
    }

    private sealed class FailingFitnessFunction : IEvolutionFitnessFunction<SimpleChromosome>
    {
        public Task<Result<double>> EvaluateAsync(SimpleChromosome chromosome)
        {
            return Task.FromResult(Result<double>.Failure("Fitness calculation failed"));
        }
    }
}
