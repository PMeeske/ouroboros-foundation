namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Extension methods for monadic composition of atom queries.
/// </summary>
public static class InterpreterExtensions
{
    /// <summary>
    /// Converts an atom to Option.
    /// </summary>
    /// <param name="atom">The atom.</param>
    /// <returns>Some(atom).</returns>
    public static Option<Atom> ToOption(this Atom atom) =>
        Option<Atom>.Some(atom);

    /// <summary>
    /// Evaluates a query and pipes results through a continuation.
    /// </summary>
    /// <param name="interpreter">The interpreter.</param>
    /// <param name="query">The query to evaluate.</param>
    /// <param name="continuation">The continuation to apply to results.</param>
    /// <returns>Transformed results.</returns>
    public static IEnumerable<TResult> EvaluateAndThen<TResult>(
        this Interpreter interpreter,
        Atom query,
        Func<IEnumerable<Atom>, IEnumerable<TResult>> continuation)
    {
        return continuation(interpreter.Evaluate(query));
    }

    /// <summary>
    /// Combines results from multiple queries.
    /// </summary>
    /// <param name="interpreter">The interpreter.</param>
    /// <param name="queries">The queries to evaluate.</param>
    /// <returns>Combined results from all queries.</returns>
    public static IEnumerable<Atom> EvaluateAll(
        this Interpreter interpreter,
        params Atom[] queries)
    {
        return queries.SelectMany(q => interpreter.Evaluate(q));
    }
}