namespace Ouroboros.Providers.Docker;

/// <summary>
/// Represents a Docker port mapping.
/// </summary>
public sealed record DockerPortMapping
{
    /// <summary>Gets the host IP.</summary>
    public string? HostIp { get; init; }

    /// <summary>Gets the host port.</summary>
    public int? HostPort { get; init; }

    /// <summary>Gets the container port.</summary>
    public int ContainerPort { get; init; }

    /// <summary>Gets the protocol (tcp/udp).</summary>
    public string Protocol { get; init; } = "tcp";
}