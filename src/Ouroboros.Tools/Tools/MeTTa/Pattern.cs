using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Represents a pattern with variables.
/// </summary>
/// <param name="Template">Pattern template.</param>
/// <param name="Variables">List of variable names.</param>
[ExcludeFromCodeCoverage]
public sealed record Pattern(
    string Template,
    List<string> Variables);