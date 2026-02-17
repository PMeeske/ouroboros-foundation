namespace Ouroboros.Core.Synthesis;

/// <summary>
/// Represents a type rule in a DSL.
/// </summary>
/// <param name="Name">The name of the type rule.</param>
/// <param name="InputTypes">The input types for this rule.</param>
/// <param name="OutputType">The output type produced by this rule.</param>
public sealed record TypeRule(
    string Name,
    List<string> InputTypes,
    string OutputType);