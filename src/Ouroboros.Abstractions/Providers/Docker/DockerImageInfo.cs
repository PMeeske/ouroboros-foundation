namespace Ouroboros.Providers.Docker;

/// <summary>
/// Represents a Docker image.
/// </summary>
public sealed record DockerImageInfo
{
    /// <summary>Gets the image ID.</summary>
    public required string Id { get; init; }

    /// <summary>Gets the repository tags.</summary>
    public IReadOnlyList<string> RepoTags { get; init; } = [];

    /// <summary>Gets the image size in bytes.</summary>
    public long Size { get; init; }

    /// <summary>Gets the creation timestamp.</summary>
    public DateTimeOffset? CreatedAt { get; init; }
}