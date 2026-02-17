namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Statistics for the neural memory system.
/// </summary>
public sealed record QdrantNeuralMemoryStats
{
    public bool IsConnected { get; init; }
    public long NeuronMessagesCount { get; init; }
    public long IntentionsCount { get; init; }
    public long MemoriesCount { get; init; }
    public long TotalPoints { get; init; }
}