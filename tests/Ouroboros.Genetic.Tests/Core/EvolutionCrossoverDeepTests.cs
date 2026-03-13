// <copyright file="EvolutionCrossoverDeepTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Core;

using FluentAssertions;
using Ouroboros.Genetic.Core;
using Ouroboros.Tests.Genetic;
using Xunit;

/// <summary>
/// Deep unit tests for EvolutionCrossover covering boundary rates,
/// constructor variants, and edge cases not in UniformCrossoverTests.
/// </summary>
[Trait("Category", "Unit")]
public class EvolutionCrossoverDeepTests
{
    [Fact]
    public void Constructor_WithDefaultRate_IsValid()
    {
        // Act
        var crossover = new EvolutionCrossover();

        // Assert
        crossover.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithZeroRate_IsValid()
    {
        // Act
        var crossover = new EvolutionCrossover(0.0);

        // Assert
        crossover.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithOneRate_IsValid()
    {
        // Act
        var crossover = new EvolutionCrossover(1.0);

        // Assert
        crossover.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithSeedInt_CreatesInstance()
    {
        // Act
        var crossover = new EvolutionCrossover(0.5, seed: 42);

        // Assert
        crossover.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_RateSlightlyBelowZero_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new EvolutionCrossover(-0.001));
    }

    [Fact]
    public void Constructor_RateSlightlyAboveOne_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new EvolutionCrossover(1.001));
    }

    [Fact]
    public void Crossover_WithAlwaysCrossover_CallsCrossoverFunc()
    {
        // Arrange
        var crossover = new EvolutionCrossover(1.0, seed: 42);
        var parent1 = new SimpleChromosome(10.0);
        var parent2 = new SimpleChromosome(20.0);
        var crossoverCalled = false;

        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) =>
            {
                crossoverCalled = true;
                return Result<SimpleChromosome>.Success(new SimpleChromosome(p1.Value + p2.Value));
            };

        // Act
        var result = crossover.Crossover(parent1, parent2, crossoverFunc);

        // Assert
        crossoverCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(30.0);
    }

    [Fact]
    public void Crossover_WithNeverCrossover_ReturnsCloneOfParent1()
    {
        // Arrange
        var crossover = new EvolutionCrossover(0.0);
        var parent1 = new SimpleChromosome(10.0, fitness: 0.5);
        var parent2 = new SimpleChromosome(20.0, fitness: 0.8);

        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Success(new SimpleChromosome(99.0));

        // Act
        var result = crossover.Crossover(parent1, parent2, crossoverFunc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(10.0);
    }

    [Fact]
    public void Crossover_CrossoverFuncReturnsFailure_PropagatesError()
    {
        // Arrange
        var crossover = new EvolutionCrossover(1.0, seed: 42);
        var parent1 = new SimpleChromosome(10.0);
        var parent2 = new SimpleChromosome(20.0);

        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Failure("Crossover logic error");

        // Act
        var result = crossover.Crossover(parent1, parent2, crossoverFunc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Crossover logic error");
    }

    [Fact]
    public void CrossoverPair_WithNeverCrossover_ReturnsClonesOfBothParents()
    {
        // Arrange
        var crossover = new EvolutionCrossover(0.0);
        var parent1 = new SimpleChromosome(10.0);
        var parent2 = new SimpleChromosome(20.0);

        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Success(new SimpleChromosome(99.0));

        // Act
        var result = crossover.CrossoverPair(parent1, parent2, crossoverFunc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Offspring1.Value.Should().Be(10.0);
        result.Value.Offspring2.Value.Should().Be(20.0);
    }

    [Fact]
    public void CrossoverPair_WithAlwaysCrossover_ProducesTwoOffspring()
    {
        // Arrange
        var crossover = new EvolutionCrossover(1.0, seed: 42);
        var parent1 = new SimpleChromosome(10.0);
        var parent2 = new SimpleChromosome(20.0);

        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) =>
                Result<SimpleChromosome>.Success(new SimpleChromosome(p1.Value * ratio + p2.Value * (1 - ratio)));

        // Act
        var result = crossover.CrossoverPair(parent1, parent2, crossoverFunc);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Offspring1.Should().NotBeNull();
        result.Value.Offspring2.Should().NotBeNull();
    }

    [Fact]
    public void Crossover_WithSeed_IsDeterministic()
    {
        // Arrange
        var c1 = new EvolutionCrossover(0.5, seed: 42);
        var c2 = new EvolutionCrossover(0.5, seed: 42);
        var parent1 = new SimpleChromosome(10.0);
        var parent2 = new SimpleChromosome(20.0);

        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Success(new SimpleChromosome(p1.Value * ratio + p2.Value * (1 - ratio)));

        // Act
        var r1 = c1.Crossover(parent1, parent2, crossoverFunc);
        var r2 = c2.Crossover(parent1, parent2, crossoverFunc);

        // Assert
        r1.Value.Value.Should().Be(r2.Value.Value);
    }

    [Fact]
    public void Crossover_BothParentsNull_ReturnsFailure()
    {
        // Arrange
        var crossover = new EvolutionCrossover(1.0);

        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Success(new SimpleChromosome(0));

        // Act
        var result = crossover.Crossover<SimpleChromosome>(null!, null!, crossoverFunc);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot be null");
    }

    [Fact]
    public void CrossoverPair_WithBothNull_ReturnsFailure()
    {
        // Arrange
        var crossover = new EvolutionCrossover(1.0);

        Func<SimpleChromosome, SimpleChromosome, double, Result<SimpleChromosome>> crossoverFunc =
            (p1, p2, ratio) => Result<SimpleChromosome>.Success(new SimpleChromosome(0));

        // Act
        var result = crossover.CrossoverPair<SimpleChromosome>(null!, null!, crossoverFunc);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
