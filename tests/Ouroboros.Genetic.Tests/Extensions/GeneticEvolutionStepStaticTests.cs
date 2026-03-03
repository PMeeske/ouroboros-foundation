// <copyright file="GeneticEvolutionStepStaticTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Extensions;

using FluentAssertions;
using Moq;
using Ouroboros.Core.Steps;
using Ouroboros.Genetic.Abstractions;
using Ouroboros.Genetic.Core;
using Ouroboros.Genetic.Extensions;
using Ouroboros.Tests.Genetic;
using Xunit;

/// <summary>
/// Tests for the static GeneticEvolutionStep class in Genetic/Extensions.
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

        var mockEngine = new Mock<IEvolutionEngine<SimpleChromosome>>();
        mockEngine.Setup(e => e.EvolveAsync(It.IsAny<EvolutionPopulation<SimpleChromosome>>(), 10))
            .ReturnsAsync(Result<EvolutionPopulation<SimpleChromosome>>.Success(evolvedPopulation));
        mockEngine.Setup(e => e.GetBest(It.IsAny<EvolutionPopulation<SimpleChromosome>>()))
            .Returns(Option<SimpleChromosome>.Some(bestChromosome));

        var step = GeneticEvolutionStep.CreateEvolutionStep(mockEngine.Object, 10);
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
        var mockEngine = new Mock<IEvolutionEngine<SimpleChromosome>>();
        mockEngine.Setup(e => e.EvolveAsync(It.IsAny<EvolutionPopulation<SimpleChromosome>>(), It.IsAny<int>()))
            .ReturnsAsync(Result<EvolutionPopulation<SimpleChromosome>>.Failure("Engine error"));

        var step = GeneticEvolutionStep.CreateEvolutionStep(mockEngine.Object, 5);
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
        var mockEngine = new Mock<IEvolutionEngine<SimpleChromosome>>();
        var step = GeneticEvolutionStep.CreateEvolutionStep(mockEngine.Object, 5);

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

        var mockEngine = new Mock<IEvolutionEngine<SimpleChromosome>>();
        mockEngine.Setup(e => e.EvolveAsync(It.IsAny<EvolutionPopulation<SimpleChromosome>>(), It.IsAny<int>()))
            .ReturnsAsync(Result<EvolutionPopulation<SimpleChromosome>>.Success(evolvedPopulation));
        mockEngine.Setup(e => e.GetBest(It.IsAny<EvolutionPopulation<SimpleChromosome>>()))
            .Returns(Option<SimpleChromosome>.None());

        var step = GeneticEvolutionStep.CreateEvolutionStep(mockEngine.Object, 5);
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

        var mockEngine = new Mock<IEvolutionEngine<SimpleChromosome>>();
        mockEngine.Setup(e => e.EvolveAsync(It.IsAny<EvolutionPopulation<SimpleChromosome>>(), 15))
            .ReturnsAsync(Result<EvolutionPopulation<SimpleChromosome>>.Success(evolvedPopulation));

        var step = GeneticEvolutionStep.CreatePopulationEvolutionStep(mockEngine.Object, 15);
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
        var mockEngine = new Mock<IEvolutionEngine<SimpleChromosome>>();
        var step = GeneticEvolutionStep.CreatePopulationEvolutionStep(mockEngine.Object, 5);

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
        var mockEngine = new Mock<IEvolutionEngine<SimpleChromosome>>();
        mockEngine.Setup(e => e.EvolveAsync(It.IsAny<EvolutionPopulation<SimpleChromosome>>(), It.IsAny<int>()))
            .ReturnsAsync(Result<EvolutionPopulation<SimpleChromosome>>.Failure("Population too small"));

        var step = GeneticEvolutionStep.CreatePopulationEvolutionStep(mockEngine.Object, 5);
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

        var mockEngine = new Mock<IEvolutionEngine<SimpleChromosome>>();
        mockEngine.Setup(e => e.EvolveAsync(It.IsAny<EvolutionPopulation<SimpleChromosome>>(), 10))
            .ReturnsAsync(Result<EvolutionPopulation<SimpleChromosome>>.Success(evolvedPopulation));
        mockEngine.Setup(e => e.GetBest(It.IsAny<EvolutionPopulation<SimpleChromosome>>()))
            .Returns(Option<SimpleChromosome>.Some(bestChromosome));

        Func<string, Result<EvolutionPopulation<SimpleChromosome>>> factory = input =>
        {
            var pop = new EvolutionPopulation<SimpleChromosome>(new[]
            {
                new SimpleChromosome(input.Length),
            });
            return Result<EvolutionPopulation<SimpleChromosome>>.Success(pop);
        };

        var step = GeneticEvolutionStep.CreateEvolveFromInputStep(factory, mockEngine.Object, 10);

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
        var mockEngine = new Mock<IEvolutionEngine<SimpleChromosome>>();

        Func<string, Result<EvolutionPopulation<SimpleChromosome>>> factory = input =>
            Result<EvolutionPopulation<SimpleChromosome>>.Failure("Invalid input");

        var step = GeneticEvolutionStep.CreateEvolveFromInputStep(factory, mockEngine.Object, 10);

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
        var mockEngine = new Mock<IEvolutionEngine<SimpleChromosome>>();
        mockEngine.Setup(e => e.EvolveAsync(It.IsAny<EvolutionPopulation<SimpleChromosome>>(), It.IsAny<int>()))
            .ReturnsAsync(Result<EvolutionPopulation<SimpleChromosome>>.Failure("Evolution error"));

        Func<string, Result<EvolutionPopulation<SimpleChromosome>>> factory = input =>
            Result<EvolutionPopulation<SimpleChromosome>>.Success(
                new EvolutionPopulation<SimpleChromosome>(new[] { new SimpleChromosome(1.0) }));

        var step = GeneticEvolutionStep.CreateEvolveFromInputStep(factory, mockEngine.Object, 10);

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

        var mockEngine = new Mock<IEvolutionEngine<SimpleChromosome>>();
        mockEngine.Setup(e => e.EvolveAsync(It.IsAny<EvolutionPopulation<SimpleChromosome>>(), It.IsAny<int>()))
            .ReturnsAsync(Result<EvolutionPopulation<SimpleChromosome>>.Success(evolvedPopulation));
        mockEngine.Setup(e => e.GetBest(It.IsAny<EvolutionPopulation<SimpleChromosome>>()))
            .Returns(Option<SimpleChromosome>.None());

        Func<string, Result<EvolutionPopulation<SimpleChromosome>>> factory = input =>
            Result<EvolutionPopulation<SimpleChromosome>>.Success(
                new EvolutionPopulation<SimpleChromosome>(new[] { new SimpleChromosome(1.0) }));

        var step = GeneticEvolutionStep.CreateEvolveFromInputStep(factory, mockEngine.Object, 10);

        // Act
        var result = await step("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No best chromosome");
    }
}
