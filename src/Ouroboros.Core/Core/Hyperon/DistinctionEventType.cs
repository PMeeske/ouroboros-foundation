namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Types of distinction events that can occur.
/// </summary>
public enum DistinctionEventType
{
    /// <summary>
    /// A new distinction was drawn (mark created).
    /// </summary>
    DistinctionDrawn,

    /// <summary>
    /// A distinction was crossed (negated).
    /// </summary>
    Crossed,

    /// <summary>
    /// Two distinctions were condensed (Law of Calling).
    /// </summary>
    Condensed,

    /// <summary>
    /// A double crossing was cancelled (Law of Crossing).
    /// </summary>
    Cancelled,

    /// <summary>
    /// A re-entry (self-reference) was created.
    /// </summary>
    ReEntryCreated,

    /// <summary>
    /// A distinction collapsed from imaginary to certain.
    /// </summary>
    Collapsed,

    /// <summary>
    /// A pattern was matched in the atom space.
    /// </summary>
    PatternMatched,

    /// <summary>
    /// An inference was derived through unification.
    /// </summary>
    InferenceDerived
}