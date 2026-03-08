using Ouroboros.Providers.Kubernetes;

namespace Ouroboros.Abstractions.Tests.Providers.Kubernetes;

[Trait("Category", "Unit")]
public class KubernetesRecordTests
{
    [Fact]
    public void KubernetesPodInfo_AllPropertiesSet()
    {
        // Act
        var pod = new KubernetesPodInfo
        {
            Name = "my-app-pod-abc123",
            Namespace = "default",
            Phase = "Running",
            PodIp = "10.0.0.5",
            NodeName = "node-1",
            Labels = new Dictionary<string, string> { ["app"] = "my-app" },
            Containers = new List<string> { "main", "sidecar" },
            CreatedAt = DateTimeOffset.UtcNow,
            RestartCount = 2
        };

        // Assert
        pod.Name.Should().Be("my-app-pod-abc123");
        pod.Namespace.Should().Be("default");
        pod.Phase.Should().Be("Running");
        pod.PodIp.Should().Be("10.0.0.5");
        pod.NodeName.Should().Be("node-1");
        pod.Labels.Should().ContainKey("app");
        pod.Containers.Should().HaveCount(2);
        pod.CreatedAt.Should().NotBeNull();
        pod.RestartCount.Should().Be(2);
    }

    [Fact]
    public void KubernetesPodInfo_DefaultValues_AreCorrect()
    {
        // Act
        var pod = new KubernetesPodInfo
        {
            Name = "pod-1",
            Namespace = "default",
            Phase = "Pending"
        };

        // Assert
        pod.PodIp.Should().BeNull();
        pod.NodeName.Should().BeNull();
        pod.Labels.Should().BeEmpty();
        pod.Containers.Should().BeEmpty();
        pod.CreatedAt.Should().BeNull();
        pod.RestartCount.Should().Be(0);
    }

    [Fact]
    public void KubernetesDeploymentInfo_AllPropertiesSet()
    {
        // Act
        var deployment = new KubernetesDeploymentInfo
        {
            Name = "my-deployment",
            Namespace = "production",
            Replicas = 3,
            ReadyReplicas = 3,
            AvailableReplicas = 3,
            Labels = new Dictionary<string, string> { ["env"] = "prod" },
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        deployment.Name.Should().Be("my-deployment");
        deployment.Namespace.Should().Be("production");
        deployment.Replicas.Should().Be(3);
        deployment.ReadyReplicas.Should().Be(3);
        deployment.AvailableReplicas.Should().Be(3);
        deployment.Labels.Should().ContainKey("env");
        deployment.CreatedAt.Should().NotBeNull();
    }

    [Fact]
    public void KubernetesDeploymentInfo_DefaultValues_AreCorrect()
    {
        // Act
        var deployment = new KubernetesDeploymentInfo
        {
            Name = "deploy",
            Namespace = "default"
        };

        // Assert
        deployment.Replicas.Should().Be(0);
        deployment.ReadyReplicas.Should().Be(0);
        deployment.AvailableReplicas.Should().Be(0);
        deployment.Labels.Should().BeEmpty();
        deployment.CreatedAt.Should().BeNull();
    }

    [Fact]
    public void KubernetesServiceInfo_AllPropertiesSet()
    {
        // Act
        var service = new KubernetesServiceInfo
        {
            Name = "my-service",
            Namespace = "default",
            Type = "LoadBalancer",
            ClusterIp = "10.96.0.1",
            ExternalIp = "34.120.0.1",
            Ports = new List<KubernetesPortInfo>
            {
                new KubernetesPortInfo { Name = "http", Port = 80, TargetPort = 8080, Protocol = "TCP" }
            },
            Selector = new Dictionary<string, string> { ["app"] = "web" }
        };

        // Assert
        service.Name.Should().Be("my-service");
        service.Namespace.Should().Be("default");
        service.Type.Should().Be("LoadBalancer");
        service.ClusterIp.Should().Be("10.96.0.1");
        service.ExternalIp.Should().Be("34.120.0.1");
        service.Ports.Should().HaveCount(1);
        service.Selector.Should().ContainKey("app");
    }

    [Fact]
    public void KubernetesServiceInfo_DefaultValues_AreCorrect()
    {
        // Act
        var service = new KubernetesServiceInfo
        {
            Name = "svc",
            Namespace = "ns",
            Type = "ClusterIP"
        };

        // Assert
        service.ClusterIp.Should().BeNull();
        service.ExternalIp.Should().BeNull();
        service.Ports.Should().BeEmpty();
        service.Selector.Should().BeEmpty();
    }

    [Fact]
    public void KubernetesPortInfo_AllPropertiesSet()
    {
        // Act
        var port = new KubernetesPortInfo
        {
            Name = "https",
            Protocol = "TCP",
            Port = 443,
            TargetPort = 8443,
            NodePort = 30443
        };

        // Assert
        port.Name.Should().Be("https");
        port.Protocol.Should().Be("TCP");
        port.Port.Should().Be(443);
        port.TargetPort.Should().Be(8443);
        port.NodePort.Should().Be(30443);
    }

    [Fact]
    public void KubernetesPortInfo_DefaultValues_AreCorrect()
    {
        // Act
        var port = new KubernetesPortInfo();

        // Assert
        port.Name.Should().BeNull();
        port.Protocol.Should().Be("TCP");
        port.Port.Should().Be(0);
        port.TargetPort.Should().Be(0);
        port.NodePort.Should().BeNull();
    }
}
