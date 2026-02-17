namespace Ouroboros.Core.Synthesis;

/// <summary>
/// Represents a domain-specific language for program synthesis.
/// </summary>
/// <param name="Name">The name of the DSL.</param>
/// <param name="Primitives">The primitive operations available in the DSL.</param>
/// <param name="TypeRules">The type rules governing the DSL.</param>
/// <param name="Optimizations">Rewrite rules for optimizing programs in the DSL.</param>
public sealed record DomainSpecificLanguage(
    string Name,
    List<Primitive> Primitives,
    List<TypeRule> TypeRules,
    List<RewriteRule> Optimizations);