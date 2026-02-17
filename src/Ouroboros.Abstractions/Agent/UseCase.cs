namespace Ouroboros.Agent;

/// <summary>
/// Use case classification derived from prompt analysis.
/// </summary>
public sealed record UseCase(
    UseCaseType Type,
    int EstimatedComplexity,
    string[] RequiredCapabilities,
    double PerformanceWeight,
    double CostWeight);