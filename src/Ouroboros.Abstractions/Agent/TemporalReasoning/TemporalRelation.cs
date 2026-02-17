namespace Ouroboros.Agent.TemporalReasoning;

/// <summary>
/// Represents a temporal relation between two events (Allen's interval algebra) — simplified canonical form.
/// </summary>
public enum TemporalRelation
{
    /// <summary>Event A happens before Event B.</summary>
    Before,

    /// <summary>Event A happens after Event B.</summary>
    After,

    /// <summary>Events overlap in time.</summary>
    Overlaps,

    /// <summary>Events occur simultaneously.</summary>
    Simultaneous,

    /// <summary>Event A contains Event B.</summary>
    Contains,

    /// <summary>Event A is contained within Event B.</summary>
    During,

    /// <summary>Events meet (one ends as the other starts).</summary>
    Meets,

    /// <summary>Unknown temporal relation.</summary>
    Unknown,
}