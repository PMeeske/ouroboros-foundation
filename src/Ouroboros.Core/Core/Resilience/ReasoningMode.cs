namespace Ouroboros.Core.Resilience;

/// <summary>
/// Reasoning modes for hybrid neural-symbolic systems.
/// IMPORTANT: This enum is duplicated here to avoid circular dependencies between Core and Agent layers.
/// It must be kept in sync with Ouroboros.Agent.NeuralSymbolic.ReasoningMode.
/// The integer values MUST match exactly for casting to work correctly in ResilientReasoner.
/// Changes to one enum must be reflected in the other.
/// 
/// Source of Truth: Ouroboros.Agent.NeuralSymbolic.ReasoningMode (defined in ReasoningResult.cs)
/// This enum: Duplicate for Core layer use only
/// </summary>
public enum ReasoningMode
{
    /// <summary>Try symbolic first, fall back to neural.</summary>
    SymbolicFirst = 0,

    /// <summary>Try neural first, fall back to symbolic.</summary>
    NeuralFirst = 1,

    /// <summary>Run both in parallel, combine results.</summary>
    Parallel = 2,

    /// <summary>Use only symbolic reasoning.</summary>
    SymbolicOnly = 3,

    /// <summary>Use only neural reasoning.</summary>
    NeuralOnly = 4
}