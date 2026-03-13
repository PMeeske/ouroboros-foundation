// <copyright file="KnowledgeSyncStrategy.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.MultiAgent;

/// <summary>
/// Strategy for synchronizing knowledge between agents.
/// </summary>
[ExcludeFromCodeCoverage]
public enum KnowledgeSyncStrategy
{
    /// <summary>
    /// Complete knowledge transfer - all knowledge is shared.
    /// </summary>
    Full,

    /// <summary>
    /// Only new knowledge since last sync is transferred.
    /// </summary>
    Incremental,

    /// <summary>
    /// Only relevant knowledge for current tasks is shared.
    /// </summary>
    Selective,

    /// <summary>
    /// Probabilistic propagation where knowledge spreads through network.
    /// </summary>
    Gossip,
}
