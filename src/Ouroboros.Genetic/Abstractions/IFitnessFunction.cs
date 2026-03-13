// <copyright file="IFitnessFunction.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Genetic.Abstractions;

/// <summary>
/// Evaluates the fitness of a chromosome for the genetic algorithm.
/// Fitness functions can evaluate based on test results, user satisfaction, or other metrics.
/// </summary>
/// <remarks>
/// Deprecated: This interface is part of the original Genetic API.
/// New code should implement <see cref="IEvolutionFitnessFunction{TChromosome}"/> instead,
/// which returns <c>Result&lt;double&gt;</c> for monadic error handling rather than raw doubles.
/// Existing consumers in Engine and App layers still depend on this interface;
/// it will be removed in a future major version once all consumers are migrated.
/// </remarks>
/// <typeparam name="TGene">The type of gene in the chromosomes being evaluated.</typeparam>
[Obsolete("Use IEvolutionFitnessFunction<TChromosome> instead. See Ouroboros.Genetic.Abstractions.IEvolutionFitnessFunction<TChromosome>.")]
public interface IFitnessFunction<TGene>
{
    /// <summary>
    /// Evaluates the fitness of a chromosome asynchronously.
    /// </summary>
    /// <param name="chromosome">The chromosome to evaluate.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the fitness score (higher is better).</returns>
    Task<double> EvaluateAsync(IChromosome<TGene> chromosome, CancellationToken cancellationToken);
}
