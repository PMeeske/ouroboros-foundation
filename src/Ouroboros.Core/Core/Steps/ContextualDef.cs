// <copyright file="ContextualDef.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Steps;

/// <summary>
/// Static helper class providing factory methods for creating <see cref="ContextualStepDefinition{TIn, TOut, TContext}"/> instances.
/// </summary>
public static class ContextualDef
{
    /// <summary>
    /// Creates a contextual step definition by lifting a pure synchronous function.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <typeparam name="TContext">The context type.</typeparam>
    /// <param name="func">The synchronous function to lift.</param>
    /// <param name="log">An optional log entry to emit on each invocation.</param>
    /// <returns>A contextual step definition wrapping <paramref name="func"/>.</returns>
    public static ContextualStepDefinition<TIn, TOut, TContext> LiftPure<TIn, TOut, TContext>(
        Func<TIn, TOut> func,
        string? log = null)
        => ContextualStepDefinition<TIn, TOut, TContext>.LiftPure(func, log);

    /// <summary>
    /// Creates a contextual step definition from an existing pure async step.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <typeparam name="TContext">The context type.</typeparam>
    /// <param name="step">The pure async step to wrap.</param>
    /// <param name="log">An optional log entry to emit on each invocation.</param>
    /// <returns>A contextual step definition wrapping <paramref name="step"/>.</returns>
    public static ContextualStepDefinition<TIn, TOut, TContext> FromPure<TIn, TOut, TContext>(
        Step<TIn, TOut> step,
        string? log = null)
        => ContextualStepDefinition<TIn, TOut, TContext>.FromPure(step, log);

    /// <summary>
    /// Creates a contextual step definition from a context-dependent step factory.
    /// </summary>
    /// <typeparam name="TIn">The input type.</typeparam>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <typeparam name="TContext">The context type.</typeparam>
    /// <param name="ctxStep">A step that accepts a context and returns an inner step.</param>
    /// <returns>A contextual step definition that resolves its inner step from the context at execution time.</returns>
    public static ContextualStepDefinition<TIn, TOut, TContext> FromContext<TIn, TOut, TContext>(
        Step<TContext, Step<TIn, TOut>> ctxStep)
        => ContextualStepDefinition<TIn, TOut, TContext>.FromContext(ctxStep);

    /// <summary>
    /// Creates a contextual step definition that acts as the identity: returns the input unchanged.
    /// </summary>
    /// <typeparam name="TIn">The input and output type.</typeparam>
    /// <typeparam name="TContext">The context type (unused).</typeparam>
    /// <param name="log">An optional log entry to emit on each invocation.</param>
    /// <returns>A contextual step definition that passes the input through unmodified.</returns>
    public static ContextualStepDefinition<TIn, TIn, TContext> Identity<TIn, TContext>(string? log = null)
        => LiftPure<TIn, TIn, TContext>(x => x, log);
}
