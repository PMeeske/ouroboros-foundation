using Ouroboros.Providers.Kubernetes;

namespace Ouroboros.Abstractions.Tests.Providers.Kubernetes;

/// <summary>
/// Additional edge case tests for Kubernetes record types.
/// </summary>
[Trait("Category", "Unit")]
public class KubernetesRecordEqualityTests
{
    [Fact]
    public void KubernetesPodInfo_WithContainers_ContainersAccessible()
    {
        // Act
        var pod = new KubernetesPodInfo
        {
            Name = "multi-container-pod",
            Namespace = "default",
            Phase = "Running",
            Containers = new List<string> { "app", "sidecar", "init" }
        };

        // Assert
        pod.Containers.Should().HaveCount(3);
        pod.Containers.Should().Contain("sidecar");
    }

    [Fact]
    public void KubernetesDeploymentInfo_PartiallyReady_ShowsDiscrepancy()
    {
        // Act
        var deployment = new KubernetesDeploymentInfo
        {
            Name = "scaling-deploy",
            Namespace = "production",
            Replicas = 5,
            ReadyReplicas = 3,
            AvailableReplicas = 3
        };

        // Assert
        deployment.Replicas.Should().BeGreaterThan(deployment.ReadyReplicas);
        deployment.ReadyReplicas.Should().Be(deployment.AvailableReplicas);
    }

    [Fact]
    public void KubernetesServiceInfo_ClusterIP_WithNoPorts()
    {
        // Act
        var service = new KubernetesServiceInfo
        {
            Name = "headless-svc",
            Namespace = "default",
            Type = "ClusterIP",
            ClusterIp = "None"
        };

        // Assert
        service.ClusterIp.Should().Be("None");
        service.Ports.Should().BeEmpty();
        service.ExternalIp.Should().BeNull();
    }

    [Fact]
    public void KubernetesPortInfo_WithNodePort_AllPortsSet()
    {
        // Act
        var port = new KubernetesPortInfo
        {
            Name = "http",
            Protocol = "TCP",
            Port = 80,
            TargetPort = 8080,
            NodePort = 30080
        };

        // Assert
        port.Port.Should().Be(80);
        port.TargetPort.Should().Be(8080);
        port.NodePort.Should().Be(30080);
    }

    [Fact]
    public void KubernetesServiceInfo_WithMultiplePorts_PortsAccessible()
    {
        // Act
        var service = new KubernetesServiceInfo
        {
            Name = "multi-port-svc",
            Namespace = "default",
            Type = "LoadBalancer",
            Ports = new List<KubernetesPortInfo>
            {
                new() { Name = "http", Port = 80, TargetPort = 8080, Protocol = "TCP" },
                new() { Name = "https", Port = 443, TargetPort = 8443, Protocol = "TCP" },
                new() { Name = "grpc", Port = 9090, TargetPort = 9090, Protocol = "TCP" }
            }
        };

        // Assert
        service.Ports.Should().HaveCount(3);
        service.Ports[2].Name.Should().Be("grpc");
    }

    [Fact]
    public void KubernetesDeploymentInfo_WithLabels_SelectorMatching()
    {
        // Act
        var deployment = new KubernetesDeploymentInfo
        {
            Name = "labeled-deploy",
            Namespace = "prod",
            Labels = new Dictionary<string, string>
            {
                ["app"] = "web",
                ["version"] = "v2",
                ["team"] = "backend"
            }
        };

        // Assert
        deployment.Labels.Should().HaveCount(3);
        deployment.Labels["version"].Should().Be("v2");
    }
}
