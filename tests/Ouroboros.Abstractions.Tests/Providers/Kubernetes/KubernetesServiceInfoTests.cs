namespace Ouroboros.Abstractions.Tests.Providers.Kubernetes;

using Ouroboros.Providers.Kubernetes;

[Trait("Category", "Unit")]
public class KubernetesServiceInfoTests
{
    [Fact]
    public void Constructor_RequiredProperties_Set()
    {
        var info = new KubernetesServiceInfo
        {
            Name = "my-svc",
            Namespace = "default",
            Type = "ClusterIP",
        };

        info.Name.Should().Be("my-svc");
        info.Namespace.Should().Be("default");
        info.Type.Should().Be("ClusterIP");
    }

    [Fact]
    public void ClusterIp_DefaultsToNull()
    {
        var info = new KubernetesServiceInfo { Name = "n", Namespace = "ns", Type = "t" };
        info.ClusterIp.Should().BeNull();
    }

    [Fact]
    public void ExternalIp_DefaultsToNull()
    {
        var info = new KubernetesServiceInfo { Name = "n", Namespace = "ns", Type = "t" };
        info.ExternalIp.Should().BeNull();
    }

    [Fact]
    public void Ports_DefaultsToEmpty()
    {
        var info = new KubernetesServiceInfo { Name = "n", Namespace = "ns", Type = "t" };
        info.Ports.Should().BeEmpty();
    }

    [Fact]
    public void Selector_DefaultsToEmpty()
    {
        var info = new KubernetesServiceInfo { Name = "n", Namespace = "ns", Type = "t" };
        info.Selector.Should().BeEmpty();
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var ports = new List<KubernetesPortInfo>
        {
            new() { Port = 80, TargetPort = 8080 }
        };

        var info = new KubernetesServiceInfo
        {
            Name = "svc",
            Namespace = "prod",
            Type = "LoadBalancer",
            ClusterIp = "10.0.0.1",
            ExternalIp = "1.2.3.4",
            Ports = ports,
            Selector = new Dictionary<string, string> { { "app", "web" } },
        };

        info.ClusterIp.Should().Be("10.0.0.1");
        info.ExternalIp.Should().Be("1.2.3.4");
        info.Ports.Should().HaveCount(1);
        info.Selector.Should().ContainKey("app");
    }
}
