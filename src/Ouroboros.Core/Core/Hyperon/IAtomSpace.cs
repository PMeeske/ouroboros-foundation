namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Interface for an atom space - a storage for atoms supporting queries with unification.
/// </summary>
public interface IAtomSpace
{
    /// <summary>
    /// Adds an atom to the space.
    /// </summary>
    /// <param name="atom">The atom to add.</param>
    /// <returns>True if the atom was newly added, false if it already existed.</returns>
    bool Add(Atom atom);

    /// <summary>
    /// Adds multiple atoms to the space.
    /// </summary>
    /// <param name="atoms">The atoms to add.</param>
    /// <returns>The number of newly added atoms.</returns>
    int AddRange(IEnumerable<Atom> atoms);

    /// <summary>
    /// Removes an atom from the space.
    /// </summary>
    /// <param name="atom">The atom to remove.</param>
    /// <returns>True if the atom was removed, false if it didn't exist.</returns>
    bool Remove(Atom atom);

    /// <summary>
    /// Checks if the space contains the given atom (exact match).
    /// </summary>
    /// <param name="atom">The atom to check.</param>
    /// <returns>True if the atom exists in the space.</returns>
    bool Contains(Atom atom);

    /// <summary>
    /// Gets all atoms in the space.
    /// </summary>
    /// <returns>Enumerable of all atoms.</returns>
    IEnumerable<Atom> All();

    /// <summary>
    /// Queries the space for atoms matching the given pattern.
    /// Returns pairs of (matching atom, substitution that made the match).
    /// </summary>
    /// <param name="pattern">The pattern atom (may contain variables).</param>
    /// <returns>Enumerable of (matched atom, substitution) pairs.</returns>
    IEnumerable<(Atom Atom, Substitution Bindings)> Query(Atom pattern);

    /// <summary>
    /// Gets the count of atoms in the space.
    /// </summary>
    int Count { get; }
}