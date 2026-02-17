// <copyright file="AtomSpace.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Concurrent;

namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Thread-safe in-memory atom space implementation with indexing for efficient queries.
/// Supports pattern matching with unification.
/// </summary>
public sealed class AtomSpace : IAtomSpace
{
    private readonly ConcurrentDictionary<Atom, byte> atoms = new();

    // Index for expressions by their head symbol for faster lookups
    private readonly ConcurrentDictionary<string, ConcurrentBag<Expression>> symbolIndex = new();

    /// <inheritdoc/>
    public int Count => atoms.Count;

    /// <inheritdoc/>
    public bool Add(Atom atom)
    {
        if (!atoms.TryAdd(atom, 0))
        {
            return false;
        }

        // Index expressions by their head symbol
        if (atom is Expression expr && expr.Children.Count > 0 && expr.Children[0] is Symbol headSym)
        {
            var bag = symbolIndex.GetOrAdd(headSym.Name, _ => new ConcurrentBag<Expression>());
            bag.Add(expr);
        }

        return true;
    }

    /// <inheritdoc/>
    public int AddRange(IEnumerable<Atom> atomsToAdd)
    {
        var count = 0;
        foreach (var atom in atomsToAdd)
        {
            if (Add(atom))
            {
                count++;
            }
        }

        return count;
    }

    /// <inheritdoc/>
    public bool Remove(Atom atom)
    {
        // Note: We don't remove from the index for simplicity in concurrent scenarios
        // A more sophisticated implementation could use reference counting
        return atoms.TryRemove(atom, out _);
    }

    /// <inheritdoc/>
    public bool Contains(Atom atom) => atoms.ContainsKey(atom);

    /// <inheritdoc/>
    public IEnumerable<Atom> All() => atoms.Keys;

    /// <inheritdoc/>
    public IEnumerable<(Atom Atom, Substitution Bindings)> Query(Atom pattern)
    {
        // If pattern has no variables, just check for existence
        if (!pattern.ContainsVariables())
        {
            if (Contains(pattern))
            {
                yield return (pattern, Substitution.Empty);
            }

            yield break;
        }

        // Use index if pattern is an expression with a symbol head
        IEnumerable<Atom> candidates;
        if (pattern is Expression patternExpr && patternExpr.Children.Count > 0 && patternExpr.Children[0] is Symbol headSym)
        {
            // Look up indexed expressions with the same head
            candidates = symbolIndex.TryGetValue(headSym.Name, out var indexed)
                ? indexed.Cast<Atom>()
                : All();
        }
        else
        {
            candidates = All();
        }

        // Try to unify pattern with each candidate
        foreach (var candidate in candidates)
        {
            var unifyResult = Unifier.Unify(pattern, candidate);
            if (unifyResult is not null)
            {
                yield return (candidate, unifyResult);
            }
        }
    }

    /// <summary>
    /// Clears all atoms from the space.
    /// </summary>
    public void Clear()
    {
        atoms.Clear();
        symbolIndex.Clear();
    }

    /// <summary>
    /// Creates a snapshot of the current atom space as an immutable list.
    /// </summary>
    /// <returns>An immutable list of all atoms.</returns>
    public ImmutableList<Atom> Snapshot() => All().ToImmutableList();
}
