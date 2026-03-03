// <copyright file="EvolutionMutationDeepTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Core;

using FluentAssertions;
using Ouroboros.Genetic.Core;
using Ouroboros.Tests.Genetic;
using Xunit;

/// <summary>
/// Deep unit tests for EvolutionMutation covering boundary rates,
/// constructor variants, and edge cases not in MutationTests.
/// </summary>
[Trait("Category", "Unit")]
public class EvolutionMutationDeepTests
{
    [Fact]
    public void Constructor_WithZeroRate_IsValid()
    {
        // Act
        var mutation = new EvolutionMutation(0.0);

        // Assert
        mutation.MutationRate.Should().Be(0.0);
    }

    [Fact]
    public void Constructor_WithOneRate_IsValid()
    {
        // Act
        var mutation = new EvolutionMutation(1.0);

        // Assert
        mutation.MutationRate.Should().Be(1.0);
    }

    [Fact]
    public void Constructor_WithSeedInt_CreatesInstance()
    {
        // Act
        var mutation = new EvolutionMutation(0.5, seed: 123);

        // Assert
        mutation.Should().NotBeNull();
        mutation.MutationRate.Should().Be(0.5);
    }

    [Fact]
    public void Constructor_WithDefaultRate_IsValid()
    {
        // Act
        var mutation = new EvolutionMutation();

        // Assert
        mutation.MutationRate.Should().Be(0.1);
    }

    [Fact]
    public void Constructor_RateJustBelowZero_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new EvolutionMutation(-0.001));
    }

    [Fact]
    public void Constructor_RateJustAboveOne_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new EvolutionMutation(1.001));
    }

    [Fact]
    public void MutationRate_ReturnsConfiguredRate()
    {
        // Arrange
        var mutation = new EvolutionMutation(0.42);

        // Assert
        mutation.MutationRate.Should().Be(0.42);
    }

    [Fact]
    public void Mutate_WithAlwaysMutate_CallsMutationFunc()
    {
        // Arrange
        var mutation = new EvolutionMutation(1.0, seed: 42);
        var chromosome = new SimpleChromosome(10.0);
        var mutationCalled = false;

        Func<SimpleChromosome, Ouroboros.Providers.Random.IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) =>
            {
                mutationCalled = true;
                return Result<SimpleChromosome>.Success(new SimpleChromosome(99.0));
            };

        // Act
        var result = mutation.Mutate(chromosome, mutationFunc);

        // Assert
        mutationCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(99.0);
    }

    [Fact]
    public void Mutate_WithNeverMutate_DoesNotCallMutationFunc()
    {
        // Arrange
        var mutation = new EvolutionMutation(0.0);
        var chromosome = new SimpleChromosome(10.0);
        var mutationCalled = false;

        Func<SimpleChromosome, Ouroboros.Providers.Random.IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) =>
            {
                mutationCalled = true;
                return Result<SimpleChromosome>.Success(new SimpleChromosome(99.0));
            };

        // Act
        var result = mutation.Mutate(chromosome, mutationFunc);

        // Assert
        mutationCalled.Should().BeFalse();
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(10.0);
    }

    [Fact]
    public async Task MutatePopulationAsync_WithEmptyPopulation_ReturnsEmptyPopulation()
    {
        // Arrange
        var mutation = new EvolutionMutation(1.0, seed: 42);
        var population = new EvolutionPopulation<SimpleChromosome>(Array.Empty<SimpleChromosome>());

        Func<SimpleChromosome, Ouroboros.Providers.Random.IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) => Result<SimpleChromosome>.Success(new SimpleChromosome(c.Value + 1));

        // Act
        var result = await mutation.MutatePopulationAsync(population, mutationFunc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Size.Should().Be(0);
    }

    [Fact]
    public async Task MutatePopulationAsync_PreservesPopulationSize()
    {
        // Arrange
        var mutation = new EvolutionMutation(0.5, seed: 42);
        var chromosomes = new[]
        {
            new SimpleChromosome(1.0),
            new SimpleChromosome(2.0),
            new SimpleChromosome(3.0),
            new SimpleChromosome(4.0),
        };
        var population = new EvolutionPopulation<SimpleChromosome>(chromosomes);

        Func<SimpleChromosome, Ouroboros.Providers.Random.IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) => Result<SimpleChromosome>.Success(new SimpleChromosome(c.Value * 2));

        // Act
        var result = await mutation.MutatePopulationAsync(population, mutationFunc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Size.Should().Be(4);
    }

    [Fact]
    public void Mutate_WithSeed_IsDeterministic()
    {
        // Arrange
        var m1 = new EvolutionMutation(0.5, seed: 42);
        var m2 = new EvolutionMutation(0.5, seed: 42);
        var chromosome = new SimpleChromosome(10.0);

        Func<SimpleChromosome, Ouroboros.Providers.Random.IRandomProvider, Result<SimpleChromosome>> mutationFunc =
            (c, random) => Result<SimpleChromosome>.Success(new SimpleChromosome(c.Value + random.NextDouble()));

        // Act
        var r1 = m1.Mutate(chromosome, mutationFunc);
        var r2 = m2.Mutate(chromosome, mutationFunc);

        // Assert
        r1.Value.Value.Should().Be(r2.Value.Value);
    }
}
