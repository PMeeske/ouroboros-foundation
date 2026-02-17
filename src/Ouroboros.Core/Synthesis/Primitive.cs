namespace Ouroboros.Core.Synthesis;

/// <summary>
/// Represents a primitive operation in a DSL.
/// </summary>
/// <param name="Name">The name of the primitive.</param>
/// <param name="Type">The type signature of the primitive.</param>
/// <param name="Implementation">The executable implementation of the primitive.</param>
/// <param name="LogPrior">The log prior probability of using this primitive.</param>
public sealed record Primitive(
    string Name,
    string Type,
    Func<object[], object> Implementation,
    double LogPrior);