// <copyright file="TemporalTypes.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Ouroboros.Agent.TemporalReasoning;

/// <summary>
/// Represents a temporal event with a start time and optional end time.
/// </summary>
public sealed record TemporalEvent(
    Guid Id,
    string EventType,
    string Description,
    DateTime StartTime,
    DateTime? EndTime,
    IReadOnlyDictionary<string, object> Properties,
    IReadOnlyList<string> Participants);

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

/// <summary>
/// Allen Interval Algebra relation types for temporal reasoning (detailed form).
/// </summary>
public enum TemporalRelationType
{
    /// <summary>A ends before B starts.</summary>
    Before,

    /// <summary>A starts after B ends.</summary>
    After,

    /// <summary>A ends exactly when B starts.</summary>
    Meets,

    /// <summary>A starts exactly when B ends.</summary>
    MetBy,

    /// <summary>A starts before B, ends during B.</summary>
    Overlaps,

    /// <summary>B starts before A, ends during A.</summary>
    OverlappedBy,

    /// <summary>A is contained within B.</summary>
    During,

    /// <summary>A contains B.</summary>
    Contains,

    /// <summary>A and B start together, A ends first.</summary>
    Starts,

    /// <summary>A and B start together, B ends first.</summary>
    StartedBy,

    /// <summary>A and B end together, A starts later.</summary>
    Finishes,

    /// <summary>A and B end together, B starts later.</summary>
    FinishedBy,

    /// <summary>A and B have same start and end.</summary>
    Equals,
}

/// <summary>
/// Represents a temporal relationship between two events using Allen Interval Algebra.
/// </summary>
public sealed record TemporalRelationEdge(
    TemporalEvent Event1,
    TemporalEvent Event2,
    TemporalRelationType RelationType,
    double Confidence);

/// <summary>
/// Default configuration values for temporal queries.
/// </summary>
public static class TemporalQueryDefaults
{
    /// <summary>
    /// Default maximum number of results to return from a temporal query.
    /// </summary>
    public const int DefaultMaxResults = 100;
}

/// <summary>
/// Parameters for querying temporal events.
/// </summary>
public sealed record TemporalQuery(
    DateTime? StartAfter = null,
    DateTime? StartBefore = null,
    DateTime? EndAfter = null,
    DateTime? EndBefore = null,
    string? EventType = null,
    int MaxResults = 100,
    DateTime? After = null,
    DateTime? Before = null,
    TimeSpan? Duration = null,
    Guid? RelatedEventId = null);

/// <summary>
/// Represents a causal relationship between two temporal events.
/// </summary>
public sealed record CausalRelation(
    TemporalEvent Cause,
    TemporalEvent Effect,
    double CausalStrength,
    string Mechanism,
    IReadOnlyList<string> ConfoundingFactors);

/// <summary>
/// Represents a predicted future event based on temporal patterns.
/// </summary>
public sealed record PredictedEvent(
    string EventType,
    string Description,
    DateTime PredictedTime,
    double Confidence,
    IReadOnlyList<TemporalEvent> BasedOnEvents,
    string ReasoningExplanation);

/// <summary>
/// Represents a timeline constructed from a set of temporal events.
/// </summary>
public sealed record Timeline(
    IReadOnlyList<TemporalEvent> Events,
    IReadOnlyList<TemporalRelationEdge> Relations,
    DateTime EarliestTime,
    DateTime LatestTime,
    IReadOnlyDictionary<string, IReadOnlyList<TemporalEvent>> EventsByType);

/// <summary>
/// Represents a temporal constraint for satisfiability checking.
/// </summary>
public sealed record TemporalConstraint(
    string EventIdA,
    string EventIdB,
    TemporalRelation RequiredRelation,
    TimeSpan? MinGap = null,
    TimeSpan? MaxGap = null);
