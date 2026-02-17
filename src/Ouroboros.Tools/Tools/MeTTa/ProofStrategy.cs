namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Strategy for theorem proving.
/// </summary>
public enum ProofStrategy
{
    /// <summary>Resolution-based proving.</summary>
    Resolution,

    /// <summary>Tableaux method.</summary>
    Tableaux,

    /// <summary>Natural deduction.</summary>
    NaturalDeduction,
}