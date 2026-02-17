// <copyright file="DockerModels.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Providers.Docker;

/// <summary>
/// Represents a Docker container.
/// </summary>
public sealed record DockerContainerInfo
{
    /// <summary>Gets the container ID.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the container short ID (first 12 chars).</summary>
    public string ShortId => Id.Length > 12 ? Id[..12] : Id;

    /// <summary>Gets the container name(s).</summary>
    public IReadOnlyList<string> Names { get; init; } = [];

    /// <summary>Gets the image used.</summary>
    public required string Image { get; init; }

    /// <summary>Gets the container state (running, exited, created, etc.).</summary>
    public required string State { get; init; }

    /// <summary>Gets the status string (e.g., "Up 3 hours").</summary>
    public string? Status { get; init; }

    /// <summary>Gets the port mappings.</summary>
    public IReadOnlyList<DockerPortMapping> Ports { get; init; } = [];

    /// <summary>Gets the container labels.</summary>
    public IReadOnlyDictionary<string, string> Labels { get; init; } = new Dictionary<string, string>();

    /// <summary>Gets the creation timestamp.</summary>
    public DateTimeOffset? CreatedAt { get; init; }
}