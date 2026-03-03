// <copyright file="GeneticEvolutionStepStaticTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Extensions;

using FluentAssertions;
using Ouroboros.Core.Steps;
using Ouroboros.Genetic.Abstractions;
using Ouroboros.Genetic.Core;
using Ouroboros.Genetic.Extensions;
using Ouroboros.Tests.Genetic;
using Xunit;

/// <summary>
/// Tests for the static GeneticEvolutionStep class in Genetic/Extensions.
/// Uses manual test doubles instead of Moq because SimpleChromosome is internal.
/// </summary>
[Trait("Category", "Unit")]
public class GeneticEvolutionStepStaticTests
{
    [Fact]
    public async Task CreateEvolutionStep_WhenEvolutionSucceeds_ReturnsBestChromosome()
    {
        // Arrange
        var bestChromosome = new SimpleChromosome(42.0, fitness: 0.95);
        var evolvedPopulation = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            bestChromosome,
            new SimpleChromosome(10.0, fitness: 0.5),
        });

        var engine = new FakeEvolutionEngine(
            Result<EvolutionPopulation<SimpleChromosome>>.Success(evolvedPopulation),
            Option<SimpleChromosome>.Some(bestChromosome));

        var step = GeneticEvolutionStep.CreateEvolutionStep(engine, 10);
        var initialPopulation = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0),
            new SimpleChromosome(2.0),
        });

        // Act
        var result = await step(initialPopulation);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(42.0);
        result.Value.Fitness.Should().Be(0.95);
    }

    [Fact]
    public async Task CreateEvolutionStep_WhenEvolutionFails_ReturnsFailure()
    {
        // Arrange
        var engine = new FakeEvolutionEngine(
            Result<EvolutionPopulation<SimpleChromosome>>.Failure("Engine error"),
            Option<SimpleChromosome>.None());

        var step = GeneticEvolutionStep.CreateEvolutionStep(engine, 5);
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0),
        });

        // Act
        var result = await step(population);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Engine error");
    }

    [Fact]
    public async Task CreateEvolutionStep_WhenNullPopulation_ReturnsFailure()
    {
        // Arrange
        var engine = new FakeEvolutionEngine(
            Result<EvolutionPopulation<SimpleChromosome>>.Failure("unused"),
            Option<SimpleChromosome>.None());

        var step = GeneticEvolutionStep.CreateEvolutionStep(engine, 5);

        // Act
        var result = await step(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot be null");
    }

    [Fact]
    public async Task CreateEvolutionStep_WhenNoBestFound_ReturnsFailure()
    {
        // Arrange
        var evolvedPopulation = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0),
        });

        var engine = new FakeEvolutionEngine(
            Result<EvolutionPopulation<SimpleChromosome>>.Success(evolvedPopulation),
            Option<SimpleChromosome>.None());

        var step = GeneticEvolutionStep.CreateEvolutionStep(engine, 5);
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0),
        });

        // Act
        var result = await step(population);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No best chromosome");
    }

    [Fact]
    public async Task CreatePopulationEvolutionStep_WhenSucceeds_ReturnsPopulation()
    {
        // Arrange
        var evolvedPopulation = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(42.0, fitness: 0.9),
            new SimpleChromosome(43.0, fitness: 0.8),
        });

        var engine = new FakeEvolutionEngine(
            Result<EvolutionPopulation<SimpleChromosome>>.Success(evolvedPopulation),
            Option<SimpleChromosome>.None());

        var step = GeneticEvolutionStep.CreatePopulationEvolutionStep(engine, 15);
        var initialPopulation = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0),
            new SimpleChromosome(2.0),
        });

        // Act
        var result = await step(initialPopulation);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Size.Should().Be(2);
    }

    [Fact]
    public async Task CreatePopulationEvolutionStep_WhenNullPopulation_ReturnsFailure()
    {
        // Arrange
        var engine = new FakeEvolutionEngine(
            Result<EvolutionPopulation<SimpleChromosome>>.Failure("unused"),
            Option<SimpleChromosome>.None());

        var step = GeneticEvolutionStep.CreatePopulationEvolutionStep(engine, 5);

        // Act
        var result = await step(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot be null");
    }

    [Fact]
    public async Task CreatePopulationEvolutionStep_WhenFails_PropagatesError()
    {
        // Arrange
        var engine = new FakeEvolutionEngine(
            Result<EvolutionPopulation<SimpleChromosome>>.Failure("Population too small"),
            Option<SimpleChromosome>.None());

        var step = GeneticEvolutionStep.CreatePopulationEvolutionStep(engine, 5);
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0),
        });

        // Act
        var result = await step(population);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Population too small");
    }

    [Fact]
    public async Task CreateEvolveFromInputStep_WhenSucceeds_ReturnsBestChromosome()
    {
        // Arrange
        var bestChromosome = new SimpleChromosome(42.0, fitness: 0.95);
        var evolvedPopulation = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            bestChromosome,
        });

        var engine = new FakeEvolutionEngine(
            Result<EvolutionPopulation<SimpleChromosome>>.Success(evolvedPopulation),
            Option<SimpleChromosome>.Some(bestChromosome));

        Func<string, Result<EvolutionPopulation<SimpleChromosome>>> factory = input =>
        {
            var pop = new EvolutionPopulation<SimpleChromosome>(new[]
            {
                new SimpleChromosome(input.Length),
            });
            return Result<EvolutionPopulation<SimpleChromosome>>.Success(pop);
        };

        var step = GeneticEvolutionStep.CreateEvolveFromInputStep(factory, engine, 10);

        // Act
        var result = await step("test input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(42.0);
    }

    [Fact]
    public async Task CreateEvolveFromInputStep_WhenFactoryFails_ReturnsFailure()
    {
        // Arrange
        var engine = new FakeEvolutionEngine(
            Result<EvolutionPopulation<SimpleChromosome>>.Failure("unused"),
            Option<SimpleChromosome>.None());

        Func<string, Result<EvolutionPopulation<SimpleChromosome>>> factory = input =>
            Result<EvolutionPopulation<SimpleChromosome>>.Failure("Invalid input");

        var step = GeneticEvolutionStep.CreateEvolveFromInputStep(factory, engine, 10);

        // Act
        var result = await step("bad input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Population creation failed");
    }

    [Fact]
    public async Task CreateEvolveFromInputStep_WhenEvolutionFails_ReturnsFailure()
    {
        // Arrange
        var engine = new FakeEvolutionEngine(
            Result<EvolutionPopulation<SimpleChromosome>>.Failure("Evolution error"),
            Option<SimpleChromosome>.None());

        Func<string, Result<EvolutionPopulation<SimpleChromosome>>> factory = input =>
            Result<EvolutionPopulation<SimpleChromosome>>.Success(
                new EvolutionPopulation<SimpleChromosome>(new[] { new SimpleChromosome(1.0) }));

        var step = GeneticEvolutionStep.CreateEvolveFromInputStep(factory, engine, 10);

        // Act
        var result = await step("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Evolution error");
    }

    [Fact]
    public async Task CreateEvolveFromInputStep_WhenNoBest_ReturnsFailure()
    {
        // Arrange
        var evolvedPopulation = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0),
        });

        var engine = new FakeEvolutionEngine(
            Result<EvolutionPopulation<SimpleChromosome>>.Success(evolvedPopulation),
            Option<SimpleChromosome>.None());

        Func<string, Result<EvolutionPopulation<SimpleChromosome>>> factory = input =>
            Result<EvolutionPopulation<SimpleChromosome>>.Success(
                new EvolutionPopulation<SimpleChromosome>(new[] { new SimpleChromosome(1.0) }));

        var step = GeneticEvolutionStep.CreateEvolveFromInputStep(factory, engine, 10);

        // Act
        var result = await step("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No best chromosome");
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
