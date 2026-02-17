namespace Ouroboros.Agent.TemporalReasoning;

/// <summary>
/// Represents a predicted future event based on temporal patterns.
/// </summary>
public sealed record PredictedEvent(
    string EventType,
    string Description,
    DateTime PredictedTime,
    double Confidence,
    IReadOnlyList<TemporalEvent> BasedOnEvents,
    string ReasoningExplanation);