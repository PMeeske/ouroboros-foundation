namespace Ouroboros.Core.Steps;

/// <summary>
/// Helper class for creating contextual step definitions
/// </summary>
public static class ContextualDef
{
    /// <summary>
    /// Create from pure function
    /// </summary>
    public static ContextualStepDefinition<TIn, TOut, TContext> LiftPure<TIn, TOut, TContext>(
        Func<TIn, TOut> func,
        string? log = null)
        => ContextualStepDefinition<TIn, TOut, TContext>.LiftPure(func, log);

    /// <summary>
    /// Create from pure Step
    /// </summary>
    public static ContextualStepDefinition<TIn, TOut, TContext> FromPure<TIn, TOut, TContext>(
        Step<TIn, TOut> step,
        string? log = null)
        => ContextualStepDefinition<TIn, TOut, TContext>.FromPure(step, log);

    /// <summary>
    /// Create from context-dependent step factory
    /// </summary>
    public static ContextualStepDefinition<TIn, TOut, TContext> FromContext<TIn, TOut, TContext>(
        Step<TContext, Step<TIn, TOut>> ctxStep)
        => ContextualStepDefinition<TIn, TOut, TContext>.FromContext(ctxStep);

    /// <summary>
    /// Identity contextual step
    /// </summary>
    public static ContextualStepDefinition<TIn, TIn, TContext> Identity<TIn, TContext>(string? log = null)
        => LiftPure<TIn, TIn, TContext>(x => x, log);
}