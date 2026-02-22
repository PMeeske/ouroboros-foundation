// <copyright file="ContextualStepDefinition.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Steps;

/// <summary>
/// Append-only builder for contextual pipelines:
/// steps that depend on a context (Reader) and may log/trace (Writer).
/// </summary>
/// <typeparam name="TIn">The input type.</typeparam>
/// <typeparam name="TOut">The output type.</typeparam>
/// <typeparam name="TContext">The context type.</typeparam>
public readonly struct ContextualStepDefinition<TIn, TOut, TContext>
{
    private readonly ContextualStep<TIn, TOut, TContext> _compiled;

    /// <summary>
    /// Initializes a new instance of <see cref="ContextualStepDefinition{TIn, TOut, TContext}"/>
    /// from a context-to-step factory.
    /// </summary>
    /// <param name="pure">A step that accepts a context and returns an inner step.</param>
    public ContextualStepDefinition(Step<TContext, Step<TIn, TOut>> pure)
    {
        ArgumentNullException.ThrowIfNull(pure);
        _compiled = ContextualStep.FromContext<TIn, TOut, TContext>(pure);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ContextualStepDefinition{TIn, TOut, TContext}"/>
    /// from a compiled contextual step.
    /// </summary>
    /// <param name="step">The compiled contextual step delegate.</param>
    public ContextualStepDefinition(ContextualStep<TIn, TOut, TContext> step)
    {
        ArgumentNullException.ThrowIfNull(step);
        _compiled = step;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ContextualStepDefinition{TIn, TOut, TContext}"/>
    /// from a pure synchronous function.
    /// </summary>
    /// <param name="func">The synchronous function to lift.</param>
    /// <param name="log">An optional log entry to emit on each invocation.</param>
    public ContextualStepDefinition(Func<TIn, TOut> func, string? log = null)
    {
        _compiled = ContextualStep.LiftPure<TIn, TOut, TContext>(func, log);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ContextualStepDefinition{TIn, TOut, TContext}"/>
    /// from a pure async step.
    /// </summary>
    /// <param name="pure">The pure async step to wrap.</param>
    /// <param name="log">An optional log entry to emit on each invocation.</param>
    public ContextualStepDefinition(Step<TIn, TOut> pure, string? log = null)
    {
        _compiled = ContextualStep.FromPure<TIn, TOut, TContext>(pure, log);
    }

    /// <summary>
    /// Creates a definition by lifting a pure synchronous function.
    /// </summary>
    /// <param name="func">The synchronous function to lift.</param>
    /// <param name="log">An optional log entry to emit on each invocation.</param>
    /// <returns>A new definition wrapping <paramref name="func"/>.</returns>
    public static ContextualStepDefinition<TIn, TOut, TContext> LiftPure(Func<TIn, TOut> func, string? log = null)
        => new(func, log);

    /// <summary>
    /// Creates a definition from a pure async step.
    /// </summary>
    /// <param name="step">The pure async step to wrap.</param>
    /// <param name="log">An optional log entry to emit on each invocation.</param>
    /// <returns>A new definition wrapping <paramref name="step"/>.</returns>
    public static ContextualStepDefinition<TIn, TOut, TContext> FromPure(Step<TIn, TOut> step, string? log = null)
        => new(step, log);

    /// <summary>
    /// Creates a definition from a context-dependent step factory.
    /// </summary>
    /// <param name="ctxStep">A step that accepts a context and returns an inner step.</param>
    /// <returns>A new definition that resolves its inner step from the context at execution time.</returns>
    public static ContextualStepDefinition<TIn, TOut, TContext> FromContext(Step<TContext, Step<TIn, TOut>> ctxStep)
        => new(ctxStep);

    /// <summary>
    /// Implicitly converts the definition to its compiled contextual step delegate.
    /// </summary>
    /// <param name="d">The definition to convert.</param>
    public static implicit operator ContextualStep<TIn, TOut, TContext>(ContextualStepDefinition<TIn, TOut, TContext> d)
        => d._compiled;

    /// <summary>
    /// Appends a contextual step, producing a new definition whose output flows through <paramref name="next"/>.
    /// </summary>
    /// <typeparam name="TNext">The output type of the appended step.</typeparam>
    /// <param name="next">The contextual step to append.</param>
    /// <returns>A new definition representing the composed pipeline.</returns>
    public ContextualStepDefinition<TIn, TNext, TContext> Pipe<TNext>(ContextualStep<TOut, TNext, TContext> next)
    {
        ContextualStep<TIn, TNext, TContext> newCompiled = _compiled.Then(next);
        return new ContextualStepDefinition<TIn, TNext, TContext>(newCompiled);
    }

    /// <summary>
    /// Appends a pure async step, producing a new definition whose output flows through <paramref name="pure"/>.
    /// </summary>
    /// <typeparam name="TNext">The output type of the appended step.</typeparam>
    /// <param name="pure">The pure async step to append.</param>
    /// <param name="log">An optional log entry to emit when <paramref name="pure"/> is invoked.</param>
    /// <returns>A new definition representing the composed pipeline.</returns>
    public ContextualStepDefinition<TIn, TNext, TContext> Pipe<TNext>(Step<TOut, TNext> pure, string? log = null)
    {
        ContextualStep<TOut, TNext, TContext> contextualNext = ContextualStep.FromPure<TOut, TNext, TContext>(pure, log);
        return Pipe(contextualNext);
    }

    /// <summary>
    /// Appends a pure synchronous function, producing a new definition whose output flows through <paramref name="func"/>.
    /// </summary>
    /// <typeparam name="TNext">The output type of the appended function.</typeparam>
    /// <param name="func">The synchronous function to append.</param>
    /// <param name="log">An optional log entry to emit when <paramref name="func"/> is invoked.</param>
    /// <returns>A new definition representing the composed pipeline.</returns>
    public ContextualStepDefinition<TIn, TNext, TContext> Pipe<TNext>(Func<TOut, TNext> func, string? log = null)
    {
        ContextualStep<TOut, TNext, TContext> contextualNext = ContextualStep.LiftPure<TOut, TNext, TContext>(func, log);
        return Pipe(contextualNext);
    }

    /// <summary>
    /// Executes the pipeline asynchronously with the given input and context.
    /// </summary>
    /// <param name="input">The input value.</param>
    /// <param name="context">The context value.</param>
    /// <returns>A task that resolves to the output value paired with accumulated log entries.</returns>
    public Task<(TOut result, List<string> logs)> InvokeAsync(TIn input, TContext context)
        => _compiled(input, context);

    /// <summary>
    /// Executes the pipeline synchronously by blocking the calling thread.
    /// </summary>
    /// <remarks>
    /// WARNING: This can cause deadlocks if called from a synchronization context.
    /// Prefer <see cref="InvokeAsync"/> throughout the call stack wherever possible.
    /// </remarks>
    /// <param name="input">The input value.</param>
    /// <param name="context">The context value.</param>
    /// <returns>The output value paired with accumulated log entries.</returns>
    public (TOut result, List<string> logs) Invoke(TIn input, TContext context)
        => Task.Run(() => _compiled(input, context)).GetAwaiter().GetResult();

    /// <summary>
    /// Collapses the definition to a pure step by binding the context, retaining the log output.
    /// </summary>
    /// <param name="context">The context to bind.</param>
    /// <returns>A pure step that returns the result and logs.</returns>
    public Step<TIn, (TOut result, List<string> logs)> Forget(TContext context)
        => _compiled.Forget(context);

    /// <summary>
    /// Collapses the definition to a pure step by discarding both the context and the logs.
    /// </summary>
    /// <param name="context">The context to bind.</param>
    /// <returns>A pure step that returns only the result.</returns>
    public Step<TIn, TOut> ForgetAll(TContext context)
        => _compiled.ForgetAll(context);

    /// <summary>
    /// Appends a static log message that is emitted after the current step completes.
    /// </summary>
    /// <param name="logMessage">The message to append to the log.</param>
    /// <returns>A new definition that appends <paramref name="logMessage"/> to every execution.</returns>
    public ContextualStepDefinition<TIn, TOut, TContext> WithLog(string logMessage)
    {
        ContextualStep<TIn, TOut, TContext> newCompiled = _compiled.WithLog(logMessage);
        return new ContextualStepDefinition<TIn, TOut, TContext>(newCompiled);
    }

    /// <summary>
    /// Appends a conditional log message derived from the step's output.
    /// </summary>
    /// <param name="logFunction">A function that maps the output to a log message, or <see langword="null"/> to suppress logging.</param>
    /// <returns>A new definition that conditionally appends a log entry after each execution.</returns>
    public ContextualStepDefinition<TIn, TOut, TContext> WithConditionalLog(Func<TOut, string?> logFunction)
    {
        ContextualStep<TIn, TOut, TContext> newCompiled = _compiled.WithConditionalLog(logFunction);
        return new ContextualStepDefinition<TIn, TOut, TContext>(newCompiled);
    }

    /// <summary>
    /// Wraps the definition so that exceptions are caught and returned as a <see cref="Result{TOut, Exception}"/> failure.
    /// </summary>
    /// <returns>A new definition that never throws; all errors are captured in the result.</returns>
    public ContextualStepDefinition<TIn, Result<TOut, Exception>, TContext> TryStep()
    {
        ContextualStep<TIn, Result<TOut, Exception>, TContext> newCompiled = _compiled.TryStep();
        return new ContextualStepDefinition<TIn, Result<TOut, Exception>, TContext>(newCompiled);
    }

    /// <summary>
    /// Wraps the definition so that the output is filtered through <paramref name="predicate"/>,
    /// returning <see cref="Option{TOut}.None"/> when the predicate fails or an exception is thrown.
    /// </summary>
    /// <param name="predicate">A function that determines whether the output should be present.</param>
    /// <returns>A new definition that returns an <see cref="Option{TOut}"/>.</returns>
    public ContextualStepDefinition<TIn, Option<TOut>, TContext> TryOption(Func<TOut, bool> predicate)
    {
        ContextualStep<TIn, Option<TOut>, TContext> newCompiled = _compiled.TryOption(predicate);
        return new ContextualStepDefinition<TIn, Option<TOut>, TContext>(newCompiled);
    }
}
