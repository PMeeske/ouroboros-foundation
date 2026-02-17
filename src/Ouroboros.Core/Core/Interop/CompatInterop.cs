namespace Ouroboros.Core.Interop;

/// <summary>
/// Enhanced compatibility interop with monadic integration
/// </summary>
public static class CompatInterop
{
    /// <summary>
    /// Convert Step to compatible node
    /// </summary>
    public static PipeNode<TIn, TOut> ToCompatNode<TIn, TOut>(this Step<TIn, TOut> step, string? name = null)
        => new(new LambdaNode<TIn, TOut>(
            name ?? $"Step[{typeof(TIn).Name}->{typeof(TOut).Name}]",
            async (input, ct) => await step(input).ConfigureAwait(false)));

    /// <summary>
    /// Convert KleisliResult to compatible node
    /// </summary>
    public static PipeNode<TIn, Result<TOut, TError>> ToCompatNode<TIn, TOut, TError>(
        this KleisliResult<TIn, TOut, TError> kleisliResult,
        string? name = null)
        => new(new LambdaNode<TIn, Result<TOut, TError>>(
            name ?? $"KleisliResult[{typeof(TIn).Name}->{typeof(TOut).Name}]",
            async (input, ct) => await kleisliResult(input).ConfigureAwait(false)));

    /// <summary>
    /// Convert KleisliOption to compatible node
    /// </summary>
    public static PipeNode<TIn, Option<TOut>> ToCompatNode<TIn, TOut>(
        this KleisliOption<TIn, TOut> kleisliOption,
        string? name = null)
        => new(new LambdaNode<TIn, Option<TOut>>(
            name ?? $"KleisliOption[{typeof(TIn).Name}->{typeof(TOut).Name}]",
            async (input, ct) => await kleisliOption(input).ConfigureAwait(false)));

    /// <summary>
    /// Convert pure function to compatible node
    /// </summary>
    public static PipeNode<TIn, TOut> ToCompatNode<TIn, TOut>(this Func<TIn, TOut> f, string? name = null)
        => new(new LambdaNode<TIn, TOut>(
            name ?? $"Func[{typeof(TIn).Name}->{typeof(TOut).Name}]",
            (input, ct) => Task.FromResult(f(input))));

    /// <summary>
    /// Convert async function to compatible node
    /// </summary>
    public static PipeNode<TIn, TOut> ToCompatNode<TIn, TOut>(this Func<TIn, Task<TOut>> f, string? name = null)
        => new(new LambdaNode<TIn, TOut>(
            name ?? $"Async[{typeof(TIn).Name}->{typeof(TOut).Name}]",
            (input, ct) => f(input)));

    /// <summary>
    /// Create a pipeline builder for fluent composition
    /// </summary>
    public static PipelineBuilder<TIn> StartPipeline<TIn>(string name = "Pipeline")
        => new(name);
}