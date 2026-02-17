namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Represents a type context for type inference.
/// </summary>
/// <param name="Bindings">Variable to type bindings.</param>
/// <param name="Constraints">Type constraints.</param>
public sealed record TypeContext(
    Dictionary<string, string> Bindings,
    List<string> Constraints);