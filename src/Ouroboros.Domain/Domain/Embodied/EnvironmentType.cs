// <copyright file="EnvironmentType.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Embodied;

/// <summary>
/// Defines the types of environments supported for embodied simulation.
/// </summary>
[ExcludeFromCodeCoverage]
public enum EnvironmentType
{
    /// <summary>
    /// Unity ML-Agents environment.
    /// </summary>
    Unity,

    /// <summary>
    /// OpenAI Gym compatible environment.
    /// </summary>
    Gym,

    /// <summary>
    /// Custom environment implementation.
    /// </summary>
    Custom,

    /// <summary>
    /// Physics simulation environment.
    /// </summary>
    Simulation,
}
