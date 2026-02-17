namespace Ouroboros.Providers.Kubernetes;

/// <summary>
/// Represents a Kubernetes service.
/// </summary>
public sealed record KubernetesServiceInfo
{
    /// <summary>Gets the service name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the namespace.</summary>
    public required string Namespace { get; init; }

    /// <summary>Gets the service type (ClusterIP, NodePort, LoadBalancer, ExternalName).</summary>
    public required string Type { get; init; }

    /// <summary>Gets the cluster IP.</summary>
    public string? ClusterIp { get; init; }

    /// <summary>Gets the external IP if available.</summary>
    public string? ExternalIp { get; init; }

    /// <summary>Gets the service ports.</summary>
    public IReadOnlyList<KubernetesPortInfo> Ports { get; init; } = [];

    /// <summary>Gets the service selector labels.</summary>
    public IReadOnlyDictionary<string, string> Selector { get; init; } = new Dictionary<string, string>();
}