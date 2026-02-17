namespace Ouroboros.Core.Interop;

/// <summary>
/// Compatible node interface for interop with various pipeline systems
/// </summary>
public interface ICompatNode<TIn, TOut>
{
    Task<TOut> InvokeAsync(TIn input, CancellationToken ct = default);
    string Name { get; }
}