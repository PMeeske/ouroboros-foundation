// <copyright file="IGeneticAlgorithm.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Genetic.Abstractions;

/// <summary>
/// Represents a genetic algorithm for evolutionary optimization.
/// </summary>
/// <remarks>
/// Deprecated: This interface is part of the original Genetic API.
/// New code should use <see cref="IEvolutionEngine{TChromosome}"/> instead,
/// which provides monadic <c>Result</c>-based error handling, <c>Option</c>-based
/// best-chromosome retrieval, and works with the non-generic <see cref="IChromosome"/> base.
/// Existing consumers in Engine and App layers still depend on this interface;
/// it will be removed in a future major version once all consumers are migrated.
/// </remarks>
/// <typeparam name="TGene">The type of gene in the chromosomes.</typeparam>
[Obsolete("Use IEvolutionEngine<TChromosome> instead. See Ouroboros.Genetic.Abstractions.IEvolutionEngine<TChromosome>.")]
public interface IGeneticAlgorithm<TGene>
{
    /// <summary>
    /// Evolves a population over multiple generations.
    /// </summary>
    /// <param name="initialPopulation">The initial population of chromosomes.</param>
    /// <param name="generations">The number of generations to evolve.</param>
    /// <returns>A task representing the best chromosome found.</returns>
    Task<Result<IChromosome<TGene>, string>> EvolveAsync(
        IReadOnlyList<IChromosome<TGene>> initialPopulation,
        int generations, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the fitness function used by this algorithm.
    /// </summary>
    IFitnessFunction<TGene> FitnessFunction { get; }
}
