namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Strategies for handling uncertain situations when primary approach fails.
/// </summary>
public enum FallbackStrategy
{
    /// <summary>Retry the same approach.</summary>
    Retry = 0,

    /// <summary>Escalate to human oversight.</summary>
    EscalateToHuman = 1,

    /// <summary>Use a simpler, more conservative approach.</summary>
    UseConservativeApproach = 2,

    /// <summary>Defer the decision for later.</summary>
    Defer = 3,

    /// <summary>Abort the operation.</summary>
    Abort = 4,

    /// <summary>Ask for clarification or more information.</summary>
    RequestClarification = 5,
}