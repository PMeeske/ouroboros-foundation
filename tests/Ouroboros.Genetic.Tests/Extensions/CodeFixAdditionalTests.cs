// <copyright file="CodeFixAdditionalTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Extensions;

using FluentAssertions;
using Ouroboros.Genetic.Extensions;
using Xunit;

/// <summary>
/// Additional tests for CodeFixChromosome, CodeFixFitnessFunction, and GeneticRoslynBridge edge cases.
/// </summary>
[Trait("Category", "Unit")]
public class CodeFixAdditionalTests
{
    [Fact]
    public void CodeFixChromosome_Constructor_WithNullGenes_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CodeFixChromosome(null!));
    }

    [Fact]
    public void CodeFixChromosome_WithGenes_ReturnsNewInstance()
    {
        // Arrange
        var original = new CodeFixChromosome(new[]
        {
            new CodeFixGene("a", 0.5, true),
        });
        var newGenes = new List<CodeFixGene>
        {
            new CodeFixGene("b", 0.8, false),
            new CodeFixGene("c", 0.3, true),
        };

        // Act
        var updated = original.WithGenes(newGenes);

        // Assert
        updated.Genes.Should().HaveCount(2);
        updated.Genes[0].FixStrategyId.Should().Be("b");
        original.Genes.Should().HaveCount(1); // Original unchanged
    }

    [Fact]
    public void CodeFixChromosome_IsImmutableAgainstExternalModification()
    {
        // Arrange
        var geneList = new List<CodeFixGene>
        {
            new CodeFixGene("a", 0.5, true),
        };
        var chromosome = new CodeFixChromosome(geneList);

        // Act - modify original list
        geneList.Add(new CodeFixGene("b", 0.3, false));

        // Assert - chromosome should be unaffected
        chromosome.Genes.Should().HaveCount(1);
    }

    [Fact]
    public void CodeFixChromosome_WithFitness_PreservesGenes()
    {
        // Arrange
        var genes = new[]
        {
            new CodeFixGene("fix1", 0.8, true),
            new CodeFixGene("fix2", 0.5, false),
        };
        var original = new CodeFixChromosome(genes, fitness: 0.3);

        // Act
        var updated = original.WithFitness(0.99);

        // Assert
        updated.Fitness.Should().Be(0.99);
        updated.Genes.Should().HaveCount(2);
        updated.Genes[0].FixStrategyId.Should().Be("fix1");
    }

    [Fact]
    public void CodeFixChromosome_GetActiveFixes_WithAllDisabled_ReturnsEmpty()
    {
        // Arrange
        var genes = new[]
        {
            new CodeFixGene("a", 0.8, false),
            new CodeFixGene("b", 0.5, false),
        };
        var chromosome = new CodeFixChromosome(genes);

        // Act
        var active = chromosome.GetActiveFixes();

        // Assert
        active.Should().BeEmpty();
    }

    [Fact]
    public void CodeFixChromosome_GetActiveFixes_WithAllBelowThreshold_ReturnsEmpty()
    {
        // Arrange
        var genes = new[]
        {
            new CodeFixGene("a", 0.05, true), // Below 0.1 threshold
            new CodeFixGene("b", 0.09, true), // Below 0.1 threshold
        };
        var chromosome = new CodeFixChromosome(genes);

        // Act
        var active = chromosome.GetActiveFixes();

        // Assert
        active.Should().BeEmpty();
    }

    [Fact]
    public void CodeFixChromosome_GetActiveFixes_WithExactThreshold_ReturnsEmpty()
    {
        // Arrange - Strength must be > 0.1, not >= 0.1
        var genes = new[]
        {
            new CodeFixGene("a", 0.1, true),
        };
        var chromosome = new CodeFixChromosome(genes);

        // Act
        var active = chromosome.GetActiveFixes();

        // Assert - 0.1 is NOT > 0.1
        active.Should().BeEmpty();
    }

    [Fact]
    public void CodeFixChromosome_GetActiveFixes_WithAboveThreshold_ReturnsActive()
    {
        // Arrange
        var genes = new[]
        {
            new CodeFixGene("a", 0.11, true), // Just above threshold
        };
        var chromosome = new CodeFixChromosome(genes);

        // Act
        var active = chromosome.GetActiveFixes();

        // Assert
        active.Should().HaveCount(1);
    }

    [Fact]
    public void CodeFixChromosome_GetActiveFixes_WithEmptyGenes_ReturnsEmpty()
    {
        // Arrange
        var chromosome = new CodeFixChromosome(Array.Empty<CodeFixGene>());

        // Act
        var active = chromosome.GetActiveFixes();

        // Assert
        active.Should().BeEmpty();
    }

    [Fact]
    public async Task CodeFixFitnessFunction_Constructor_WithNullEvaluator_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new CodeFixFitnessFunction(null!));
    }

    [Fact]
    public async Task CodeFixFitnessFunction_WithNullChromosome_ThrowsArgumentNullException()
    {
        // Arrange
        var fitness = new CodeFixFitnessFunction(genes =>
            Task.FromResult(new CodeFixEvaluationResult(true, 1, 1, 0.5)));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            fitness.EvaluateAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task CodeFixFitnessFunction_WithMaxQualityImprovement_ReturnsMaxScore()
    {
        // Arrange
        var fitness = new CodeFixFitnessFunction(genes =>
            Task.FromResult(new CodeFixEvaluationResult(
                CompilationSucceeded: true,
                DiagnosticsFixed: 100,
                TotalDiagnostics: 100,
                QualityImprovement: 1.0)));

        var chromosome = new CodeFixChromosome(new[]
        {
            new CodeFixGene("fix", 0.8, true),
        });

        // Act
        var score = await fitness.EvaluateAsync(chromosome, CancellationToken.None);

        // Assert
        score.Should().Be(1.0); // 0.5 + 0.3*(100/100) + 0.2*1.0 = 1.0
    }

    [Fact]
    public async Task CodeFixFitnessFunction_WithPartialResults_ReturnsPartialScore()
    {
        // Arrange
        var fitness = new CodeFixFitnessFunction(genes =>
            Task.FromResult(new CodeFixEvaluationResult(
                CompilationSucceeded: true,
                DiagnosticsFixed: 5,
                TotalDiagnostics: 10,
                QualityImprovement: 0.0)));

        var chromosome = new CodeFixChromosome(new[]
        {
            new CodeFixGene("fix", 0.8, true),
        });

        // Act
        var score = await fitness.EvaluateAsync(chromosome, CancellationToken.None);

        // Assert
        // 0.5 + 0.3 * (5/10) + 0.2 * 0.0 = 0.65
        score.Should().BeApproximately(0.65, 0.001);
    }

    [Fact]
    public async Task CodeFixFitnessFunction_WithZeroDiagnostics_HandlesZeroDivision()
    {
        // Arrange
        var fitness = new CodeFixFitnessFunction(genes =>
            Task.FromResult(new CodeFixEvaluationResult(
                CompilationSucceeded: true,
                DiagnosticsFixed: 0,
                TotalDiagnostics: 0,
                QualityImprovement: 0.5)));

        var chromosome = new CodeFixChromosome(new[]
        {
            new CodeFixGene("fix", 0.8, true),
        });

        // Act
        var score = await fitness.EvaluateAsync(chromosome, CancellationToken.None);

        // Assert
        // 0.5 + 0.3*(0/max(1,0)) + 0.2*0.5 = 0.5 + 0 + 0.1 = 0.6
        score.Should().BeApproximately(0.6, 0.001);
    }

    [Fact]
    public async Task CodeFixFitnessFunction_ScoreIsClampedToOne()
    {
        // Arrange
        var fitness = new CodeFixFitnessFunction(genes =>
            Task.FromResult(new CodeFixEvaluationResult(
                CompilationSucceeded: true,
                DiagnosticsFixed: 1000,
                TotalDiagnostics: 1,
                QualityImprovement: 10.0))); // Extreme values

        var chromosome = new CodeFixChromosome(new[]
        {
            new CodeFixGene("fix", 0.8, true),
        });

        // Act
        var score = await fitness.EvaluateAsync(chromosome, CancellationToken.None);

        // Assert
        score.Should().BeLessThanOrEqualTo(1.0);
    }

    [Fact]
    public void CodeFixGene_RecordEquality()
    {
        // Arrange
        var gene1 = new CodeFixGene("add_null_check", 0.5, true);
        var gene2 = new CodeFixGene("add_null_check", 0.5, true);
        var gene3 = new CodeFixGene("add_null_check", 0.6, true);

        // Assert
        gene1.Should().Be(gene2);
        gene1.Should().NotBe(gene3);
    }

    [Fact]
    public void CodeFixEvaluationResult_RecordEquality()
    {
        // Arrange
        var result1 = new CodeFixEvaluationResult(true, 5, 10, 0.8);
        var result2 = new CodeFixEvaluationResult(true, 5, 10, 0.8);
        var result3 = new CodeFixEvaluationResult(false, 5, 10, 0.8);

        // Assert
        result1.Should().Be(result2);
        result1.Should().NotBe(result3);
    }

    [Fact]
    public void GeneticRoslynBridge_CreateInitialPopulation_WithZeroSize_ReturnsEmpty()
    {
        // Act
        var population = GeneticRoslynBridge.CreateInitialPopulation(0);

        // Assert
        population.Should().BeEmpty();
    }

    [Fact]
    public void GeneticRoslynBridge_CreateInitialPopulation_UsesDefaultStrategies()
    {
        // Act
        var population = GeneticRoslynBridge.CreateInitialPopulation(1, seed: 42);

        // Assert
        population.Should().HaveCount(1);
        population[0].Genes.Should().HaveCount(10); // 10 default strategies
    }

    [Fact]
    public void GeneticRoslynBridge_Mutate_WithZeroRate_PreservesAllGenes()
    {
        // Arrange
        var original = new CodeFixChromosome(new[]
        {
            new CodeFixGene("a", 0.5, true),
            new CodeFixGene("b", 0.3, false),
        });

        // Act
        var mutated = GeneticRoslynBridge.Mutate(original, mutationRate: 0.0, seed: 42);

        // Assert
        mutated.Genes.Should().Equal(original.Genes);
    }

    [Fact]
    public void GeneticRoslynBridge_Crossover_WithSingleGene_Works()
    {
        // Arrange
        var parent1 = new CodeFixChromosome(new[]
        {
            new CodeFixGene("a", 1.0, true),
        });
        var parent2 = new CodeFixChromosome(new[]
        {
            new CodeFixGene("a", 0.0, false),
        });

        // Act
        var (child1, child2) = GeneticRoslynBridge.Crossover(parent1, parent2, seed: 42);

        // Assert
        child1.Genes.Should().HaveCount(1);
        child2.Genes.Should().HaveCount(1);
    }

    [Fact]
    public void GeneticRoslynBridge_Crossover_WithDifferentLengths_UsesMinLength()
    {
        // Arrange
        var parent1 = new CodeFixChromosome(new[]
        {
            new CodeFixGene("a", 0.5, true),
            new CodeFixGene("b", 0.5, true),
            new CodeFixGene("c", 0.5, true),
        });
        var parent2 = new CodeFixChromosome(new[]
        {
            new CodeFixGene("x", 0.1, false),
            new CodeFixGene("y", 0.1, false),
        });

        // Act
        var (child1, child2) = GeneticRoslynBridge.Crossover(parent1, parent2, seed: 42);

        // Assert
        child1.Genes.Should().HaveCount(2); // min(3, 2)
        child2.Genes.Should().HaveCount(2);
    }

    [Fact]
    public void GeneticRoslynBridge_Mutate_WithSeed_IsDeterministic()
    {
        // Arrange
        var original = new CodeFixChromosome(new[]
        {
            new CodeFixGene("a", 0.5, true),
            new CodeFixGene("b", 0.5, true),
            new CodeFixGene("c", 0.5, true),
        });

        // Act
        var mutated1 = GeneticRoslynBridge.Mutate(original, mutationRate: 0.5, seed: 42);
        var mutated2 = GeneticRoslynBridge.Mutate(original, mutationRate: 0.5, seed: 42);

        // Assert
        mutated1.Genes.Should().BeEquivalentTo(mutated2.Genes);
    }

    [Fact]
    public void GeneticRoslynBridge_Crossover_WithSeed_IsDeterministic()
    {
        // Arrange
        var parent1 = new CodeFixChromosome(new[]
        {
            new CodeFixGene("a", 1.0, true),
            new CodeFixGene("b", 1.0, true),
        });
        var parent2 = new CodeFixChromosome(new[]
        {
            new CodeFixGene("x", 0.0, false),
            new CodeFixGene("y", 0.0, false),
        });

        // Act
        var result1 = GeneticRoslynBridge.Crossover(parent1, parent2, seed: 42);
        var result2 = GeneticRoslynBridge.Crossover(parent1, parent2, seed: 42);

        // Assert
        result1.child1.Genes.Should().BeEquivalentTo(result2.child1.Genes);
        result1.child2.Genes.Should().BeEquivalentTo(result2.child2.Genes);
    }

    [Fact]
    public void GeneticRoslynBridge_Crossover_WithEmptyParents_ReturnsEmptyChildren()
    {
        // Arrange
        var parent1 = new CodeFixChromosome(Array.Empty<CodeFixGene>());
        var parent2 = new CodeFixChromosome(Array.Empty<CodeFixGene>());

        // Act
        var (child1, child2) = GeneticRoslynBridge.Crossover(parent1, parent2, seed: 42);

        // Assert
        child1.Genes.Should().BeEmpty();
        child2.Genes.Should().BeEmpty();
    }
}
