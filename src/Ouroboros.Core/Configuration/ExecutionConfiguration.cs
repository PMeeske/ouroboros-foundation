namespace Ouroboros.Core.Configuration;

/// <summary>
/// Configuration for pipeline execution.
/// </summary>
public class ExecutionConfiguration
{
    /// <summary>
    /// Gets or sets maximum turns for iterative reasoning.
    /// </summary>
    public int MaxTurns { get; set; } = 5;

    /// <summary>
    /// Gets or sets maximum parallel tool executions.
    /// </summary>
    public int MaxParallelToolExecutions { get; set; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether enable detailed debugging output.
    /// </summary>
    public bool EnableDebugOutput { get; set; } = false;

    /// <summary>
    /// Gets or sets tool execution timeout in seconds.
    /// </summary>
    public int ToolExecutionTimeoutSeconds { get; set; } = 60;
}