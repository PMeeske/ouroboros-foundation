namespace Ouroboros.Core.Steps;

/// <summary>
/// Interface representing a computation step that transforms input of type <typeparamref name="TIn"/>
/// to output of type <typeparamref name="TOut"/> with enhanced error handling capabilities.
/// </summary>
/// <remarks>
/// <para>
/// This interface provides a more robust alternative to the <see cref="Step{TIn, TOut}"/> delegate
/// by incorporating explicit error handling through the <see cref="TryExecuteAsync"/> method.
/// </para>
/// <para>
/// The covariant type parameters (<c>out TOut</c>) allow for more flexible type relationships
/// when working with step compositions and hierarchies.
/// </para>
/// </remarks>
/// <typeparam name="TIn">The contravariant input type.</typeparam>
/// <typeparam name="TOut">The output type.</typeparam>
public interface IStep<in TIn, TOut>
{
    /// <summary>
    /// Attempts to execute the step asynchronously with enhanced error handling.
    /// </summary>
    /// <param name="input">The input value to process.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> that completes with a <see cref="StepResult{TOut}"/> containing
    /// either the successful result or detailed error information.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method provides a non-throwing alternative to the standard execution pattern,
    /// allowing callers to handle errors without try-catch blocks. The method should not
    /// throw exceptions under normal operation - all errors should be captured in the
    /// returned <see cref="StepResult{TOut}"/>.
    /// </para>
    /// <para>
    /// Implementations should ensure thread safety and proper resource management.
    /// </para>
    /// </remarks>
    ValueTask<StepResult<TOut>> TryExecuteAsync(TIn input);

    /// <summary>
    /// Executes the step asynchronously with traditional exception throwing behavior.
    /// </summary>
    /// <param name="input">The input value to process.</param>
    /// <returns>A task that represents the asynchronous operation and contains the result.</returns>
    /// <exception cref="StepExecutionException">
    /// Thrown when the step execution fails. The exception contains detailed context
    /// about the failure including the step type and input value.
    /// </exception>
    /// <remarks>
    /// This method provides compatibility with existing code that expects exception-based
    /// error handling. For new code, consider using <see cref="TryExecuteAsync"/> for
    /// more granular error control.
    /// </remarks>
    async Task<TOut> ExecuteAsync(TIn input)
    {
        var result = await TryExecuteAsync(input).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            return result.Value!;
        }

        throw new StepExecutionException(
            GetType(),
            input,
            result.ErrorMessage ?? "Step execution failed",
            result.Exception);
    }
}