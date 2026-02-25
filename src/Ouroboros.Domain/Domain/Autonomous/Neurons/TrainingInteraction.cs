namespace Ouroboros.Domain.Autonomous.Neurons;

/// <summary>
/// Represents an interaction record for training analysis.
/// </summary>
public sealed record TrainingInteraction(
    Guid Id,
    string UserMessage,
    string SystemResponse,
    DateTime Timestamp,
    double? UserSatisfaction,
    List<string>? Feedback,
    Dictionary<string, object>? Metrics);