namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Event arguments for meta-level reasoning events.
/// </summary>
public sealed class MetaReasoningEventArgs : EventArgs
{
    /// <summary>
    /// Gets the meta-level operation being performed.
    /// </summary>
    public required string Operation { get; init; }

    /// <summary>
    /// Gets the object-level expression being reasoned about.
    /// </summary>
    public required Atom ObjectLevel { get; init; }

    /// <summary>
    /// Gets the meta-level representation.
    /// </summary>
    public required Atom MetaLevel { get; init; }

    /// <summary>
    /// Gets any bindings discovered during meta-reasoning.
    /// </summary>
    public Substitution Bindings { get; init; } = Substitution.Empty;
}