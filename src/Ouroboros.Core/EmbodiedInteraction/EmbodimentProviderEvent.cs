using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Event from an embodiment provider.
/// </summary>
/// <param name="EventType">Type of event.</param>
/// <param name="Timestamp">When the event occurred.</param>
/// <param name="Details">Event details.</param>
[ExcludeFromCodeCoverage]
public sealed record EmbodimentProviderEvent(
    EmbodimentProviderEventType EventType,
    DateTime Timestamp,
    IReadOnlyDictionary<string, object>? Details = null);
