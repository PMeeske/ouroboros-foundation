namespace Ouroboros.Core.Reasoning;

/// <summary>
/// Represents a variable in the causal graph.
/// </summary>
/// <param name="Name">The name of the variable.</param>
/// <param name="Type">The type of the variable (binary, categorical, continuous, ordinal).</param>
/// <param name="PossibleValues">The possible values this variable can take.</param>
public sealed record Variable(
    string Name,
    VariableType Type,
    List<object> PossibleValues);