namespace Ouroboros.Agent;

/// <summary>
/// Configuration options for orchestrator behavior.
/// Provides a unified configuration pattern across all orchestrators.
/// </summary>
public record OrchestratorConfig
{
    /// <summary>
    /// Gets or initializes whether to enable distributed tracing.
    /// </summary>
    public bool EnableTracing { get; init; } = true;

    /// <summary>
    /// Gets or initializes whether to track performance metrics.
    /// </summary>
    public bool EnableMetrics { get; init; } = true;

    /// <summary>
    /// Gets or initializes whether to enable safety checks.
    /// </summary>
    public bool EnableSafetyChecks { get; init; } = true;

    /// <summary>
    /// Gets or initializes the maximum execution timeout.
    /// </summary>
    public TimeSpan? ExecutionTimeout { get; init; }

    /// <summary>
    /// Gets or initializes the retry configuration.
    /// </summary>
    public RetryConfig? RetryConfig { get; init; }

    /// <summary>
    /// Gets or initializes additional custom settings.
    /// </summary>
    public Dictionary<string, object> CustomSettings { get; init; } = new();

    /// <summary>
    /// Creates a default configuration.
    /// </summary>
    public static OrchestratorConfig Default() => new OrchestratorConfig();

    /// <summary>
    /// Gets a custom setting value or returns default.
    /// </summary>
    public T? GetSetting<T>(string key, T? defaultValue = default) =>
        CustomSettings.TryGetValue(key, out var value) && value is T typed
            ? typed
            : defaultValue;
}