namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// State of the embodiment aggregate.
/// </summary>
/// <param name="AggregateId">Aggregate identifier.</param>
/// <param name="Name">Aggregate name.</param>
/// <param name="Status">Current status.</param>
/// <param name="Capabilities">Aggregate capabilities.</param>
/// <param name="LastUpdatedAt">Last update timestamp.</param>
public sealed record EmbodimentAggregateState(
    string AggregateId,
    string Name,
    AggregateStatus Status,
    EmbodimentCapabilities Capabilities,
    DateTime LastUpdatedAt);