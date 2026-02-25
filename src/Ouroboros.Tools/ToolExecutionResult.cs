namespace Ouroboros.Tools;

/// <summary>
/// Result of tool execution.
/// </summary>
public class ToolExecutionResult
{
    public bool Success { get; }
    public string Result { get; }

    public ToolExecutionResult(bool success, string result)
    {
        Success = success;
        Result = result;
    }
}