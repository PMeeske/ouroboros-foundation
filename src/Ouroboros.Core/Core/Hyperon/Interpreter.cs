// <copyright file="Interpreter.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Interpreter for evaluating atoms against an atom space.
/// Supports forward-chaining inference and grounded operations.
/// Designed with monadic composition patterns for pipeline integration.
/// </summary>
public sealed class Interpreter
{
    private readonly IAtomSpace space;
    private readonly GroundedRegistry groundedOps;

    /// <summary>
    /// Initializes a new instance of the <see cref="Interpreter"/> class.
    /// </summary>
    /// <param name="space">The atom space to evaluate against.</param>
    /// <param name="groundedOps">Optional custom grounded operations registry.</param>
    public Interpreter(IAtomSpace space, GroundedRegistry? groundedOps = null)
    {
        this.space = space ?? throw new ArgumentNullException(nameof(space));
        this.groundedOps = groundedOps ?? GroundedRegistry.CreateStandard();
    }

    /// <summary>
    /// Evaluates a query atom against the space.
    /// Returns all derived/matched atoms.
    /// </summary>
    /// <param name="query">The query atom.</param>
    /// <returns>Enumerable of result atoms.</returns>
    public IEnumerable<Atom> Evaluate(Atom query)
    {
        return EvaluateInternal(query, new HashSet<string>(), 0);
    }

    /// <summary>
    /// Evaluates a query and returns results along with their derivation substitutions.
    /// </summary>
    /// <param name="query">The query atom.</param>
    /// <returns>Enumerable of (result, substitution) pairs.</returns>
    public IEnumerable<(Atom Result, Substitution Bindings)> EvaluateWithBindings(Atom query)
    {
        // For ground queries, check direct match
        if (!query.ContainsVariables())
        {
            foreach (Atom result in EvaluateInternal(query, new HashSet<string>(), 0))
            {
                yield return (result, Substitution.Empty);
            }

            yield break;
        }

        // For patterns, return results with bindings
        foreach ((Atom? atom, Substitution? bindings) in space.Query(query))
        {
            yield return (atom, bindings);
        }

        // Also evaluate with implies rules
        foreach ((Atom Result, Substitution Bindings) ruleResult in EvaluateWithRules(query))
        {
            yield return ruleResult;
        }
    }

    private IEnumerable<Atom> EvaluateInternal(Atom query, HashSet<string> visited, int depth)
    {
        // Prevent infinite recursion
        const int maxDepth = 100;
        if (depth > maxDepth)
        {
            yield break;
        }

        string queryKey = query.ToSExpr();
        if (visited.Contains(queryKey))
        {
            yield break;
        }

        visited.Add(queryKey);

        // Case 1: Query is a grounded operation expression
        if (query is Expression expr && expr.Children.Count > 0 && expr.Children[0] is Symbol headSym)
        {
            Option<GroundedOperation> opResult = groundedOps.Get(headSym.Name);
            if (opResult.HasValue && opResult.Value is not null)
            {
                foreach (Atom result in opResult.Value(space, expr))
                {
                    yield return result;
                }

                yield break;
            }
        }

        // Case 2: Direct match in atom space (ground query or pattern match)
        foreach ((Atom _, Substitution? bindings) in space.Query(query))
        {
            yield return bindings.Apply(query);
        }

        // Case 3: Evaluate using implies rules
        foreach ((Atom? result, Substitution _) in EvaluateWithRules(query))
        {
            // Avoid duplicates from direct matches
            yield return result;
        }
    }

    private IEnumerable<(Atom Result, Substitution Bindings)> EvaluateWithRules(Atom query)
    {
        // Find all implies rules in the space
        Expression impliesPattern = Atom.Expr(Atom.Sym("implies"), Atom.Var("_cond"), Atom.Var("_conc"));

        foreach ((Atom? rule, Substitution _) in space.Query(impliesPattern))
        {
            if (rule is not Expression ruleExpr || ruleExpr.Children.Count < 3)
            {
                continue;
            }

            Atom condition = ruleExpr.Children[1];
            Atom conclusion = ruleExpr.Children[2];

            // Try to unify query with conclusion
            Substitution? queryBindings = Unifier.Unify(conclusion, query);
            if (queryBindings is null)
            {
                continue;
            }

            // Apply bindings to condition and check if it's satisfied
            Atom boundCondition = queryBindings.Apply(condition);

            foreach ((Atom _, Substitution? conditionBindings) in space.Query(boundCondition))
            {
                // Combine bindings and yield the result
                Substitution? composedBindings = queryBindings.Compose(conditionBindings);
                if (composedBindings is not null)
                {
                    Atom result = composedBindings.Apply(query);
                    yield return (result, composedBindings);
                }
            }
        }
    }

    /// <summary>
    /// Evaluates a query and returns true if any results are found.
    /// </summary>
    /// <param name="query">The query atom.</param>
    /// <returns>True if the query has at least one result.</returns>
    public bool Succeeds(Atom query) => Evaluate(query).Any();

    /// <summary>
    /// Evaluates a query and returns the first result, if any.
    /// </summary>
    /// <param name="query">The query atom.</param>
    /// <returns>The first result wrapped in Option, or None.</returns>
    public Option<Atom> EvaluateFirst(Atom query) =>
        Evaluate(query).FirstOrDefault()?.ToOption() ?? Option<Atom>.None();

    /// <summary>
    /// Creates a monadic query that can be composed with other operations.
    /// The returned function always creates an <see cref="Interpreter"/> bound to
    /// the <c>space</c> it receives, so the correct atom space is always used.
    /// When <paramref name="groundedOps"/> is provided, its operations are reused
    /// across queries, ensuring consistent behavior when custom operations have
    /// been registered.
    /// </summary>
    /// <param name="query">The query atom.</param>
    /// <param name="groundedOps">
    /// Optional grounded operations registry to reuse across invocations.
    /// When <c>null</c>, a standard registry is created per invocation.
    /// </param>
    /// <returns>A function that evaluates the query against a space.</returns>
    public static Func<IAtomSpace, IEnumerable<Atom>> Query(
        Atom query,
        GroundedRegistry? groundedOps = null) =>
        space => new Interpreter(space, groundedOps).Evaluate(query);

    /// <summary>
    /// Composes two queries using monadic bind (SelectMany).
    /// The second query can use results from the first.
    /// </summary>
    /// <typeparam name="T">The type of intermediate result.</typeparam>
    /// <param name="first">The first query producing intermediate values.</param>
    /// <param name="selector">Function to create the second query from first results.</param>
    /// <returns>Combined enumerable of results.</returns>
    public static IEnumerable<Atom> Bind<T>(IEnumerable<T> first, Func<T, IEnumerable<Atom>> selector)
    {
        return first.SelectMany(selector);
    }
}
