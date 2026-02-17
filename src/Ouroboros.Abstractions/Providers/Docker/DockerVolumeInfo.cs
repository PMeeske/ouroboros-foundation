namespace Ouroboros.Providers.Docker;

/// <summary>
/// Represents a Docker volume.
/// </summary>
public sealed record DockerVolumeInfo
{
    /// <summary>Gets the volume name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the driver.</summary>
    public required string Driver { get; init; }

    /// <summary>Gets the mountpoint.</summary>
    public string? Mountpoint { get; init; }

    /// <summary>Gets the volume labels.</summary>
    public IReadOnlyDictionary<string, string> Labels { get; init; } = new Dictionary<string, string>();
}