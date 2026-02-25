// <copyright file="InteractionEvents.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Domain.Voice;

/// <summary>
/// Base type for all interaction events in the unified voice stream.
/// All modalities (text, voice, agent output) share this common base,
/// enabling unified stream composition via IObservable{InteractionEvent}.
/// </summary>
public abstract record InteractionEvent
{
    /// <summary>Gets unique identifier for correlation.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Gets timestamp when the event was created.</summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Gets source of the event (User, Agent, System).</summary>
    public required InteractionSource Source { get; init; }

    /// <summary>Gets optional correlation ID for request/response pairing.</summary>
    public Guid? CorrelationId { get; init; }
}

// ============================================================================
// USER INPUT EVENTS
// ============================================================================

// ============================================================================
// AGENT EVENTS
// ============================================================================

// ============================================================================
// OUTPUT EVENTS
// ============================================================================

// ============================================================================
// PRESENCE/STATE EVENTS
// ============================================================================

// ============================================================================
// UTILITY EVENTS
// ============================================================================