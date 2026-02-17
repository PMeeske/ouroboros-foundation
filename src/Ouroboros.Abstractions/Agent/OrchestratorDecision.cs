using Ouroboros.Abstractions.Core;

namespace Ouroboros.Agent;

/// <summary>
/// Result of orchestrator's model selection decision.
/// </summary>
public sealed record OrchestratorDecision(
    IChatCompletionModel SelectedModel,
    string ModelName,
    string Reason,
    ToolRegistry RecommendedTools,
    double ConfidenceScore);