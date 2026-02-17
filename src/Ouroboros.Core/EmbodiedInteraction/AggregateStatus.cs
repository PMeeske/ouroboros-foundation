namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Status of the aggregate.
/// </summary>
public enum AggregateStatus
{
    /// <summary>Aggregate is inactive.</summary>
    Inactive,

    /// <summary>Aggregate is activating.</summary>
    Activating,

    /// <summary>Aggregate is active.</summary>
    Active,

    /// <summary>Aggregate is deactivating.</summary>
    Deactivating,

    /// <summary>Aggregate failed to activate.</summary>
    Failed
}