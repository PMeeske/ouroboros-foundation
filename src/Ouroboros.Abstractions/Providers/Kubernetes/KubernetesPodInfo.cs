// <copyright file="KubernetesModels.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Providers.Kubernetes;

/// <summary>
/// Represents a Kubernetes pod.
/// </summary>
public sealed record KubernetesPodInfo
{
    /// <summary>Gets the pod name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the namespace.</summary>
    public required string Namespace { get; init; }

    /// <summary>Gets the pod phase (Running, Pending, Succeeded, Failed, Unknown).</summary>
    public required string Phase { get; init; }

    /// <summary>Gets the pod IP address.</summary>
    public string? PodIp { get; init; }

    /// <summary>Gets the node the pod is running on.</summary>
    public string? NodeName { get; init; }

    /// <summary>Gets the pod labels.</summary>
    public IReadOnlyDictionary<string, string> Labels { get; init; } = new Dictionary<string, string>();

    /// <summary>Gets the container names.</summary>
    public IReadOnlyList<string> Containers { get; init; } = [];

    /// <summary>Gets the pod creation timestamp.</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>Gets the restart count across all containers.</summary>
    public int RestartCount { get; init; }
}