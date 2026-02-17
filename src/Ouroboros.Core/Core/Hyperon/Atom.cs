// <copyright file="Atom.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Text;

namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Abstract base record for all atom types in the MeTTa-style knowledge representation.
/// Atoms are the fundamental building blocks: symbols, variables, and expressions (S-expressions).
/// Immutable by design, with value-based equality inherited from record semantics.
/// </summary>
public abstract record Atom
{
    /// <summary>
    /// Returns the S-expression string representation of this atom.
    /// </summary>
    /// <returns>S-expression formatted string.</returns>
    public abstract string ToSExpr();

    /// <inheritdoc/>
    public sealed override string ToString() => ToSExpr();

    /// <summary>
    /// Creates a Symbol atom with the given name.
    /// </summary>
    /// <param name="name">The symbol name.</param>
    /// <returns>A new Symbol atom.</returns>
    public static Symbol Sym(string name) => new(name);

    /// <summary>
    /// Creates a Variable atom with the given name (without the '$' prefix).
    /// </summary>
    /// <param name="name">The variable name (without '$' prefix).</param>
    /// <returns>A new Variable atom.</returns>
    public static Variable Var(string name) => new(name);

    /// <summary>
    /// Creates an Expression atom from the given children.
    /// </summary>
    /// <param name="children">The child atoms.</param>
    /// <returns>A new Expression atom.</returns>
    public static Expression Expr(params Atom[] children) => new(children.ToImmutableList());

    /// <summary>
    /// Creates an Expression atom from the given children.
    /// </summary>
    /// <param name="children">The child atoms.</param>
    /// <returns>A new Expression atom.</returns>
    public static Expression Expr(ImmutableList<Atom> children) => new(children);

    /// <summary>
    /// Creates an Expression atom from an enumerable of children.
    /// </summary>
    /// <param name="children">The child atoms.</param>
    /// <returns>A new Expression atom.</returns>
    public static Expression Expr(IEnumerable<Atom> children) => new(children.ToImmutableList());

    /// <summary>
    /// Determines if this atom contains any variables.
    /// </summary>
    /// <returns>True if the atom or any nested child contains variables.</returns>
    public virtual bool ContainsVariables() => false;
}