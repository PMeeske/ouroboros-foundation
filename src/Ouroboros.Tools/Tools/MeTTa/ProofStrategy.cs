using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Strategy for theorem proving.
/// </summary>
[ExcludeFromCodeCoverage]
public enum ProofStrategy
{
    /// <summary>Resolution-based proving.</summary>
    Resolution,

    /// <summary>Tableaux method.</summary>
    Tableaux,

    /// <summary>Natural deduction.</summary>
    NaturalDeduction,
}