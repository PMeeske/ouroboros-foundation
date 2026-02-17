namespace Ouroboros.Agent.MetaAI.MetaLearning;

/// <summary>
/// Enumeration of learning approaches.
/// </summary>
public enum LearningApproach
{
    /// <summary>Supervised learning with labeled examples.</summary>
    Supervised,

    /// <summary>Reinforcement learning with rewards.</summary>
    Reinforcement,

    /// <summary>Self-supervised learning from unlabeled data.</summary>
    SelfSupervised,

    /// <summary>Learning by imitating expert demonstrations.</summary>
    ImitationLearning,

    /// <summary>Progressive learning from simple to complex.</summary>
    CurriculumLearning,

    /// <summary>Meta-learning using gradient-based optimization.</summary>
    MetaGradient,

    /// <summary>Prototypical learning with similarity metrics.</summary>
    PrototypicalLearning,
}