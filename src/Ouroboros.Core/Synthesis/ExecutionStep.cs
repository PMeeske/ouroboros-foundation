namespace Ouroboros.Core.Synthesis;

/// <summary>
/// Represents a single step in program execution.
/// </summary>
/// <param name="PrimitiveName">The name of the primitive executed.</param>
/// <param name="Inputs">The input values to the primitive.</param>
/// <param name="Output">The output value produced by the primitive.</param>
public sealed record ExecutionStep(
    string PrimitiveName,
    List<object> Inputs,
    object Output);