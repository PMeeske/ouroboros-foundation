namespace Ouroboros.Tools;

/// <summary>
/// Result of tool execution.
/// </summary>
public sealed class ToolExecutionResult
{
    /// <summary>
    /// Gets a value indicating whether the tool execution completed successfully.
    /// </summary>
    public bool Success { get; }

    /// <summary>
    /// Gets the output produced by the tool execution, or an error message on failure.
    /// </summary>
    public string Result { get; }

    public ToolExecutionResult(bool success, string result)
    {
        Success = success;
        Result = result;
    }
}