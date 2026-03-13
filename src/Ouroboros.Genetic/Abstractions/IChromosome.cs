// <copyright file="IChromosome.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Genetic.Abstractions;

/// <summary>
/// Represents a chromosome in a genetic algorithm, encoding a potential solution.
/// Chromosomes can represent pipeline configurations, parameters, or prompt variations.
/// </summary>
/// <remarks>
/// Deprecated: This generic gene-based interface is part of the original Genetic API.
/// New code should implement <see cref="Ouroboros.Genetic.Abstractions.IChromosome"/> (non-generic)
/// and use <see cref="Ouroboros.Genetic.Core.EvolutionEngine{TChromosome}"/> which provides
/// monadic Result-based error handling instead of exceptions.
/// Existing consumers in Engine and App layers still depend on this interface;
/// it will be removed in a future major version once all consumers are migrated.
/// </remarks>
/// <typeparam name="TGene">The type of gene in this chromosome.</typeparam>
[Obsolete("Use the non-generic IChromosome interface with EvolutionEngine<TChromosome> instead. See Ouroboros.Genetic.Abstractions.IChromosome.")]
public interface IChromosome<TGene>
{
    /// <summary>
    /// Gets the genes that make up this chromosome.
    /// </summary>
    IReadOnlyList<TGene> Genes { get; }

    /// <summary>
    /// Gets the fitness score of this chromosome.
    /// Higher values indicate better fitness.
    /// </summary>
    double Fitness { get; }

    /// <summary>
    /// Creates a new chromosome with the specified genes.
    /// </summary>
    /// <param name="genes">The genes for the new chromosome.</param>
    /// <returns>A new chromosome instance.</returns>
    IChromosome<TGene> WithGenes(IReadOnlyList<TGene> genes);

    /// <summary>
    /// Creates a new chromosome with the specified fitness score.
    /// </summary>
    /// <param name="fitness">The fitness score.</param>
    /// <returns>A new chromosome instance.</returns>
    IChromosome<TGene> WithFitness(double fitness);
}
