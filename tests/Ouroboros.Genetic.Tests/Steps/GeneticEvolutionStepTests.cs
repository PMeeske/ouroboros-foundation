// <copyright file="GeneticEvolutionStepTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Steps;

using FluentAssertions;
using Moq;
using Ouroboros.Core.Steps;
using Ouroboros.Genetic.Abstractions;
using Ouroboros.Genetic.Core;
using Ouroboros.Genetic.Steps;
using Xunit;

/// <summary>
/// Tests for the GeneticEvolutionStep{TIn, TOut, TGene} class.
/// </summary>
[Trait("Category", "Unit")]
public class GeneticEvolutionStepTests
{
    [Fact]
    public void Constructor_WithNullAlgorithm_ThrowsArgumentNullException()
    {
        // Arrange
        Func<int, Step<string, string>> stepFactory = gene => input => Task.FromResult(input + gene);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GeneticEvolutionStep<string, string, int>(null!, stepFactory));
    }

    [Fact]
    public void Constructor_WithNullStepFactory_ThrowsArgumentNullException()
    {
        // Arrange
        var mockAlgorithm = new Mock<IGeneticAlgorithm<int>>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GeneticEvolutionStep<string, string, int>(mockAlgorithm.Object, null!));
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var mockAlgorithm = new Mock<IGeneticAlgorithm<int>>();
        Func<int, Step<string, string>> stepFactory = gene => input => Task.FromResult(input + gene);

        // Act
        var step = new GeneticEvolutionStep<string, string, int>(mockAlgorithm.Object, stepFactory);

        // Assert
        step.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateEvolvedStep_WhenEvolutionSucceeds_ReturnsSuccess()
    {
        // Arrange
        var bestChromosome = new Chromosome<int>(new List<int> { 5 }, fitness: 1.0);
        var mockAlgorithm = new Mock<IGeneticAlgorithm<int>>();
        mockAlgorithm.Setup(a => a.EvolveAsync(
                It.IsAny<IReadOnlyList<IChromosome<int>>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IChromosome<int>, string>.Success(bestChromosome));

        Func<int, Step<string, string>> stepFactory = gene =>
            input => Task.FromResult(input + "-" + gene);

        var evolutionStep = new GeneticEvolutionStep<string, string, int>(
            mockAlgorithm.Object, stepFactory);

        var initialPopulation = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
            new Chromosome<int>(new List<int> { 2 }),
        };

        // Act
        var step = evolutionStep.CreateEvolvedStep(initialPopulation, 10, CancellationToken.None);
        var result = await step("hello");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello-5");
    }

    [Fact]
    public async Task CreateEvolvedStep_WhenEvolutionFails_ReturnsFailure()
    {
        // Arrange
        var mockAlgorithm = new Mock<IGeneticAlgorithm<int>>();
        mockAlgorithm.Setup(a => a.EvolveAsync(
                It.IsAny<IReadOnlyList<IChromosome<int>>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IChromosome<int>, string>.Failure("Evolution failed"));

        Func<int, Step<string, string>> stepFactory = gene =>
            input => Task.FromResult(input);

        var evolutionStep = new GeneticEvolutionStep<string, string, int>(
            mockAlgorithm.Object, stepFactory);

        var initialPopulation = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
        };

        // Act
        var step = evolutionStep.CreateEvolvedStep(initialPopulation, 10, CancellationToken.None);
        var result = await step("hello");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Evolution failed");
    }

    [Fact]
    public async Task CreateEvolvedStep_WhenBestChromosomeHasNoGenes_ReturnsFailure()
    {
        // Arrange - use string (reference type) so that FirstOrDefault() returns null
        var emptyChromosome = new Chromosome<string>(new List<string>(), fitness: 1.0);
        var mockAlgorithm = new Mock<IGeneticAlgorithm<string>>();
        mockAlgorithm.Setup(a => a.EvolveAsync(
                It.IsAny<IReadOnlyList<IChromosome<string>>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IChromosome<string>, string>.Success(emptyChromosome));

        Func<string, Step<string, string>> stepFactory = gene =>
            input => Task.FromResult(input);

        var evolutionStep = new GeneticEvolutionStep<string, string, string>(
            mockAlgorithm.Object, stepFactory);

        var initialPopulation = new List<IChromosome<string>>
        {
            new Chromosome<string>(new List<string> { "a" }),
        };

        // Act
        var step = evolutionStep.CreateEvolvedStep(initialPopulation, 10, CancellationToken.None);
        var result = await step("hello");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no genes");
    }

    [Fact]
    public async Task CreateEvolvedStep_WhenOptimizedStepThrows_ReturnsFailure()
    {
        // Arrange
        var bestChromosome = new Chromosome<int>(new List<int> { 5 }, fitness: 1.0);
        var mockAlgorithm = new Mock<IGeneticAlgorithm<int>>();
        mockAlgorithm.Setup(a => a.EvolveAsync(
                It.IsAny<IReadOnlyList<IChromosome<int>>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IChromosome<int>, string>.Success(bestChromosome));

        Func<int, Step<string, string>> stepFactory = gene =>
            new Step<string, string>(_ => throw new InvalidOperationException("Step blew up"));

        var evolutionStep = new GeneticEvolutionStep<string, string, int>(
            mockAlgorithm.Object, stepFactory);

        var initialPopulation = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
        };

        // Act
        var step = evolutionStep.CreateEvolvedStep(initialPopulation, 10, CancellationToken.None);
        var result = await step("hello");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Optimized step execution failed");
        result.Error.Should().Contain("Step blew up");
    }

    [Fact]
    public async Task CreateEvolvedStep_WhenCancelled_PropagatesCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var mockAlgorithm = new Mock<IGeneticAlgorithm<int>>();
        mockAlgorithm.Setup(a => a.EvolveAsync(
                It.IsAny<IReadOnlyList<IChromosome<int>>>(),
                It.IsAny<int>(),
                cts.Token))
            .ThrowsAsync(new OperationCanceledException());

        Func<int, Step<string, string>> stepFactory = gene =>
            input => Task.FromResult(input);

        var evolutionStep = new GeneticEvolutionStep<string, string, int>(
            mockAlgorithm.Object, stepFactory);

        var initialPopulation = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
        };

        // Act
        var step = evolutionStep.CreateEvolvedStep(initialPopulation, 10, cts.Token);

        // Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => step("hello"));
    }

    [Fact]
    public async Task CreateEvolvedStepWithMetadata_WhenEvolutionSucceeds_ReturnsMetadata()
    {
        // Arrange
        var bestChromosome = new Chromosome<int>(new List<int> { 7 }, fitness: 0.95);
        var mockAlgorithm = new Mock<IGeneticAlgorithm<int>>();
        mockAlgorithm.Setup(a => a.EvolveAsync(
                It.IsAny<IReadOnlyList<IChromosome<int>>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IChromosome<int>, string>.Success(bestChromosome));

        Func<int, Step<string, string>> stepFactory = gene =>
            input => Task.FromResult(input + "-" + gene);

        var evolutionStep = new GeneticEvolutionStep<string, string, int>(
            mockAlgorithm.Object, stepFactory);

        var initialPopulation = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
        };

        // Act
        var step = evolutionStep.CreateEvolvedStepWithMetadata(initialPopulation, 10, CancellationToken.None);
        var result = await step("hello");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Output.Should().Be("hello-7");
        result.Value.BestChromosome.Should().NotBeNull();
        result.Value.BestChromosome.Fitness.Should().Be(0.95);
        result.Value.BestChromosome.Genes.Should().Equal(7);
    }

    [Fact]
    public async Task CreateEvolvedStepWithMetadata_WhenEvolutionFails_ReturnsFailure()
    {
        // Arrange
        var mockAlgorithm = new Mock<IGeneticAlgorithm<int>>();
        mockAlgorithm.Setup(a => a.EvolveAsync(
                It.IsAny<IReadOnlyList<IChromosome<int>>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IChromosome<int>, string>.Failure("Too few chromosomes"));

        Func<int, Step<string, string>> stepFactory = gene =>
            input => Task.FromResult(input);

        var evolutionStep = new GeneticEvolutionStep<string, string, int>(
            mockAlgorithm.Object, stepFactory);

        var initialPopulation = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
        };

        // Act
        var step = evolutionStep.CreateEvolvedStepWithMetadata(initialPopulation, 5, CancellationToken.None);
        var result = await step("test");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Too few chromosomes");
    }

    [Fact]
    public async Task CreateEvolvedStepWithMetadata_WhenNoGenes_ReturnsFailure()
    {
        // Arrange - use string (reference type) so that FirstOrDefault() returns null
        var emptyChromosome = new Chromosome<string>(new List<string>(), fitness: 0.5);
        var mockAlgorithm = new Mock<IGeneticAlgorithm<string>>();
        mockAlgorithm.Setup(a => a.EvolveAsync(
                It.IsAny<IReadOnlyList<IChromosome<string>>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IChromosome<string>, string>.Success(emptyChromosome));

        Func<string, Step<string, string>> stepFactory = gene =>
            input => Task.FromResult(input);

        var evolutionStep = new GeneticEvolutionStep<string, string, string>(
            mockAlgorithm.Object, stepFactory);

        var initialPopulation = new List<IChromosome<string>>
        {
            new Chromosome<string>(new List<string> { "a" }),
        };

        // Act
        var step = evolutionStep.CreateEvolvedStepWithMetadata(initialPopulation, 5, CancellationToken.None);
        var result = await step("test");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no genes");
    }

    [Fact]
    public async Task CreateEvolvedStepWithMetadata_WhenStepThrows_ReturnsFailure()
    {
        // Arrange
        var bestChromosome = new Chromosome<int>(new List<int> { 1 }, fitness: 0.5);
        var mockAlgorithm = new Mock<IGeneticAlgorithm<int>>();
        mockAlgorithm.Setup(a => a.EvolveAsync(
                It.IsAny<IReadOnlyList<IChromosome<int>>>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IChromosome<int>, string>.Success(bestChromosome));

        Func<int, Step<string, string>> stepFactory = gene =>
            new Step<string, string>(_ => throw new InvalidOperationException("Boom"));

        var evolutionStep = new GeneticEvolutionStep<string, string, int>(
            mockAlgorithm.Object, stepFactory);

        var initialPopulation = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
        };

        // Act
        var step = evolutionStep.CreateEvolvedStepWithMetadata(initialPopulation, 5, CancellationToken.None);
        var result = await step("test");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Optimized step execution failed");
    }

    [Fact]
    public async Task CreateEvolvedStep_PassesCorrectParametersToAlgorithm()
    {
        // Arrange
        var bestChromosome = new Chromosome<int>(new List<int> { 1 }, fitness: 1.0);
        var mockAlgorithm = new Mock<IGeneticAlgorithm<int>>();
        mockAlgorithm.Setup(a => a.EvolveAsync(
                It.IsAny<IReadOnlyList<IChromosome<int>>>(),
                It.Is<int>(g => g == 25),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IChromosome<int>, string>.Success(bestChromosome));

        Func<int, Step<string, string>> stepFactory = gene =>
            input => Task.FromResult(input);

        var evolutionStep = new GeneticEvolutionStep<string, string, int>(
            mockAlgorithm.Object, stepFactory);

        var initialPopulation = new List<IChromosome<int>>
        {
            new Chromosome<int>(new List<int> { 1 }),
            new Chromosome<int>(new List<int> { 2 }),
        };

        // Act
        var step = evolutionStep.CreateEvolvedStep(initialPopulation, 25, CancellationToken.None);
        await step("test");

        // Assert
        mockAlgorithm.Verify(a => a.EvolveAsync(
            It.Is<IReadOnlyList<IChromosome<int>>>(p => p.Count == 2),
            25,
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
