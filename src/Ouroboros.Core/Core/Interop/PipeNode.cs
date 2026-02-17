namespace Ouroboros.Core.Interop;

/// <summary>
/// Enhanced pipe node wrapper with monadic integration
/// </summary>
public readonly struct PipeNode<TIn, TOut>
{
    public readonly ICompatNode<TIn, TOut> Node;

    public PipeNode(ICompatNode<TIn, TOut> node)
    {
        Node = node;
    }

    /// <summary>
    /// Pipe execution operator: value -> pipeline result
    /// Note: Can't be async, so returns Task directly
    /// </summary>
    public static Task<TOut> operator |(TIn value, PipeNode<TIn, TOut> node)
        => node.Node.InvokeAsync(value);

    /// <summary>
    /// Pipe composition with method syntax for better type safety
    /// </summary>
    public PipeNode<TIn, TNext> Pipe<TNext>(PipeNode<TOut, TNext> next)
    {
        ICompatNode<TIn, TOut> currentNode = Node; // Capture to avoid struct 'this' issues
        return new PipeNode<TIn, TNext>(new LambdaNode<TIn, TNext>(
            $"{currentNode.Name} | {next.Node.Name}",
            async (input, ct) =>
            {
                TOut? mid = await currentNode.InvokeAsync(input, ct).ConfigureAwait(false);
                return await next.Node.InvokeAsync(mid, ct).ConfigureAwait(false);
            }));
    }

    /// <summary>
    /// Pipe composition with Step using method syntax
    /// </summary>
    public PipeNode<TIn, TNext> Pipe<TNext>(Step<TOut, TNext> step, string? stepName = null)
        => Pipe(step.ToCompatNode(stepName));

    /// <summary>
    /// Convert to Kleisli Step for monadic operations
    /// </summary>
    public Step<TIn, TOut> ToStep()
    {
        ICompatNode<TIn, TOut> currentNode = Node; // Capture to avoid struct 'this' issues
        return async input => await currentNode.InvokeAsync(input).ConfigureAwait(false);
    }

    /// <summary>
    /// Convert to KleisliResult with exception handling
    /// </summary>
    public KleisliResult<TIn, TOut, Exception> ToKleisliResult()
    {
        ICompatNode<TIn, TOut> currentNode = Node; // Capture to avoid struct 'this' issues
        return async input =>
        {
            try
            {
                TOut? result = await currentNode.InvokeAsync(input).ConfigureAwait(false);
                return Result<TOut, Exception>.Success(result);
            }
            catch (Exception ex)
            {
                return Result<TOut, Exception>.Failure(ex);
            }
        };
    }

    /// <inheritdoc/>
    public override string ToString() => Node?.Name ?? "EmptyPipeNode";
}