// <copyright file="EvolutionPipelineExtensionsTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Extensions;

using FluentAssertions;
using Ouroboros.Core.Steps;
using Ouroboros.Genetic.Abstractions;
using Ouroboros.Genetic.Core;
using Ouroboros.Genetic.Extensions;
using Ouroboros.Providers.Random;
using Ouroboros.Tests.Genetic;
using Xunit;

/// <summary>
/// Tests for the EvolutionPipelineExtensions class.
/// Uses manual test doubles instead of Moq because SimpleChromosome is internal.
/// </summary>
[Trait("Category", "Unit")]
public class EvolutionPipelineExtensionsTests
{
    [Fact]
    public async Task Evolve_WithFakeEngine_ReturnsResult()
    {
        // Arrange
        var bestChromosome = new SimpleChromosome(50.0, fitness: 0.9);
        var evolvedPopulation = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            bestChromosome,
        });

        var engine = new FakeEvolutionEngine(
            Result<EvolutionPopulation<SimpleChromosome>>.Success(evolvedPopulation),
            Option<SimpleChromosome>.Some(bestChromosome));

        Step<int, EvolutionPopulation<SimpleChromosome>> inputStep = input =>
        {
            var pop = new EvolutionPopulation<SimpleChromosome>(new[]
            {
                new SimpleChromosome(input * 1.0),
            });
            return Task.FromResult(pop);
        };

        // Act
        var pipeline = inputStep.Evolve(engine, 5);
        var result = await pipeline(10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(50.0);
    }

    [Fact]
    public async Task EvolvePopulation_WithFakeEngine_ReturnsPopulation()
    {
        // Arrange
        var evolvedPopulation = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(10.0, fitness: 0.8),
            new SimpleChromosome(20.0, fitness: 0.9),
        });

        var engine = new FakeEvolutionEngine(
            Result<EvolutionPopulation<SimpleChromosome>>.Success(evolvedPopulation),
            Option<SimpleChromosome>.None());

        Step<int, EvolutionPopulation<SimpleChromosome>> inputStep = input =>
            Task.FromResult(new EvolutionPopulation<SimpleChromosome>(new[]
            {
                new SimpleChromosome(input * 1.0),
            }));

        // Act
        var pipeline = inputStep.EvolvePopulation(engine, 10);
        var result = await pipeline(5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Size.Should().Be(2);
    }

    [Fact]
    public async Task UnwrapOrDefault_WhenSuccess_ReturnsValue()
    {
        // Arrange
        var chromosome = new SimpleChromosome(42.0, fitness: 1.0);
        var defaultChromosome = new SimpleChromosome(-1.0);

        Step<int, Result<SimpleChromosome>> inputStep = input =>
            Task.FromResult(Result<SimpleChromosome>.Success(chromosome));

        // Act
        var pipeline = inputStep.UnwrapOrDefault(defaultChromosome);
        var result = await pipeline(1);

        // Assert
        result.Value.Should().Be(42.0);
    }

    [Fact]
    public async Task UnwrapOrDefault_WhenFailure_ReturnsDefault()
    {
        // Arrange
        var defaultChromosome = new SimpleChromosome(-999.0);

        Step<int, Result<SimpleChromosome>> inputStep = input =>
            Task.FromResult(Result<SimpleChromosome>.Failure("Error"));

        // Act
        var pipeline = inputStep.UnwrapOrDefault(defaultChromosome);
        var result = await pipeline(1);

        // Assert
        result.Value.Should().Be(-999.0);
    }

    [Fact]
    public async Task MatchResult_WhenSuccess_AppliesOnSuccess()
    {
        // Arrange
        var chromosome = new SimpleChromosome(42.0, fitness: 0.95);

        Step<int, Result<SimpleChromosome>> inputStep = input =>
            Task.FromResult(Result<SimpleChromosome>.Success(chromosome));

        // Act
        var pipeline = inputStep.MatchResult(
            c => $"Value: {c.Value}",
            err => $"Error: {err}");
        var result = await pipeline(1);

        // Assert
        result.Should().Be("Value: 42");
    }

    [Fact]
    public async Task MatchResult_WhenFailure_AppliesOnFailure()
    {
        // Arrange
        Step<int, Result<SimpleChromosome>> inputStep = input =>
            Task.FromResult(Result<SimpleChromosome>.Failure("Something went wrong"));

        // Act
        var pipeline = inputStep.MatchResult(
            c => $"Value: {c.Value}",
            err => $"Error: {err}");
        var result = await pipeline(1);

        // Assert
        result.Should().Be("Error: Something went wrong");
    }

    [Fact]
    public async Task EvolveWith_CreatesEngineAndEvolves()
    {
        // Arrange
        var fitnessFunction = new TargetValueFitnessFunction(50.0);

        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) =>
            {
                var newValue = p1.Value * ratio + p2.Value * (1 - ratio);
                return Result<SimpleChromosome>.Success(new SimpleChromosome(newValue));
            };

        Func<SimpleChromosome, IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) =>
            {
                var delta = (random.NextDouble() - 0.5) * 5.0;
                return Result<SimpleChromosome>.Success(new SimpleChromosome(c.Value + delta));
            };

        Step<int, EvolutionPopulation<SimpleChromosome>> createPopulation = size =>
        {
            var chromosomes = Enumerable.Range(0, size)
                .Select(i => new SimpleChromosome(i * 3.0))
                .ToList();
            return Task.FromResult(new EvolutionPopulation<SimpleChromosome>(chromosomes));
        };

        // Act
        var pipeline = createPopulation.EvolveWith(
            fitnessFunction,
            crossoverFunc,
            mutationFunc,
            generations: 10,
            crossoverRate: 0.8,
            mutationRate: 0.1,
            elitismRate: 0.1);

        var result = await pipeline(10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Evolve_WhenEngineReturnsFailure_PropagatesFailure()
    {
        // Arrange
        var engine = new FakeEvolutionEngine(
            Result<EvolutionPopulation<SimpleChromosome>>.Failure("Engine broke"),
            Option<SimpleChromosome>.None());

        Step<int, EvolutionPopulation<SimpleChromosome>> inputStep = input =>
            Task.FromResult(new EvolutionPopulation<SimpleChromosome>(new[]
            {
                new SimpleChromosome(input * 1.0),
            }));

        // Act
        var pipeline = inputStep.Evolve(engine, 5);
        var result = await pipeline(10);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Engine broke");
    }

    /// <summary>
    /// Manual test double for IEvolutionEngine to avoid Moq proxy issues with internal types.
    /// </summary>
    private sealed class FakeEvolutionEngine : IEvolutionEngine<SimpleChromosome>
    {
        private readonly Result<EvolutionPopulation<SimpleChromosome>> evolveResult;
        private readonly Option<SimpleChromosome> bestResult;

        public FakeEvolutionEngine(
            Result<EvolutionPopulation<SimpleChromosome>> evolveResult,
            Option<SimpleChromosome> bestResult)
        {
            this.evolveResult = evolveResult;
            this.bestResult = bestResult;
        }

        public Task<Result<EvolutionPopulation<SimpleChromosome>>> EvolveAsync(
            EvolutionPopulation<SimpleChromosome> initialPopulation, int generations)
        {
            return Task.FromResult(this.evolveResult);
        }

        public Option<SimpleChromosome> GetBest(EvolutionPopulation<SimpleChromosome> population)
        {
            return this.bestResult;
        }
    }
}
