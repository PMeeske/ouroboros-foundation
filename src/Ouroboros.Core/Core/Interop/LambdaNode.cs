namespace Ouroboros.Core.Interop;

/// <summary>
/// Lambda-based compatible node implementation
/// </summary>
public sealed class LambdaNode<TIn, TOut> : ICompatNode<TIn, TOut>
{
    private readonly Func<TIn, CancellationToken, Task<TOut>> _fn;

    /// <inheritdoc/>
    public string Name { get; }

    /// <summary>Initialises the node with a display name and the delegate to execute.</summary>
    /// <param name="name">Display name for this node.</param>
    /// <param name="fn">The async function implementing the node logic.</param>
    public LambdaNode(string name, Func<TIn, CancellationToken, Task<TOut>> fn)
    {
        Name = name;
        _fn = fn;
    }

    /// <inheritdoc/>
    public Task<TOut> InvokeAsync(TIn input, CancellationToken ct = default) => _fn(input, ct);

    /// <inheritdoc/>
    public override string ToString() => Name;
}
