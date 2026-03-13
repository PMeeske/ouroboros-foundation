using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Strategy for rule induction.
/// </summary>
[ExcludeFromCodeCoverage]
public enum InductionStrategy
{
    /// <summary>First Order Inductive Learner.</summary>
    FOIL,

    /// <summary>General purpose learning algorithm.</summary>
    GOLEM,

    /// <summary>Progol algorithm.</summary>
    Progol,

    /// <summary>Inductive Logic Programming.</summary>
    ILP,
}