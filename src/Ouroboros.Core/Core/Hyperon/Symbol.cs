namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Represents a symbol (named constant) in the atom space.
/// Symbols are ground terms with no unbound variables.
/// Examples: Human, Mortal, Socrates, implies.
/// </summary>
/// <param name="Name">The symbol's name.</param>
public sealed record Symbol(string Name) : Atom
{
    /// <inheritdoc/>
    public override string ToSExpr() => Name;

    /// <inheritdoc/>
    public override bool ContainsVariables() => false;
}