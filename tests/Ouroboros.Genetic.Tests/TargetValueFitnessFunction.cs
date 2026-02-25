using Ouroboros.Genetic.Abstractions;

namespace Ouroboros.Tests.Genetic;

/// <summary>
/// Simple fitness function for testing that optimizes towards a target value.
/// </summary>
internal sealed class TargetValueFitnessFunction : IEvolutionFitnessFunction<SimpleChromosome>
{
    private readonly double targetValue;

    public TargetValueFitnessFunction(double targetValue)
    {
        this.targetValue = targetValue;
    }

    public Task<Result<double>> EvaluateAsync(SimpleChromosome chromosome)
    {
        // Fitness is inverse of distance from target (closer = better)
        var distance = Math.Abs(chromosome.Value - this.targetValue);
        var fitness = 1.0 / (1.0 + distance);
        return Task.FromResult(Result<double>.Success(fitness));
    }
}