namespace Ouroboros.Agent;

/// <summary>
/// Retry configuration for orchestrator operations.
/// </summary>
public sealed record RetryConfig(
    int MaxRetries = 3,
    TimeSpan InitialDelay = default,
    TimeSpan MaxDelay = default,
    double BackoffMultiplier = 2.0)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RetryConfig"/> class with defaults.
    /// </summary>
    public RetryConfig()
        : this(3, TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(10), 2.0)
    {
    }

    /// <summary>
    /// Creates a default retry configuration.
    /// </summary>
    public static RetryConfig Default() => new RetryConfig();
}