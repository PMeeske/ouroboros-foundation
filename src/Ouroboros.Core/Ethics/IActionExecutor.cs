namespace Ouroboros.Core.Ethics;

/// <summary>
/// Interface for action executors that can be wrapped with ethics enforcement.
/// </summary>
/// <typeparam name="TAction">The type of action to execute.</typeparam>
/// <typeparam name="TResult">The type of result produced.</typeparam>
public interface IActionExecutor<in TAction, TResult>
{
    /// <summary>
    /// Executes an action and returns the result.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of execution.</returns>
    Task<Result<TResult, string>> ExecuteAsync(TAction action, CancellationToken ct = default);
}