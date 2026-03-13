// <copyright file="HealthStatus.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.Infrastructure.HealthCheck;

/// <summary>
/// Represents the health status of a component.
/// </summary>
[ExcludeFromCodeCoverage]
public enum HealthStatus
{
    /// <summary>
    /// Component is healthy and fully operational.
    /// </summary>
    Healthy,

    /// <summary>
    /// Component is operational but experiencing degraded performance.
    /// </summary>
    Degraded,

    /// <summary>
    /// Component is unhealthy or not operational.
    /// </summary>
    Unhealthy,
}
