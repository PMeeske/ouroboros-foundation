// <copyright file="MonadNode.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Ouroboros.Abstractions.Network;

/// <summary>
/// Simplified representation of a monadic value node for interface definitions.
/// This is a minimal version used by interfaces; the full implementation remains in Network.
/// </summary>
public sealed record MonadNode
{
    /// <summary>
    /// Gets the unique identifier for this node.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the type name of the monad or state.
    /// </summary>
    public string TypeName { get; init; }

    /// <summary>
    /// Gets the serialized JSON payload of the monadic value.
    /// </summary>
    public string PayloadJson { get; init; }

    /// <summary>
    /// Gets the timestamp when this node was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the IDs of parent nodes in the DAG.
    /// </summary>
    public ImmutableArray<Guid> ParentIds { get; init; }

    /// <summary>
    /// Gets the Merkle hash of this node.
    /// </summary>
    public string Hash { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MonadNode"/> class.
    /// </summary>
    public MonadNode(
        Guid id,
        string typeName,
        string payloadJson,
        DateTimeOffset createdAt,
        ImmutableArray<Guid> parentIds,
        string hash)
    {
        this.Id = id;
        this.TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
        this.PayloadJson = payloadJson ?? throw new ArgumentNullException(nameof(payloadJson));
        this.CreatedAt = createdAt;
        this.ParentIds = parentIds;
        this.Hash = hash ?? throw new ArgumentNullException(nameof(hash));
    }
}