// <copyright file="CodeFixGeneRecordTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Genetic.Extensions;

using FluentAssertions;
using Ouroboros.Genetic.Extensions;
using Xunit;

/// <summary>
/// Unit tests for CodeFixGene record, CodeFixEvaluationResult record,
/// and GeneticRoslynBridge static method edge cases.
/// </summary>
[Trait("Category", "Unit")]
public class CodeFixGeneRecordTests
{
    [Fact]
    public void CodeFixGene_Properties_AreCorrect()
    {
        // Arrange & Act
        var gene = new CodeFixGene("add_null_check", 0.75, true);

        // Assert
        gene.FixStrategyId.Should().Be("add_null_check");
        gene.Strength.Should().Be(0.75);
        gene.Enabled.Should().BeTrue();
    }

    [Fact]
    public void CodeFixGene_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new CodeFixGene("fix", 0.5, true);

        // Act
        var modified = original with { Enabled = false };

        // Assert
        modified.FixStrategyId.Should().Be("fix");
        modified.Strength.Should().Be(0.5);
        modified.Enabled.Should().BeFalse();
        original.Enabled.Should().BeTrue();
    }

    [Fact]
    public void CodeFixGene_WithStrengthModified_CreatesModifiedCopy()
    {
        // Arrange
        var original = new CodeFixGene("fix", 0.5, true);

        // Act
        var modified = original with { Strength = 0.9 };

        // Assert
        modified.Strength.Should().Be(0.9);
        original.Strength.Should().Be(0.5);
    }

    [Fact]
    public void CodeFixGene_Equality_SameValues_AreEqual()
    {
        // Arrange
        var g1 = new CodeFixGene("id", 0.5, true);
        var g2 = new CodeFixGene("id", 0.5, true);

        // Assert
        g1.Should().Be(g2);
        (g1 == g2).Should().BeTrue();
    }

    [Fact]
    public void CodeFixGene_Equality_DifferentValues_AreNotEqual()
    {
        // Arrange
        var g1 = new CodeFixGene("id1", 0.5, true);
        var g2 = new CodeFixGene("id2", 0.5, true);

        // Assert
        g1.Should().NotBe(g2);
    }

    [Fact]
    public void CodeFixGene_GetHashCode_SameValues_SameHash()
    {
        // Arrange
        var g1 = new CodeFixGene("fix", 0.8, true);
        var g2 = new CodeFixGene("fix", 0.8, true);

        // Assert
        g1.GetHashCode().Should().Be(g2.GetHashCode());
    }

    [Fact]
    public void CodeFixEvaluationResult_Properties_AreCorrect()
    {
        // Arrange & Act
        var result = new CodeFixEvaluationResult(true, 5, 10, 0.8);

        // Assert
        result.CompilationSucceeded.Should().BeTrue();
        result.DiagnosticsFixed.Should().Be(5);
        result.TotalDiagnostics.Should().Be(10);
        result.QualityImprovement.Should().Be(0.8);
    }

    [Fact]
    public void CodeFixEvaluationResult_Equality_SameValues_AreEqual()
    {
        // Arrange
        var r1 = new CodeFixEvaluationResult(true, 3, 7, 0.5);
        var r2 = new CodeFixEvaluationResult(true, 3, 7, 0.5);

        // Assert
        r1.Should().Be(r2);
    }

    [Fact]
    public void CodeFixEvaluationResult_Equality_DifferentCompilation_AreNotEqual()
    {
        // Arrange
        var r1 = new CodeFixEvaluationResult(true, 3, 7, 0.5);
        var r2 = new CodeFixEvaluationResult(false, 3, 7, 0.5);

        // Assert
        r1.Should().NotBe(r2);
    }

    [Fact]
    public void CodeFixEvaluationResult_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new CodeFixEvaluationResult(true, 5, 10, 0.8);

        // Act
        var modified = original with { DiagnosticsFixed = 10 };

        // Assert
        modified.DiagnosticsFixed.Should().Be(10);
        original.DiagnosticsFixed.Should().Be(5);
    }

    [Fact]
    public void GeneticRoslynBridge_Mutate_WithFullRate_ChangesAllGenes()
    {
        // Arrange
        var original = new CodeFixChromosome(new[]
        {
            new CodeFixGene("a", 0.5, true),
            new CodeFixGene("b", 0.5, true),
            new CodeFixGene("c", 0.5, true),
        });

        // Act
        var mutated = GeneticRoslynBridge.Mutate(original, mutationRate: 1.0, seed: 42);

        // Assert
        var differences = mutated.Genes
            .Zip(original.Genes, (m, o) => m != o)
            .Count(d => d);
        differences.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GeneticRoslynBridge_Mutate_PreservesGeneCount()
    {
        // Arrange
        var original = new CodeFixChromosome(new[]
        {
            new CodeFixGene("a", 0.5, true),
            new CodeFixGene("b", 0.5, false),
        });

        // Act
        var mutated = GeneticRoslynBridge.Mutate(original, mutationRate: 0.5, seed: 42);

        // Assert
        mutated.Genes.Should().HaveCount(2);
    }

    [Fact]
    public void GeneticRoslynBridge_Crossover_PreservesFixStrategyIds()
    {
        // Arrange
        var parent1 = new CodeFixChromosome(new[]
        {
            new CodeFixGene("a", 1.0, true),
            new CodeFixGene("b", 1.0, true),
        });
        var parent2 = new CodeFixChromosome(new[]
        {
            new CodeFixGene("a", 0.0, false),
            new CodeFixGene("b", 0.0, false),
        });

        // Act
        var (child1, child2) = GeneticRoslynBridge.Crossover(parent1, parent2, seed: 42);

        // Assert
        child1.Genes.Should().AllSatisfy(g =>
            (g.FixStrategyId == "a" || g.FixStrategyId == "b").Should().BeTrue());
    }

    [Fact]
    public void GeneticRoslynBridge_CreateInitialPopulation_AllChromosomesHaveZeroFitness()
    {
        // Act
        var population = GeneticRoslynBridge.CreateInitialPopulation(5, seed: 42);

        // Assert
        population.Should().AllSatisfy(c => c.Fitness.Should().Be(0));
    }

    [Fact]
    public void GeneticRoslynBridge_CreateInitialPopulation_WithOneChromosome_Succeeds()
    {
        // Act
        var population = GeneticRoslynBridge.CreateInitialPopulation(1, seed: 42);

        // Assert
        population.Should().HaveCount(1);
        population[0].Genes.Should().NotBeEmpty();
    }

    [Fact]
    public void CodeFixChromosome_GetActiveFixes_WithMixedStates_ReturnsOnlyActive()
    {
        // Arrange
        var genes = new[]
        {
            new CodeFixGene("a", 0.9, true),   // active
            new CodeFixGene("b", 0.05, true),  // below threshold
            new CodeFixGene("c", 0.5, false),  // disabled
            new CodeFixGene("d", 0.2, true),   // active
            new CodeFixGene("e", 0.0, true),   // zero strength
        };
        var chromosome = new CodeFixChromosome(genes);

        // Act
        var active = chromosome.GetActiveFixes();

        // Assert
        active.Should().HaveCount(2);
        active.Select(g => g.FixStrategyId).Should().BeEquivalentTo(new[] { "a", "d" });
    }
}
