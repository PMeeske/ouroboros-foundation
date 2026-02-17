namespace Ouroboros.Agent;

/// <summary>
/// Result of an orchestrator execution with typed output.
/// </summary>
public sealed record OrchestratorResult<T>(
    T? Output,
    bool Success,
    string? ErrorMessage,
    OrchestratorMetrics Metrics,
    TimeSpan ExecutionTime,
    Dictionary<string, object> Metadata)
{
    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static OrchestratorResult<T> Ok(
        T output,
        OrchestratorMetrics metrics,
        TimeSpan executionTime,
        Dictionary<string, object>? metadata = null) =>
        new OrchestratorResult<T>(
            output,
            Success: true,
            ErrorMessage: null,
            metrics,
            executionTime,
            metadata ?? new Dictionary<string, object>());

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static OrchestratorResult<T> Failure(
        string errorMessage,
        OrchestratorMetrics metrics,
        TimeSpan executionTime,
        Dictionary<string, object>? metadata = null) =>
        new OrchestratorResult<T>(
            Output: default,
            Success: false,
            errorMessage,
            metrics,
            executionTime,
            metadata ?? new Dictionary<string, object>());

    /// <summary>
    /// Converts to Result monad.
    /// </summary>
    public Result<T, string> ToResult() =>
        Success && Output != null
            ? Result<T, string>.Success(Output)
            : Result<T, string>.Failure(ErrorMessage ?? "Operation failed");

    /// <summary>
    /// Gets a metadata value or returns default.
    /// </summary>
    public TValue? GetMetadata<TValue>(string key, TValue? defaultValue = default) =>
        Metadata.TryGetValue(key, out var value) && value is TValue typed
            ? typed
            : defaultValue;
}