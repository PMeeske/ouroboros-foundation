namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Standard grounded operations for MeTTa-like semantics.
/// </summary>
public static class StandardOperations
{
    /// <summary>
    /// Registers all standard operations in the given registry.
    /// </summary>
    /// <param name="registry">The registry to populate.</param>
    public static void RegisterAll(GroundedRegistry registry)
    {
        registry.Register("implies", Implies);
        registry.Register("equal", Equal);
        registry.Register("not", Not);
        registry.Register("and", And);
        registry.Register("or", Or);
        registry.Register("assert", Assert);
        registry.Register("retract", Retract);
        registry.Register("match", Match);
        registry.Register("quote", Quote);
    }

    /// <summary>
    /// Implication rule: (implies condition conclusion)
    /// When condition matches atoms in space, derives conclusion with bindings applied.
    /// </summary>
    /// <remarks>
    /// This is a forward-chaining inference rule. Given (implies A B),
    /// when A matches facts in the space, B is derived with the matching substitution.
    /// </remarks>
    public static IEnumerable<Atom> Implies(IAtomSpace space, Expression args)
    {
        if (args.Children.Count < 3)
        {
            yield break;
        }

        var condition = args.Children[1];
        var conclusion = args.Children[2];

        // Find all atoms matching the condition
        foreach (var (_, bindings) in space.Query(condition))
        {
            // Apply bindings to conclusion
            yield return bindings.Apply(conclusion);
        }
    }

    /// <summary>
    /// Equality check: (equal a b) returns the atom if a and b are equal.
    /// </summary>
    public static IEnumerable<Atom> Equal(IAtomSpace space, Expression args)
    {
        if (args.Children.Count < 3)
        {
            yield break;
        }

        var a = args.Children[1];
        var b = args.Children[2];

        if (a.Equals(b))
        {
            yield return Atom.Sym("True");
        }
    }

    /// <summary>
    /// Logical not: (not expr) succeeds if expr produces no results.
    /// Negation as failure.
    /// </summary>
    public static IEnumerable<Atom> Not(IAtomSpace space, Expression args)
    {
        if (args.Children.Count < 2)
        {
            yield break;
        }

        var expr = args.Children[1];

        // Query for the expression - if no results, "not" succeeds
        if (!space.Query(expr).Any())
        {
            yield return Atom.Sym("True");
        }
    }

    /// <summary>
    /// Logical and: (and a b ...) succeeds if all arguments can be matched.
    /// Returns True if all conjuncts succeed.
    /// </summary>
    public static IEnumerable<Atom> And(IAtomSpace space, Expression args)
    {
        if (args.Children.Count < 2)
        {
            yield break;
        }

        var conjuncts = args.Children.Skip(1).ToList();
        if (conjuncts.Count == 0)
        {
            yield return Atom.Sym("True");
            yield break;
        }

        // Check all conjuncts
        foreach (var (subst, _) in EvaluateConjuncts(space, conjuncts, Substitution.Empty))
        {
            yield return Atom.Sym("True");
            yield break; // At least one success
        }
    }

    /// <summary>
    /// Logical or: (or a b ...) succeeds if any argument can be matched.
    /// </summary>
    public static IEnumerable<Atom> Or(IAtomSpace space, Expression args)
    {
        if (args.Children.Count < 2)
        {
            yield break;
        }

        foreach (var disjunct in args.Children.Skip(1))
        {
            if (space.Query(disjunct).Any())
            {
                yield return Atom.Sym("True");
                yield break;
            }
        }
    }

    /// <summary>
    /// Assert: (assert atom) adds atom to the space.
    /// </summary>
    public static IEnumerable<Atom> Assert(IAtomSpace space, Expression args)
    {
        if (args.Children.Count < 2)
        {
            yield break;
        }

        var atom = args.Children[1];
        if (space is AtomSpace mutableSpace)
        {
            mutableSpace.Add(atom);
            yield return atom;
        }
    }

    /// <summary>
    /// Retract: (retract atom) removes atom from the space.
    /// </summary>
    public static IEnumerable<Atom> Retract(IAtomSpace space, Expression args)
    {
        if (args.Children.Count < 2)
        {
            yield break;
        }

        var atom = args.Children[1];
        if (space is AtomSpace mutableSpace && mutableSpace.Remove(atom))
        {
            yield return Atom.Sym("True");
        }
    }

    /// <summary>
    /// Match: (match pattern) returns all atoms matching the pattern.
    /// </summary>
    public static IEnumerable<Atom> Match(IAtomSpace space, Expression args)
    {
        if (args.Children.Count < 2)
        {
            yield break;
        }

        var pattern = args.Children[1];
        foreach (var (atom, _) in space.Query(pattern))
        {
            yield return atom;
        }
    }

    /// <summary>
    /// Quote: (quote atom) returns atom without evaluation.
    /// </summary>
    public static IEnumerable<Atom> Quote(IAtomSpace space, Expression args)
    {
        if (args.Children.Count < 2)
        {
            yield break;
        }

        yield return args.Children[1];
    }

    private static IEnumerable<(Substitution Subst, bool Success)> EvaluateConjuncts(
        IAtomSpace space,
        IReadOnlyList<Atom> conjuncts,
        Substitution currentSubst)
    {
        if (conjuncts.Count == 0)
        {
            yield return (currentSubst, true);
            yield break;
        }

        var first = currentSubst.Apply(conjuncts[0]);
        var rest = conjuncts.Skip(1).ToList();

        foreach (var (_, bindings) in space.Query(first))
        {
            var composedSubst = currentSubst.Compose(bindings);
            if (composedSubst is null)
            {
                continue;
            }

            foreach (var result in EvaluateConjuncts(space, rest, composedSubst))
            {
                yield return result;
            }
        }
    }
}