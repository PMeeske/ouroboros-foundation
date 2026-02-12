// <copyright file="TransitionEdge.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Ouroboros.Abstractions.Network;

/// <summary>
/// Simplified representation of a transition edge between monadic nodes.
/// This is a minimal version used by interfaces; the full implementation remains in Network.
/// </summary>
public sealed record TransitionEdge
{
    /// <summary>
    /// Gets the unique identifier for this edge.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the ID of the source node.
    /// </summary>
    public Guid SourceId { get; init; }

    /// <summary>
    /// Gets the ID of the target node.
    /// </summary>
    public Guid TargetId { get; init; }

    /// <summary>
    /// Gets the type of transition.
    /// </summary>
    public string TransitionType { get; init; }

    /// <summary>
    /// Gets the timestamp when this edge was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the metadata associated with this transition.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransitionEdge"/> class.
    /// </summary>
    public TransitionEdge(
        Guid id,
        Guid sourceId,
        Guid targetId,
        string transitionType,
        DateTimeOffset createdAt,
        Dictionary<string, object> metadata)
    {
        this.Id = id;
        this.SourceId = sourceId;
        this.TargetId = targetId;
        this.TransitionType = transitionType ?? throw new ArgumentNullException(nameof(transitionType));
        this.CreatedAt = createdAt;
        this.Metadata = metadata ?? new Dictionary<string, object>();
    }
}