namespace Ouroboros.Core;

/// <summary>
/// Extension methods for working with disposable resources in pipelines.
/// </summary>
public static class DisposableExtensions
{
    /// <summary>
    /// Executes an action within a using scope, ensuring the resource is disposed.
    /// Returns a Result monad for error handling.
    /// </summary>
    /// <typeparam name="TResource">The disposable resource type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="resource">The resource to use.</param>
    /// <param name="action">The action to execute.</param>
    /// <returns>A Result containing the action result or an error.</returns>
    public static Result<TResult, Exception> Use<TResource, TResult>(
        this TResource resource,
        Func<TResource, TResult> action)
        where TResource : IDisposable
    {
        try
        {
            using (resource)
            {
                return Result<TResult, Exception>.Success(action(resource));
            }
        }
        catch (Exception ex)
        {
            return Result<TResult, Exception>.Failure(ex);
        }
    }

    /// <summary>
    /// Executes an async action within a using scope, ensuring the resource is disposed.
    /// Returns a Result monad for error handling.
    /// </summary>
    /// <typeparam name="TResource">The async disposable resource type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="resource">The resource to use.</param>
    /// <param name="action">The async action to execute.</param>
    /// <returns>A task with a Result containing the action result or an error.</returns>
    public static async Task<Result<TResult, Exception>> UseAsync<TResource, TResult>(
        this TResource resource,
        Func<TResource, Task<TResult>> action)
        where TResource : IAsyncDisposable
    {
        try
        {
            await using (resource.ConfigureAwait(false))
            {
                return Result<TResult, Exception>.Success(await action(resource).ConfigureAwait(false));
            }
        }
        catch (Exception ex)
        {
            return Result<TResult, Exception>.Failure(ex);
        }
    }
}