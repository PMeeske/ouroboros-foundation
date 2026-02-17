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