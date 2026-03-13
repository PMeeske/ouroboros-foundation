using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Domain event from the embodiment aggregate.
/// </summary>
/// <param name="EventType">Type of domain event.</param>
/// <param name="Timestamp">When the event occurred.</param>
/// <param name="Details">Event details.</param>
[ExcludeFromCodeCoverage]
public sealed record EmbodimentDomainEvent(
    EmbodimentDomainEventType EventType,
    DateTime Timestamp,
    IReadOnlyDictionary<string, object>? Details = null);
