// <copyright file="ContextualStep.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Steps;

/// <summary>
/// A contextual step that depends on context and produces logs (Reader + Writer pattern).
/// </summary>
/// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
public delegate Task<(TOut result, List<string> logs)> ContextualStep<in TIn, TOut, in TContext>(TIn input, TContext context);

/// <summary>
/// Static factory methods for contextual steps.
/// </summary>
public static class ContextualStep
{
    /// <summary>
    /// Lift a pure function into a contextual step.
    /// </summary>
    /// <returns></returns>
    public static ContextualStep<TIn, TOut, TContext> LiftPure<TIn, TOut, TContext>(
        Func<TIn, TOut> func,
        string? log = null)
        => async (input, context) =>
        {
            await Task.Yield();
            TOut? result = func(input);
            List<string> logs = log != null ? [log] : new List<string>();
            return (result, logs);
        };

    /// <summary>
    /// Create from pure Step.
    /// </summary>
    /// <returns></returns>
    public static ContextualStep<TIn, TOut, TContext> FromPure<TIn, TOut, TContext>(
        Step<TIn, TOut> step,
        string? log = null)
        => async (input, context) =>
        {
            TOut? result = await step(input);
            List<string> logs = log != null ? [log] : new List<string>();
            return (result, logs);
        };

    /// <summary>
    /// Create from context-dependent step factory.
    /// </summary>
    /// <returns></returns>
    public static ContextualStep<TIn, TOut, TContext> FromContext<TIn, TOut, TContext>(
        Step<TContext, Step<TIn, TOut>> contextStep)
        => async (input, context) =>
        {
            Step<TIn, TOut> innerStep = await contextStep(context);
            TOut? result = await innerStep(input);
            return (result, []);
        };

    /// <summary>
    /// Identity contextual step.
    /// </summary>
    /// <returns></returns>
    public static ContextualStep<TIn, TIn, TContext> Identity<TIn, TContext>(string? log = null)
        => LiftPure<TIn, TIn, TContext>(x => x, log);
}