namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Types of limitations.
/// </summary>
public enum LimitationType
{
    /// <summary>Cannot perceive certain information.</summary>
    PerceptualBlind,

    /// <summary>Cannot perform certain actions.</summary>
    ActionRestricted,

    /// <summary>Finite memory/context window.</summary>
    MemoryBounded,

    /// <summary>Processing takes finite time.</summary>
    ProcessingTime,

    /// <summary>Knowledge is incomplete or outdated.</summary>
    KnowledgeGap,

    /// <summary>Cannot verify certain claims.</summary>
    VerificationLimit,

    /// <summary>Ethical/policy constraints.</summary>
    EthicalConstraint
}