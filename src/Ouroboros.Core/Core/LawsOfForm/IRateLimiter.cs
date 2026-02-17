namespace Ouroboros.Core.LawsOfForm;

/// <summary>
/// Interface for rate limiting tool executions.
/// </summary>
public interface IRateLimiter
{
    /// <summary>
    /// Checks if a tool call is allowed under current rate limits.
    /// </summary>
    /// <param name="toolCall">The tool call to check.</param>
    /// <returns>True if allowed, false if rate limit exceeded.</returns>
    bool IsAllowed(ToolCall toolCall);

    /// <summary>
    /// Records a tool execution for rate limiting.
    /// </summary>
    /// <param name="toolCall">The tool call that was executed.</param>
    void Record(ToolCall toolCall);
}