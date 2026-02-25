// <copyright file="ContextualStepExtensions.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Steps;

/// <summary>
/// Extension methods for composing and transforming <see cref="ContextualStep{TIn, TOut, TContext}"/> instances.
/// </summary>
public static class ContextualStepExtensions
{
    /// <summary>
    /// Composes two contextual steps in sequence (Kleisli composition).
    /// Logs from both steps are accumulated in order.
    /// </summary>
    /// <typeparam name="TIn">The input type of the first step.</typeparam>
    /// <typeparam name="TMid">The intermediate type connecting the two steps.</typeparam>
    /// <typeparam name="TOut">The output type of the second step.</typeparam>
    /// <typeparam name="TContext">The shared context type.</typeparam>
    /// <param name="first">The first contextual step to execute.</param>
    /// <param name="second">The second contextual step to execute with the first step's output.</param>
    /// <returns>A contextual step that executes both steps and combines their log entries.</returns>
    public static ContextualStep<TIn, TOut, TContext> Then<TIn, TMid, TOut, TContext>(
        this ContextualStep<TIn, TMid, TContext> first,
        ContextualStep<TMid, TOut, TContext> second)
        => async (input, context) =>
        {
            (TMid midResult, List<string> firstLogs) = await first(input, context).ConfigureAwait(false);
            (TOut finalResult, List<string> secondLogs) = await second(midResult, context).ConfigureAwait(false);

            List<string> combinedLogs = new List<string>(firstLogs.Count + secondLogs.Count);
            combinedLogs.AddRange(firstLogs);
            combinedLogs.AddRange(secondLogs);

            return (finalResult, combinedLogs);
        };

    /// <summary>
    /// Maps the output of a contextual step using a synchronous transformation while preserving context and logs.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TMid">The output type of the source step.</typeparam>
    /// <typeparam name="TOut">The output type after applying <paramref name="mapper"/>.</typeparam>
    /// <typeparam name="TContext">The context type.</typeparam>
    /// <param name="step">The contextual step whose output is mapped.</param>
    /// <param name="mapper">The synchronous transformation to apply.</param>
    /// <param name="log">An optional log entry to append after the mapping.</param>
    /// <returns>A contextual step whose output is the mapped value.</returns>
    public static ContextualStep<TIn, TOut, TContext> Map<TIn, TMid, TOut, TContext>(
        this ContextualStep<TIn, TMid, TContext> step,
        Func<TMid, TOut> mapper,
        string? log = null)
        => async (input, context) =>
        {
            (TMid midResult, List<string> logs) = await step(input, context).ConfigureAwait(false);
            TOut finalResult = mapper(midResult);

            if (log != null)
            {
                logs.Add(log);
            }

            return (finalResult, logs);
        };

    /// <summary>
    /// Appends a static log message to a contextual step's output.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <typeparam name="TContext">The context type.</typeparam>
    /// <param name="step">The contextual step to decorate.</param>
    /// <param name="logMessage">The message to append to the log on every invocation.</param>
    /// <returns>A contextual step that appends <paramref name="logMessage"/> after each execution.</returns>
    public static ContextualStep<TIn, TOut, TContext> WithLog<TIn, TOut, TContext>(
        this ContextualStep<TIn, TOut, TContext> step,
        string logMessage)
        => async (input, context) =>
        {
            (TOut result, List<string> logs) = await step(input, context).ConfigureAwait(false);
            logs.Add(logMessage);
            return (result, logs);
        };

    /// <summary>
    /// Appends a conditional log message derived from the step's output.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <typeparam name="TContext">The context type.</typeparam>
    /// <param name="step">The contextual step to decorate.</param>
    /// <param name="logFunction">A function that maps the output to a log message, or <see langword="null"/> to suppress logging.</param>
    /// <returns>A contextual step that conditionally appends a log entry after each execution.</returns>
    public static ContextualStep<TIn, TOut, TContext> WithConditionalLog<TIn, TOut, TContext>(
        this ContextualStep<TIn, TOut, TContext> step,
        Func<TOut, string?> logFunction)
        => async (input, context) =>
        {
            (TOut result, List<string> logs) = await step(input, context).ConfigureAwait(false);
            string? conditionalLog = logFunction(result);
            if (conditionalLog != null)
            {
                logs.Add(conditionalLog);
            }

            return (result, logs);
        };

    /// <summary>
    /// Collapses a contextual step to a pure step by binding the context, retaining the log output.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <typeparam name="TContext">The context type.</typeparam>
    /// <param name="step">The contextual step to collapse.</param>
    /// <param name="context">The context value to bind.</param>
    /// <returns>A pure step that returns the result and logs.</returns>
    public static Step<TIn, (TOut result, List<string> logs)> Forget<TIn, TOut, TContext>(
        this ContextualStep<TIn, TOut, TContext> step,
        TContext context)
        => input => step(input, context);

    /// <summary>
    /// Collapses a contextual step to a pure step, discarding both the context and the logs.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <typeparam name="TContext">The context type.</typeparam>
    /// <param name="step">The contextual step to collapse.</param>
    /// <param name="context">The context value to bind.</param>
    /// <returns>A pure step that returns only the result.</returns>
    public static Step<TIn, TOut> ForgetAll<TIn, TOut, TContext>(
        this ContextualStep<TIn, TOut, TContext> step,
        TContext context)
        => async input =>
        {
            (TOut result, _) = await step(input, context).ConfigureAwait(false);
            return result;
        };

    /// <summary>
    /// Wraps a contextual step so that exceptions are caught and returned as a
    /// <see cref="Result{TOut, Exception}"/> failure, with the error message appended to the logs.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <typeparam name="TContext">The context type.</typeparam>
    /// <param name="step">The contextual step to wrap.</param>
    /// <returns>A contextual step that never throws; all errors are captured in the result.</returns>
    public static ContextualStep<TIn, Result<TOut, Exception>, TContext> TryStep<TIn, TOut, TContext>(
        this ContextualStep<TIn, TOut, TContext> step)
        => async (input, context) =>
        {
            try
            {
                (TOut result, List<string> logs) = await step(input, context).ConfigureAwait(false);
                return (Result<TOut, Exception>.Success(result), logs);
            }
            catch (Exception ex)
            {
                return (Result<TOut, Exception>.Failure(ex), new List<string> { $"Error: {ex.Message}" });
            }
        };

    /// <summary>
    /// Wraps a contextual step so that the output is filtered through <paramref name="predicate"/>,
    /// returning <see cref="Option{TOut}.None"/> when the predicate fails or an exception is thrown.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <typeparam name="TContext">The context type.</typeparam>
    /// <param name="step">The contextual step to wrap.</param>
    /// <param name="predicate">A function that determines whether the output should be present.</param>
    /// <returns>A contextual step that returns an <see cref="Option{TOut}"/>.</returns>
    public static ContextualStep<TIn, Option<TOut>, TContext> TryOption<TIn, TOut, TContext>(
        this ContextualStep<TIn, TOut, TContext> step,
        Func<TOut, bool> predicate)
        => async (input, context) =>
        {
            try
            {
                (TOut result, List<string> logs) = await step(input, context).ConfigureAwait(false);
                Option<TOut> option = predicate(result) ? Option<TOut>.Some(result) : Option<TOut>.None();
                return (option, logs);
            }
            catch (Exception ex)
            {
                return (Option<TOut>.None(), new List<string> { $"Exception converted to None: {ex.Message}" });
            }
        };
}
