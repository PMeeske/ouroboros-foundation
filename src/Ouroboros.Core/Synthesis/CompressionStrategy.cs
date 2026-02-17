namespace Ouroboros.Core.Synthesis;

/// <summary>
/// Compression strategy for library learning.
/// </summary>
public enum CompressionStrategy
{
    /// <summary>
    /// Find common patterns via anti-unification.
    /// </summary>
    AntiUnification,

    /// <summary>
    /// E-graph based compression.
    /// </summary>
    EGraph,

    /// <summary>
    /// Grammar-based fragment extraction.
    /// </summary>
    FragmentGrammar,
}