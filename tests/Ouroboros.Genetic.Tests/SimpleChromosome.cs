using Ouroboros.Genetic.Abstractions;

namespace Ouroboros.Tests.Genetic;

/// <summary>
/// Test implementation of IChromosome for unit testing.
/// Represents a simple numeric chromosome.
/// </summary>
internal sealed class SimpleChromosome : IChromosome
{
    public SimpleChromosome(double value, int generation = 0, double fitness = 0.0)
    {
        this.Id = Guid.NewGuid().ToString();
        this.Value = value;
        this.Generation = generation;
        this.Fitness = fitness;
    }

    private SimpleChromosome(string id, double value, int generation, double fitness)
    {
        this.Id = id;
        this.Value = value;
        this.Generation = generation;
        this.Fitness = fitness;
    }

    public string Id { get; }

    public double Value { get; }

    public int Generation { get; }

    public double Fitness { get; }

    public IChromosome Clone()
    {
        return new SimpleChromosome(this.Id, this.Value, this.Generation, this.Fitness);
    }

    public IChromosome WithFitness(double fitness)
    {
        return new SimpleChromosome(this.Id, this.Value, this.Generation, fitness);
    }

    public SimpleChromosome WithValue(double value)
    {
        return new SimpleChromosome(this.Id, value, this.Generation, this.Fitness);
    }

    public SimpleChromosome WithGeneration(int generation)
    {
        return new SimpleChromosome(this.Id, this.Value, generation, this.Fitness);
    }
}