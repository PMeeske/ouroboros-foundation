namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Delegate for grounded operations that bridge abstract atoms to executable code.
/// Grounded operations can query/modify the atom space and return computed results.
/// </summary>
/// <param name="space">The atom space context.</param>
/// <param name="args">The argument expression (children of the call expression).</param>
/// <returns>Enumerable of result atoms (may be empty, single, or multiple solutions).</returns>
public delegate IEnumerable<Atom> GroundedOperation(IAtomSpace space, Expression args);