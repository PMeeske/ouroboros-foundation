namespace Ouroboros.Agent.MetaAI.Executive;

/// <summary>
/// Interface for cognitive flexibility and set-shifting.
/// Detects when current strategy is failing and triggers strategy switching.
/// Implements SCAMPER creative reasoning and conceptual blending.
/// </summary>
public interface ICognitiveFlexibility
{
    /// <summary>
    /// Evaluates whether the current strategy should be shifted based on recent outcomes.
    /// </summary>
    /// <param name="currentStrategy">The strategy currently in use.</param>
    /// <param name="recentOutcomes">Recent task outcomes for trend analysis.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Strategy shift recommendation.</returns>
    Task<Result<StrategyShiftResult, string>> EvaluateStrategyAsync(
        string currentStrategy, List<TaskOutcome> recentOutcomes, CancellationToken ct = default);

    /// <summary>
    /// Generates alternative strategies when the current one is failing.
    /// </summary>
    /// <param name="failedStrategy">The strategy that failed.</param>
    /// <param name="context">Current context and constraints.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of alternative strategy descriptions.</returns>
    Task<Result<List<string>, string>> GenerateAlternativeStrategiesAsync(
        string failedStrategy, string context, CancellationToken ct = default);

    /// <summary>
    /// Estimates the cognitive cost of switching between tasks.
    /// </summary>
    /// <param name="fromTask">The task being switched from.</param>
    /// <param name="toTask">The task being switched to.</param>
    /// <returns>Estimated switch cost (0.0 = trivial, 1.0 = very costly).</returns>
    double EstimateTaskSwitchCost(string fromTask, string toTask);

    /// <summary>
    /// Records the outcome of a strategy for future evaluation.
    /// </summary>
    /// <param name="strategy">The strategy used.</param>
    /// <param name="success">Whether the strategy succeeded.</param>
    /// <param name="quality">Quality of the outcome (0.0 to 1.0).</param>
    void RecordStrategyOutcome(string strategy, bool success, double quality);

    /// <summary>
    /// Gets flexibility statistics.
    /// </summary>
    /// <returns>Flexibility statistics.</returns>
    FlexibilityStats GetStats();
}

/// <summary>
/// Result of a strategy shift evaluation.
/// </summary>
/// <param name="ShouldShift">Whether a strategy shift is recommended.</param>
/// <param name="RecommendedStrategy">The recommended new strategy.</param>
/// <param name="ConfidenceInShift">Confidence in the shift recommendation (0.0 to 1.0).</param>
/// <param name="Reason">Human-readable reason for the recommendation.</param>
/// <param name="EstimatedSwitchCost">Estimated cost of switching (0.0 to 1.0).</param>
public sealed record StrategyShiftResult(
    bool ShouldShift, string RecommendedStrategy, double ConfidenceInShift,
    string Reason, double EstimatedSwitchCost);

/// <summary>
/// Represents the outcome of a task execution.
/// </summary>
/// <param name="TaskId">Unique task identifier.</param>
/// <param name="Success">Whether the task succeeded.</param>
/// <param name="Quality">Quality of the outcome (0.0 to 1.0).</param>
/// <param name="Timestamp">When the outcome was recorded.</param>
public sealed record TaskOutcome(string TaskId, bool Success, double Quality, DateTime Timestamp);

/// <summary>
/// Statistics for cognitive flexibility.
/// </summary>
/// <param name="TotalShifts">Total number of strategy shifts performed.</param>
/// <param name="SuccessfulShifts">Number of shifts that led to improved outcomes.</param>
/// <param name="AverageShiftCost">Average cognitive cost of shifts.</param>
/// <param name="ConsecutiveFailuresBeforeShift">Average consecutive failures before a shift is triggered.</param>
public sealed record FlexibilityStats(
    int TotalShifts, int SuccessfulShifts, double AverageShiftCost,
    int ConsecutiveFailuresBeforeShift);
