// <copyright file="AgentGroup.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.MultiAgent;

/// <summary>
/// Represents a logical grouping of agents with a specific distribution strategy.
/// </summary>
/// <param name="Name">The name of the group.</param>
/// <param name="Members">The agents that belong to this group.</param>
/// <param name="Type">The message distribution strategy for the group.</param>
[ExcludeFromCodeCoverage]
public sealed record AgentGroup(
    string Name,
    List<AgentId> Members,
    GroupType Type);
