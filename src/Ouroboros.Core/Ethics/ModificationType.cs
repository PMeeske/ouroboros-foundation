namespace Ouroboros.Core.Ethics;

/// <summary>
/// Types of self-modification requests.
/// </summary>
public enum ModificationType
{
    /// <summary>Adding new capabilities or skills</summary>
    CapabilityAddition,

    /// <summary>Modifying existing behavior or logic</summary>
    BehaviorModification,

    /// <summary>Updating knowledge or data</summary>
    KnowledgeUpdate,

    /// <summary>Modifying goals or objectives</summary>
    GoalModification,

    /// <summary>Changing ethical constraints or parameters</summary>
    EthicsModification,

    /// <summary>System configuration changes</summary>
    ConfigurationChange
}