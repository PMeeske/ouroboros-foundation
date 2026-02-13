// <copyright file="TemporalTypes.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Agent.TemporalReasoning;

/// <summary>
/// Represents a temporal relation between two events (Allen's interval algebra).
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
/// Represents a causal relationship between events.
/// </summary>
public sealed record CausalRelation(
    string CauseEventId,
    string EffectEventId,
    double Confidence,
    string? Mechanism = null);

/// <summary>
/// Represents a predicted future event.
/// </summary>
public sealed record PredictedEvent(
    string EventType,
    DateTime PredictedTime,
    double Probability,
    string? Description = null);

/// <summary>
/// Represents an ordered timeline of events.
/// </summary>
public sealed record Timeline(
    List<Abstractions.Domain.TemporalEvent> Events,
    DateTime Start,
    DateTime End,
    string? Description = null);

/// <summary>
/// Represents a temporal constraint for satisfiability checking.
/// </summary>
public sealed record TemporalConstraint(
    string EventIdA,
    string EventIdB,
    TemporalRelation RequiredRelation,
    TimeSpan? MinGap = null,
    TimeSpan? MaxGap = null);
