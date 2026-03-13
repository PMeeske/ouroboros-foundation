// <copyright file="GeneticRoslynBridge.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Genetic.Extensions;

using Ouroboros.Genetic.Abstractions;
using Ouroboros.Genetic.Core;

/// <summary>
/// Represents a gene encoding a Roslyn code transformation parameter.
/// Each gene specifies a fix strategy and its strength.
/// </summary>
/// <param name="FixStrategyId">Identifier for the Roslyn fix strategy (e.g., "add_null_check", "extract_method").</param>
/// <param name="Strength">How aggressively to apply the fix (0.0 to 1.0).</param>
/// <param name="Enabled">Whether this fix gene is active.</param>
public sealed record CodeFixGene(
    string FixStrategyId,
    double Strength,
    bool Enabled);

/// <summary>
/// Chromosome that encodes a set of Roslyn code transformations as genes.
/// Enables evolutionary optimization of code fix strategies.
/// </summary>
public sealed record CodeFixChromosome : IChromosome<CodeFixGene>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CodeFixChromosome"/> class.
    /// </summary>
    public CodeFixChromosome(IReadOnlyList<CodeFixGene> genes, double fitness = 0)
    {
        Genes = genes?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(genes));
        Fitness = fitness;
    }

    /// <inheritdoc/>
    public IReadOnlyList<CodeFixGene> Genes { get; init; }

    /// <inheritdoc/>
    public double Fitness { get; init; }

    /// <inheritdoc/>
    public IChromosome<CodeFixGene> WithGenes(IReadOnlyList<CodeFixGene> genes)
        => this with { Genes = genes };

    /// <inheritdoc/>
    public IChromosome<CodeFixGene> WithFitness(double fitness)
        => this with { Fitness = fitness };

    /// <summary>
    /// Gets only the active (enabled) fix genes.
    /// </summary>
    public IReadOnlyList<CodeFixGene> GetActiveFixes()
        => Genes.Where(g => g.Enabled && g.Strength > 0.1).ToList();
}

/// <summary>
/// Fitness function that evaluates code fix chromosomes.
/// Scores based on: compilation success, number of diagnostics fixed,
/// and code quality metrics.
/// </summary>
public sealed class CodeFixFitnessFunction : IFitnessFunction<CodeFixGene>
{
    private readonly Func<IReadOnlyList<CodeFixGene>, Task<CodeFixEvaluationResult>> _evaluator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeFixFitnessFunction"/> class.
    /// </summary>
    /// <param name="evaluator">
    /// Function that evaluates a set of code fix genes against the codebase.
    /// Returns a result containing compilation success and metrics.
    /// </param>
    public CodeFixFitnessFunction(
        Func<IReadOnlyList<CodeFixGene>, Task<CodeFixEvaluationResult>> evaluator)
    {
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
    }

    /// <inheritdoc/>
    public async Task<double> EvaluateAsync(
        IChromosome<CodeFixGene> chromosome,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(chromosome);

        var activeFixes = chromosome.Genes.Where(g => g.Enabled).ToList();

        if (activeFixes.Count == 0)
        {
            return 0.0;
        }

        var result = await _evaluator(activeFixes);

        // Score: compilation success is essential (0 or 0.5 base),
        // then bonus for diagnostics fixed and code quality
        double score = 0.0;

        if (result.CompilationSucceeded)
        {
            score += 0.5;
            score += 0.3 * Math.Min(1.0, result.DiagnosticsFixed / Math.Max(1.0, result.TotalDiagnostics));
            score += 0.2 * result.QualityImprovement;
        }

        return Math.Clamp(score, 0.0, 1.0);
    }
}

/// <summary>
/// Result of evaluating a set of code fixes against the codebase.
/// </summary>
/// <param name="CompilationSucceeded">Whether the code compiles after applying fixes.</param>
/// <param name="DiagnosticsFixed">Number of diagnostics resolved by the fixes.</param>
/// <param name="TotalDiagnostics">Total number of diagnostics before applying fixes.</param>
/// <param name="QualityImprovement">Normalized quality improvement score (0.0 to 1.0).</param>
public sealed record CodeFixEvaluationResult(
    bool CompilationSucceeded,
    int DiagnosticsFixed,
    int TotalDiagnostics,
    double QualityImprovement);

/// <summary>
/// Bridge connecting the genetic algorithm to the Roslynator code fix pipeline.
/// Enables evolutionary self-modification of code through genetic optimization
/// of Roslyn-based code transformations.
/// </summary>
public static class GeneticRoslynBridge
{
    private static readonly string[] DefaultFixStrategies = new[]
    {
        "add_null_check", "extract_method", "simplify_expression",
        "remove_unused_variable", "add_async_await", "fix_naming_convention",
        "add_using_directive", "replace_magic_number", "add_documentation",
        "fix_accessibility_modifier"
    };

    /// <summary>
    /// Creates a random initial population of code fix chromosomes.
    /// </summary>
    /// <param name="populationSize">Number of chromosomes in the population.</param>
    /// <param name="strategies">Available fix strategies. Uses defaults if null.</param>
    /// <param name="seed">Random seed for reproducibility.</param>
    /// <returns>A list of randomly initialized chromosomes.</returns>
    public static IReadOnlyList<CodeFixChromosome> CreateInitialPopulation(
        int populationSize,
        IReadOnlyList<string>? strategies = null,
        int seed = 42)
    {
        var rng = new Random(seed);
        strategies ??= DefaultFixStrategies;

        var population = new List<CodeFixChromosome>();

        for (int i = 0; i < populationSize; i++)
        {
            var genes = strategies.Select(s => new CodeFixGene(
                FixStrategyId: s,
                Strength: rng.NextDouble(),
                Enabled: rng.NextDouble() > 0.3 // 70% chance of being enabled
            )).ToList();

            population.Add(new CodeFixChromosome(genes));
        }

        return population;
    }

    /// <summary>
    /// Mutates a code fix chromosome by randomly toggling genes or adjusting strengths.
    /// </summary>
    /// <param name="chromosome">The chromosome to mutate.</param>
    /// <param name="mutationRate">Probability of each gene being mutated (0.0 to 1.0).</param>
    /// <param name="seed">Random seed.</param>
    /// <returns>A new mutated chromosome.</returns>
    public static CodeFixChromosome Mutate(
        CodeFixChromosome chromosome,
        double mutationRate = 0.1,
        int? seed = null)
    {
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();

        var mutatedGenes = chromosome.Genes.Select(gene =>
        {
            if (rng.NextDouble() < mutationRate)
            {
                // Randomly toggle enabled or adjust strength
                return rng.NextDouble() < 0.5
                    ? gene with { Enabled = !gene.Enabled }
                    : gene with { Strength = Math.Clamp(gene.Strength + (rng.NextDouble() - 0.5) * 0.2, 0.0, 1.0) };
            }
            return gene;
        }).ToList();

        return new CodeFixChromosome(mutatedGenes);
    }

    /// <summary>
    /// Performs uniform crossover between two parent chromosomes.
    /// </summary>
    /// <param name="parent1">First parent.</param>
    /// <param name="parent2">Second parent.</param>
    /// <param name="seed">Random seed.</param>
    /// <returns>Two offspring chromosomes.</returns>
    public static (CodeFixChromosome child1, CodeFixChromosome child2) Crossover(
        CodeFixChromosome parent1,
        CodeFixChromosome parent2,
        int? seed = null)
    {
        var rng = seed.HasValue ? new Random(seed.Value) : new Random();
        int length = Math.Min(parent1.Genes.Count, parent2.Genes.Count);

        var child1Genes = new List<CodeFixGene>();
        var child2Genes = new List<CodeFixGene>();

        for (int i = 0; i < length; i++)
        {
            if (rng.NextDouble() < 0.5)
            {
                child1Genes.Add(parent1.Genes[i]);
                child2Genes.Add(parent2.Genes[i]);
            }
            else
            {
                child1Genes.Add(parent2.Genes[i]);
                child2Genes.Add(parent1.Genes[i]);
            }
        }

        return (new CodeFixChromosome(child1Genes), new CodeFixChromosome(child2Genes));
    }
}
