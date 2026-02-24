// <copyright file="GeneticRoslynBridgeTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Genetic.Extensions;
using Xunit;

namespace Ouroboros.Genetic.Tests.Extensions;

/// <summary>
/// Tests for the Genetic-Roslynator bridge components.
/// </summary>
[Trait("Category", "Unit")]
public class GeneticRoslynBridgeTests
{
    [Fact]
    public void CodeFixChromosome_Constructor_StoresGenes()
    {
        var genes = new[]
        {
            new CodeFixGene("add_null_check", 0.8, true),
            new CodeFixGene("extract_method", 0.5, false)
        };

        var chromosome = new CodeFixChromosome(genes);

        chromosome.Genes.Should().HaveCount(2);
        chromosome.Fitness.Should().Be(0);
    }

    [Fact]
    public void CodeFixChromosome_WithFitness_ReturnsNewInstance()
    {
        var genes = new[] { new CodeFixGene("fix", 0.5, true) };
        var chromosome = new CodeFixChromosome(genes);

        var updated = chromosome.WithFitness(0.95);

        updated.Fitness.Should().Be(0.95);
        chromosome.Fitness.Should().Be(0, "original should be unchanged");
    }

    [Fact]
    public void CodeFixChromosome_GetActiveFixes_ReturnsOnlyEnabled()
    {
        var genes = new[]
        {
            new CodeFixGene("a", 0.8, true),
            new CodeFixGene("b", 0.5, false),
            new CodeFixGene("c", 0.05, true), // Below 0.1 threshold
            new CodeFixGene("d", 0.9, true)
        };

        var chromosome = new CodeFixChromosome(genes);
        var active = chromosome.GetActiveFixes();

        active.Should().HaveCount(2);
        active.Should().Contain(g => g.FixStrategyId == "a");
        active.Should().Contain(g => g.FixStrategyId == "d");
    }

    [Fact]
    public async Task CodeFixFitnessFunction_SuccessfulCompilation_ReturnsHighScore()
    {
        var fitness = new CodeFixFitnessFunction(genes =>
            Task.FromResult(new CodeFixEvaluationResult(
                CompilationSucceeded: true,
                DiagnosticsFixed: 5,
                TotalDiagnostics: 10,
                QualityImprovement: 0.8)));

        var chromosome = new CodeFixChromosome(new[]
        {
            new CodeFixGene("fix1", 0.8, true),
            new CodeFixGene("fix2", 0.6, true)
        });

        var score = await fitness.EvaluateAsync(chromosome, CancellationToken.None);

        score.Should().BeGreaterThan(0.5, "successful compilation should yield high score");
        score.Should().BeLessThanOrEqualTo(1.0);
    }

    [Fact]
    public async Task CodeFixFitnessFunction_FailedCompilation_ReturnsZero()
    {
        var fitness = new CodeFixFitnessFunction(genes =>
            Task.FromResult(new CodeFixEvaluationResult(
                CompilationSucceeded: false,
                DiagnosticsFixed: 0,
                TotalDiagnostics: 10,
                QualityImprovement: 0.0)));

        var chromosome = new CodeFixChromosome(new[]
        {
            new CodeFixGene("bad_fix", 0.8, true)
        });

        var score = await fitness.EvaluateAsync(chromosome, CancellationToken.None);

        score.Should().Be(0.0);
    }

    [Fact]
    public async Task CodeFixFitnessFunction_NoActiveGenes_ReturnsZero()
    {
        var fitness = new CodeFixFitnessFunction(genes =>
            Task.FromResult(new CodeFixEvaluationResult(true, 0, 0, 0.0)));

        var chromosome = new CodeFixChromosome(new[]
        {
            new CodeFixGene("fix", 0.8, false) // disabled
        });

        var score = await fitness.EvaluateAsync(chromosome, CancellationToken.None);

        score.Should().Be(0.0);
    }

    [Fact]
    public void CreateInitialPopulation_ReturnsCorrectSize()
    {
        var population = GeneticRoslynBridge.CreateInitialPopulation(20);

        population.Should().HaveCount(20);
        population.Should().AllSatisfy(c => c.Genes.Should().NotBeEmpty());
    }

    [Fact]
    public void CreateInitialPopulation_WithCustomStrategies_UsesProvided()
    {
        var strategies = new[] { "strat1", "strat2", "strat3" };

        var population = GeneticRoslynBridge.CreateInitialPopulation(5, strategies);

        population.Should().HaveCount(5);
        population.Should().AllSatisfy(c =>
            c.Genes.Should().HaveCount(3));
    }

    [Fact]
    public void Mutate_ProducesNewChromosome()
    {
        var original = new CodeFixChromosome(new[]
        {
            new CodeFixGene("a", 0.5, true),
            new CodeFixGene("b", 0.5, true),
            new CodeFixGene("c", 0.5, true),
            new CodeFixGene("d", 0.5, true),
            new CodeFixGene("e", 0.5, true)
        });

        // High mutation rate to ensure visible changes
        var mutated = GeneticRoslynBridge.Mutate(original, mutationRate: 1.0, seed: 42);

        mutated.Should().NotBeNull();
        mutated.Genes.Should().HaveCount(original.Genes.Count);
        // At least one gene should differ
        var differences = mutated.Genes
            .Zip(original.Genes, (m, o) => m != o)
            .Count(d => d);
        differences.Should().BeGreaterThan(0, "high mutation rate should produce changes");
    }

    [Fact]
    public void Crossover_ProducesTwoOffspring()
    {
        var parent1 = new CodeFixChromosome(new[]
        {
            new CodeFixGene("a", 1.0, true),
            new CodeFixGene("b", 1.0, true)
        });

        var parent2 = new CodeFixChromosome(new[]
        {
            new CodeFixGene("a", 0.0, false),
            new CodeFixGene("b", 0.0, false)
        });

        var (child1, child2) = GeneticRoslynBridge.Crossover(parent1, parent2, seed: 42);

        child1.Genes.Should().HaveCount(2);
        child2.Genes.Should().HaveCount(2);
        // Each gene in children should come from one of the parents
        for (int i = 0; i < 2; i++)
        {
            var c1Gene = child1.Genes[i];
            var c2Gene = child2.Genes[i];
            var p1Gene = parent1.Genes[i];
            var p2Gene = parent2.Genes[i];

            (c1Gene == p1Gene || c1Gene == p2Gene).Should().BeTrue();
            (c2Gene == p1Gene || c2Gene == p2Gene).Should().BeTrue();
        }
    }

    [Fact]
    public void CreateInitialPopulation_IsDeterministicWithSameSeed()
    {
        var pop1 = GeneticRoslynBridge.CreateInitialPopulation(10, seed: 123);
        var pop2 = GeneticRoslynBridge.CreateInitialPopulation(10, seed: 123);

        pop1.Should().BeEquivalentTo(pop2);
    }
}
