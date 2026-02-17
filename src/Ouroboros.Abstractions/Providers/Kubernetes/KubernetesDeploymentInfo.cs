namespace Ouroboros.Providers.Kubernetes;

/// <summary>
/// Represents a Kubernetes deployment.
/// </summary>
public sealed record KubernetesDeploymentInfo
{
    /// <summary>Gets the deployment name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the namespace.</summary>
    public required string Namespace { get; init; }

    /// <summary>Gets the desired replica count.</summary>
    public int Replicas { get; init; }

    /// <summary>Gets the number of ready replicas.</summary>
    public int ReadyReplicas { get; init; }

    /// <summary>Gets the number of available replicas.</summary>
    public int AvailableReplicas { get; init; }

    /// <summary>Gets the deployment labels.</summary>
    public IReadOnlyDictionary<string, string> Labels { get; init; } = new Dictionary<string, string>();

    /// <summary>Gets the deployment creation timestamp.</summary>
    public DateTimeOffset? CreatedAt { get; init; }
}