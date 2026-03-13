// <copyright file="EvolutionPopulationDeepTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Core;

using FluentAssertions;
using Ouroboros.Genetic.Core;
using Ouroboros.Tests.Genetic;
using Xunit;

/// <summary>
/// Deep unit tests for EvolutionPopulation{TChromosome} covering immutability,
/// edge cases, and boundary conditions not in PopulationTests.
/// </summary>
[Trait("Category", "Unit")]
public class EvolutionPopulationDeepTests
{
    [Fact]
    public void WithChromosomes_ReturnsNewPopulationWithNewChromosomes()
    {
        // Arrange
        var original = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0),
        });
        var newChromosomes = new[]
        {
            new SimpleChromosome(10.0),
            new SimpleChromosome(20.0),
            new SimpleChromosome(30.0),
        };

        // Act
        var updated = original.WithChromosomes(newChromosomes);

        // Assert
        updated.Size.Should().Be(3);
        original.Size.Should().Be(1);
    }

    [Fact]
    public void Generation_WithEmptyPopulation_ReturnsZero()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(Array.Empty<SimpleChromosome>());

        // Act
        var generation = population.Generation;

        // Assert
        generation.Should().Be(0);
    }

    [Fact]
    public void Generation_WithMixedGenerations_ReturnsMax()
    {
        // Arrange
        var chromosomes = new[]
        {
            new SimpleChromosome(1.0, generation: 5),
            new SimpleChromosome(2.0, generation: 10),
            new SimpleChromosome(3.0, generation: 3),
        };
        var population = new EvolutionPopulation<SimpleChromosome>(chromosomes);

        // Act
        var generation = population.Generation;

        // Assert
        generation.Should().Be(10);
    }

    [Fact]
    public void Add_ToEmptyPopulation_CreatesSingleElementPopulation()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(Array.Empty<SimpleChromosome>());
        var chromosome = new SimpleChromosome(42.0);

        // Act
        var updated = population.Add(chromosome);

        // Assert
        updated.Size.Should().Be(1);
        updated.Chromosomes[0].Value.Should().Be(42.0);
    }

    [Fact]
    public void SortByFitness_WithSingleChromosome_ReturnsPopulationOfOne()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(5.0, fitness: 0.5),
        });

        // Act
        var sorted = population.SortByFitness();

        // Assert
        sorted.Size.Should().Be(1);
        sorted.Chromosomes[0].Fitness.Should().Be(0.5);
    }

    [Fact]
    public void SortByFitness_WithEqualFitness_DoesNotThrow()
    {
        // Arrange
        var chromosomes = new[]
        {
            new SimpleChromosome(1.0, fitness: 0.5),
            new SimpleChromosome(2.0, fitness: 0.5),
            new SimpleChromosome(3.0, fitness: 0.5),
        };
        var population = new EvolutionPopulation<SimpleChromosome>(chromosomes);

        // Act
        var sorted = population.SortByFitness();

        // Assert
        sorted.Size.Should().Be(3);
        sorted.Chromosomes.Should().AllSatisfy(c => c.Fitness.Should().Be(0.5));
    }

    [Fact]
    public void Take_MoreThanSize_ReturnsAll()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0, fitness: 0.3),
            new SimpleChromosome(2.0, fitness: 0.5),
        });

        // Act
        var top = population.Take(10);

        // Assert
        top.Size.Should().Be(2);
    }

    [Fact]
    public void Take_Zero_ReturnsEmptyPopulation()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0, fitness: 0.3),
        });

        // Act
        var top = population.Take(0);

        // Assert
        top.Size.Should().Be(0);
    }

    [Fact]
    public void GetBest_WithSingleChromosome_ReturnsThatChromosome()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(42.0, fitness: 0.7),
        });

        // Act
        var best = population.GetBest();

        // Assert
        best.HasValue.Should().BeTrue();
        best.Value!.Value.Should().Be(42.0);
    }

    [Fact]
    public void GetAverageFitness_WithSingleChromosome_ReturnsThatFitness()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0, fitness: 0.75),
        });

        // Act
        var avg = population.GetAverageFitness();

        // Assert
        avg.Should().BeApproximately(0.75, 0.0001);
    }

    [Fact]
    public void GetAverageFitness_WithNegativeFitness_CalculatesCorrectly()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0, fitness: -3.0),
            new SimpleChromosome(2.0, fitness: 5.0),
        });

        // Act
        var avg = population.GetAverageFitness();

        // Assert
        avg.Should().BeApproximately(1.0, 0.0001);
    }

    [Fact]
    public void Chromosomes_IsReadOnly_ExternalModificationDoesNotAffect()
    {
        // Arrange
        var chromosomes = new List<SimpleChromosome>
        {
            new SimpleChromosome(1.0),
            new SimpleChromosome(2.0),
        };
        var population = new EvolutionPopulation<SimpleChromosome>(chromosomes);

        // Act
        chromosomes.Add(new SimpleChromosome(3.0));

        // Assert
        population.Size.Should().Be(2);
    }

    [Fact]
    public void Add_MultipleTimes_AccumulatesChromosomes()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0),
        });

        // Act
        var p2 = population.Add(new SimpleChromosome(2.0));
        var p3 = p2.Add(new SimpleChromosome(3.0));

        // Assert
        population.Size.Should().Be(1);
        p2.Size.Should().Be(2);
        p3.Size.Should().Be(3);
    }

    [Fact]
    public void SortByFitness_OriginalPopulationUnchanged()
    {
        // Arrange
        var chromosomes = new[]
        {
            new SimpleChromosome(1.0, fitness: 0.1),
            new SimpleChromosome(2.0, fitness: 0.9),
        };
        var population = new EvolutionPopulation<SimpleChromosome>(chromosomes);

        // Act
        var sorted = population.SortByFitness();

        // Assert
        population.Chromosomes[0].Fitness.Should().Be(0.1);
        sorted.Chromosomes[0].Fitness.Should().Be(0.9);
    }

    [Fact]
    public void WithChromosomes_WithEmptyList_CreatesEmptyPopulation()
    {
        // Arrange
        var population = new EvolutionPopulation<SimpleChromosome>(new[]
        {
            new SimpleChromosome(1.0),
        });

        // Act
        var empty = population.WithChromosomes(Array.Empty<SimpleChromosome>());

        // Assert
        empty.Size.Should().Be(0);
    }
}
