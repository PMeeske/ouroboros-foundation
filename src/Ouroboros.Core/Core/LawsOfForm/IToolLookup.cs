namespace Ouroboros.Core.LawsOfForm;

/// <summary>
/// Interface for tool lookup operations.
/// Abstracts tool registry to avoid circular dependencies.
/// </summary>
public interface IToolLookup
{
    /// <summary>
    /// Gets a tool by name.
    /// </summary>
    /// <param name="toolName">The name of the tool.</param>
    /// <returns>An Option containing the tool if found.</returns>
    Option<IToolExecutor> GetTool(string toolName);
}