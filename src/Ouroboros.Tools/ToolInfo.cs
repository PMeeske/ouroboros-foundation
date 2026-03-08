namespace Ouroboros.Tools;

/// <summary>
/// Information about a tool.
/// </summary>
public sealed class ToolInfo
{
    /// <summary>
    /// Gets the unique name of the tool.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a human-readable description of what the tool does.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the JSON schema object describing the tool's expected input parameters.
    /// </summary>
    public object InputSchema { get; }

    public ToolInfo(string name, string description, object inputSchema)
    {
        Name = name;
        Description = description;
        InputSchema = inputSchema;
    }
}