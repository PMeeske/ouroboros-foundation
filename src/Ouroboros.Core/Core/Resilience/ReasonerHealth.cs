namespace Ouroboros.Core.Resilience;

/// <summary>
/// Represents the health status of the reasoning system.
/// </summary>
/// <param name="CircuitState">Current state of the LLM circuit breaker (Closed/Open/HalfOpen).</param>
/// <param name="SymbolicAvailable">Whether symbolic reasoning is available.</param>
/// <param name="ConsecutiveLlmFailures">Number of consecutive LLM failures.</param>
/// <param name="LastLlmSuccess">Timestamp of last successful LLM operation, if any.</param>
public sealed record ReasonerHealth(
    string CircuitState,
    bool SymbolicAvailable,
    int ConsecutiveLlmFailures,
    DateTimeOffset? LastLlmSuccess);