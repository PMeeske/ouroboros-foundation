using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.Interop;

/// <summary>
/// Compatible node interface for interop with various pipeline systems
/// </summary>
[ExcludeFromCodeCoverage]
public interface ICompatNode<TIn, TOut>
{
    /// <summary>Executes the node with the given input and returns the output asynchronously.</summary>
    /// <param name="input">The node input value.</param>
    /// <param name="ct">Optional cancellation token.</param>
    Task<TOut> InvokeAsync(TIn input, CancellationToken ct = default);

    /// <summary>Gets the display name of this node.</summary>
    string Name { get; }
}
