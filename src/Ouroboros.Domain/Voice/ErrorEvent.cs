namespace Ouroboros.Domain.Voice;

/// <summary>
/// Error event for stream error handling.
/// </summary>
public sealed record ErrorEvent : InteractionEvent
{
    /// <summary>Gets the error message.</summary>
    public required string Message { get; init; }

    /// <summary>Gets the exception if available.</summary>
    public Exception? Exception { get; init; }

    /// <summary>Gets the error category.</summary>
    public ErrorCategory Category { get; init; } = ErrorCategory.Unknown;

    /// <summary>Gets whether the error is recoverable.</summary>
    public bool IsRecoverable { get; init; } = true;
}