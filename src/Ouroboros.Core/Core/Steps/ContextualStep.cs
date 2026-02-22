// <copyright file="ContextualStep.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Steps;

/// <summary>
/// A contextual step that depends on a context and produces logs (Reader + Writer pattern).
/// </summary>
/// <typeparam name="TIn">The input type.</typeparam>
/// <typeparam name="TOut">The output type.</typeparam>
/// <typeparam name="TContext">The context type.</typeparam>
/// <param name="input">The input value.</param>
/// <param name="context">The context value.</param>
/// <returns>A task that resolves to the output value paired with accumulated log entries.</returns>
public delegate Task<(TOut result, List<string> logs)> ContextualStep<in TIn, TOut, in TContext>(TIn input, TContext context);

/// <summary>
/// Static factory methods for creating contextual steps.
/// </summary>
public static class ContextualStep
{
    /// <summary>
    /// Lifts a pure synchronous function into a contextual step.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <typeparam name="TContext">The context type (unused but required by the delegate signature).</typeparam>
    /// <param name="func">The pure function to lift.</param>
    /// <param name="log">An optional log entry to emit on each invocation.</param>
    /// <returns>A contextual step that applies <paramref name="func"/> and emits <paramref name="log"/>.</returns>
    public static ContextualStep<TIn, TOut, TContext> LiftPure<TIn, TOut, TContext>(
        Func<TIn, TOut> func,
        string? log = null)
    {
        ArgumentNullException.ThrowIfNull(func);
        return (input, _) => Task.FromResult((func(input), log != null ? new List<string> { log } : new List<string>()));
    }

    /// <summary>
    /// Creates a contextual step from an existing pure async step.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <typeparam name="TContext">The context type (unused but required by the delegate signature).</typeparam>
    /// <param name="step">The pure async step to wrap.</param>
    /// <param name="log">An optional log entry to emit on each invocation.</param>
    /// <returns>A contextual step that delegates to <paramref name="step"/> and emits <paramref name="log"/>.</returns>
    public static ContextualStep<TIn, TOut, TContext> FromPure<TIn, TOut, TContext>(
        Step<TIn, TOut> step,
        string? log = null)
    {
        ArgumentNullException.ThrowIfNull(step);
        return async (input, _) =>
        {
            TOut result = await step(input).ConfigureAwait(false);
            return (result, log != null ? new List<string> { log } : new List<string>());
        };
    }

    /// <summary>
    /// Creates a contextual step from a context-dependent step factory.
    /// The context is used to produce an inner step, which is then applied to the input.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <typeparam name="TContext">The context type.</typeparam>
    /// <param name="contextStep">A step that resolves a context into an inner step.</param>
    /// <returns>A contextual step that resolves the context at execution time.</returns>
    public static ContextualStep<TIn, TOut, TContext> FromContext<TIn, TOut, TContext>(
        Step<TContext, Step<TIn, TOut>> contextStep)
    {
        ArgumentNullException.ThrowIfNull(contextStep);
        return async (input, context) =>
        {
            Step<TIn, TOut> innerStep = await contextStep(context).ConfigureAwait(false);
            TOut result = await innerStep(input).ConfigureAwait(false);
            return (result, new List<string>());
        };
    }

    /// <summary>
    /// Creates a contextual identity step that returns the input unchanged.
    /// </summary>
    /// <typeparam name="TIn">The input and output type.</typeparam>
    /// <typeparam name="TContext">The context type (unused).</typeparam>
    /// <param name="log">An optional log entry to emit on each invocation.</param>
    /// <returns>A contextual step that passes the input through unmodified.</returns>
    public static ContextualStep<TIn, TIn, TContext> Identity<TIn, TContext>(string? log = null)
        => LiftPure<TIn, TIn, TContext>(x => x, log);
}
