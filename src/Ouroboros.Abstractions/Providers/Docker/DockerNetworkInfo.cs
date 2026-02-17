namespace Ouroboros.Providers.Docker;

/// <summary>
/// Represents a Docker network.
/// </summary>
public sealed record DockerNetworkInfo
{
    /// <summary>Gets the network ID.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the network name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the driver (bridge, overlay, host, etc.).</summary>
    public required string Driver { get; init; }

    /// <summary>Gets the network scope (local, swarm, global).</summary>
    public string? Scope { get; init; }
}