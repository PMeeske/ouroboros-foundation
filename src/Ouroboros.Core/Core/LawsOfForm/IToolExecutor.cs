namespace Ouroboros.Core.LawsOfForm;

/// <summary>
/// Interface for tool execution.
/// Abstracts the tool interface to avoid circular dependencies.
/// </summary>
public interface IToolExecutor
{
    /// <summary>
    /// Executes the tool with the given input.
    /// </summary>
    /// <param name="input">The input for the tool.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Result containing the tool output or error.</returns>
    Task<Result<string, string>> InvokeAsync(string input, CancellationToken ct = default);
}