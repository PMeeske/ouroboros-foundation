namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Configuration for transfer learning behavior.
/// </summary>
public sealed record TransferLearningConfig(
    double MinTransferabilityThreshold = 0.5,
    int MaxAdaptationAttempts = 3,
    bool EnableAnalogicalReasoning = true,
    bool TrackTransferHistory = true);