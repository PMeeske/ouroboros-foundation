using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Represents a typed atom.
/// </summary>
/// <param name="Atom">The atom expression.</param>
/// <param name="Type">The inferred type.</param>
/// <param name="TypeParameters">Type parameters if polymorphic.</param>
[ExcludeFromCodeCoverage]
public sealed record TypedAtom(
    string Atom,
    string Type,
    Dictionary<string, string> TypeParameters);