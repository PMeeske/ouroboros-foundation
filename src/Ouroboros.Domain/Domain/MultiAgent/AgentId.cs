// <copyright file="AgentId.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.MultiAgent;

/// <summary>
/// Unique identifier for an agent instance.
/// </summary>
/// <param name="Value">The unique identifier value.</param>
/// <param name="Name">The human-readable name of the agent.</param>
[ExcludeFromCodeCoverage]
public sealed record AgentId(Guid Value, string Name);
