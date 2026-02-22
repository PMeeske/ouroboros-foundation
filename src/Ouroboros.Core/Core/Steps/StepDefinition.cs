// <copyright file="StepDefinition.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Steps;

/// <summary>
/// Minimal append-only builder for composing <see cref="Step{TIn, TOut}"/> instances
/// using the <c>|</c> pipeline syntax that the CLI DSL expects.
/// </summary>
/// <typeparam name="TIn">Input type.</typeparam>
/// <typeparam name="TOut">Output type.</typeparam>
public readonly struct StepDefinition<TIn, TOut>
{
    private readonly Step<TIn, TOut> _compiled;

    /// <summary>
    /// Initializes a new instance of the <see cref="StepDefinition{TIn, TOut}"/> struct
    /// from an asynchronous step.
    /// </summary>
    /// <param name="step">The async step to wrap.</param>
    public StepDefinition(Step<TIn, TOut> step)
    {
        ArgumentNullException.ThrowIfNull(step);
        _compiled = step;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StepDefinition{TIn, TOut}"/> struct
    /// from a synchronous function.
    /// </summary>
    /// <param name="func">The synchronous function to wrap.</param>
    public StepDefinition(Func<TIn, TOut> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        _compiled = input => Task.FromResult(func(input));
    }

    /// <summary>
    /// Composes with another step, returning a new definition representing the appended pipeline.
    /// </summary>
    /// <typeparam name="TNext">The output type of the composed pipeline.</typeparam>
    /// <param name="next">The step to append.</param>
    /// <returns>A new definition representing the composed pipeline.</returns>
    public StepDefinition<TIn, TNext> Pipe<TNext>(Step<TOut, TNext> next)
    {
        ArgumentNullException.ThrowIfNull(next);
        Step<TIn, TOut> current = _compiled;
        return new StepDefinition<TIn, TNext>(async input =>
        {
            TOut mid = await current(input).ConfigureAwait(false);
            return await next(mid).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Composes with a synchronous function.
    /// </summary>
    /// <typeparam name="TNext">The output type of the composed pipeline.</typeparam>
    /// <param name="func">The synchronous function to append.</param>
    /// <returns>A new definition representing the composed pipeline.</returns>
    public StepDefinition<TIn, TNext> Pipe<TNext>(Func<TOut, TNext> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        return Pipe<TNext>(value => Task.FromResult(func(value)));
    }

    /// <summary>
    /// Builds the composed step delegate.
    /// </summary>
    /// <returns>The underlying <see cref="Step{TIn, TOut}"/> delegate.</returns>
    public Step<TIn, TOut> Build() => _compiled;

    /// <summary>
    /// Operator syntax sugar to mirror the old DSL pipeline semantics.
    /// </summary>
    /// <param name="definition">The current definition.</param>
    /// <param name="next">The step to append.</param>
    /// <returns>A new definition with <paramref name="next"/> appended.</returns>
    public static StepDefinition<TIn, TOut> operator |(StepDefinition<TIn, TOut> definition, Step<TOut, TOut> next)
        => definition.Pipe(next);

    /// <summary>
    /// Operator overload for appending synchronous functions.
    /// </summary>
    /// <param name="definition">The current definition.</param>
    /// <param name="func">The synchronous function to append.</param>
    /// <returns>A new definition with <paramref name="func"/> appended.</returns>
    public static StepDefinition<TIn, TOut> operator |(StepDefinition<TIn, TOut> definition, Func<TOut, TOut> func)
        => definition.Pipe(func);
}
