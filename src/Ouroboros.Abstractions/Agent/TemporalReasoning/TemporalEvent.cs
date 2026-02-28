// <copyright file="TemporalTypes.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

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
