namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Represents an S-expression (list of atoms) in the atom space.
/// Expressions can contain nested expressions, symbols, and variables.
/// Examples: (Human Socrates), (implies (Human $x) (Mortal $x)).
/// </summary>
/// <param name="Children">The ordered list of child atoms.</param>
public sealed record Expression(ImmutableList<Atom> Children) : Atom
{
    /// <inheritdoc/>
    public override string ToSExpr()
    {
        if (Children.Count == 0)
        {
            return "()";
        }

        var sb = new StringBuilder();
        sb.Append('(');
        for (var i = 0; i < Children.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(' ');
            }

            sb.Append(Children[i].ToSExpr());
        }

        sb.Append(')');
        return sb.ToString();
    }

    /// <inheritdoc/>
    public override bool ContainsVariables() => Children.Any(c => c.ContainsVariables());

    /// <summary>
    /// Gets the head (first element) of the expression, if any.
    /// </summary>
    /// <returns>The head atom, or None if the expression is empty.</returns>
    public Option<Atom> Head() => Children.Count > 0
        ? Option<Atom>.Some(Children[0])
        : Option<Atom>.None();

    /// <summary>
    /// Gets the tail (all elements except the first) of the expression.
    /// </summary>
    /// <returns>An immutable list of the tail atoms.</returns>
    public ImmutableList<Atom> Tail() => Children.Count > 0
        ? Children.RemoveAt(0)
        : ImmutableList<Atom>.Empty;

    /// <summary>
    /// Determines equality for expressions based on structural equality of children.
    /// Required for proper value-based equality with ImmutableList.
    /// </summary>
    /// <param name="other">The other expression to compare.</param>
    /// <returns>True if the expressions are structurally equal.</returns>
    public bool Equals(Expression? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (Children.Count != other.Children.Count)
        {
            return false;
        }

        for (var i = 0; i < Children.Count; i++)
        {
            if (!Children[i].Equals(other.Children[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var child in Children)
        {
            hash.Add(child);
        }

        return hash.ToHashCode();
    }
}