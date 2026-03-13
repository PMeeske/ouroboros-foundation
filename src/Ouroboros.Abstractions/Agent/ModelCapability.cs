using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent;

/// <summary>
/// Represents metadata about a model's capabilities and performance characteristics.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record ModelCapability(
    string ModelName,
    string[] Strengths,
    int MaxTokens,
    double AverageCost,
    double AverageLatencyMs,
    ModelType Type);
