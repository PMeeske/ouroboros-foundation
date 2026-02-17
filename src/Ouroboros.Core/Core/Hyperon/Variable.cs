namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Represents a variable (placeholder for pattern matching) in the atom space.
/// Variables are prefixed with '$' in MeTTa syntax.
/// Examples: $x, $y, $person.
/// </summary>
/// <param name="Name">The variable's name (without '$' prefix).</param>
public sealed record Variable(string Name) : Atom
{
    /// <inheritdoc/>
    public override string ToSExpr() => $"${Name}";

    /// <inheritdoc/>
    public override bool ContainsVariables() => true;
}