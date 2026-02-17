// <copyright file="NeuralNetwork.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Represents a message passed between neurons.
/// </summary>
public sealed record NeuronMessage
{
    /// <summary>Unique message identifier.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Source neuron identifier.</summary>
    public required string SourceNeuron { get; init; }

    /// <summary>Target neuron identifier (null = broadcast).</summary>
    public string? TargetNeuron { get; init; }

    /// <summary>Message type/topic.</summary>
    public required string Topic { get; init; }

    /// <summary>Message payload.</summary>
    public required object Payload { get; init; }

    /// <summary>Message priority.</summary>
    public IntentionPriority Priority { get; init; } = IntentionPriority.Normal;

    /// <summary>When the message was created.</summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Time-to-live in seconds (0 = no expiry).</summary>
    public int TtlSeconds { get; init; } = 0;

    /// <summary>Whether this message expects a response.</summary>
    public bool ExpectsResponse { get; init; } = false;

    /// <summary>Correlation ID for request-response patterns.</summary>
    public Guid? CorrelationId { get; init; }

    /// <summary>Vector embedding for semantic routing.</summary>
    public float[]? Embedding { get; init; }
}