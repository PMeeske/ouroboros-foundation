namespace Ouroboros.Providers.Kubernetes;

/// <summary>
/// Represents a Kubernetes service port.
/// </summary>
public sealed record KubernetesPortInfo
{
    /// <summary>Gets the port name.</summary>
    public string? Name { get; init; }

    /// <summary>Gets the protocol (TCP, UDP).</summary>
    public string Protocol { get; init; } = "TCP";

    /// <summary>Gets the port number.</summary>
    public int Port { get; init; }

    /// <summary>Gets the target port.</summary>
    public int TargetPort { get; init; }

    /// <summary>Gets the node port (for NodePort/LoadBalancer types).</summary>
    public int? NodePort { get; init; }
}