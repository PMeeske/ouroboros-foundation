namespace Ouroboros.Tools;

/// <summary>
/// Information about a tool.
/// </summary>
public class ToolInfo
{
    public string Name { get; }
    public string Description { get; }
    public object InputSchema { get; }

    public ToolInfo(string name, string description, object inputSchema)
    {
        Name = name;
        Description = description;
        InputSchema = inputSchema;
    }
}