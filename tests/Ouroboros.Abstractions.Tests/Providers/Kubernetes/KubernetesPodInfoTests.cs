namespace Ouroboros.Abstractions.Tests.Providers.Kubernetes;

using Ouroboros.Providers.Kubernetes;

[Trait("Category", "Unit")]
public class KubernetesPodInfoTests
{
    [Fact]
    public void Constructor_RequiredProperties_Set()
    {
        var info = new KubernetesPodInfo
        {
            Name = "my-pod",
            Namespace = "default",
            Phase = "Running",
        };

        info.Name.Should().Be("my-pod");
        info.Namespace.Should().Be("default");
        info.Phase.Should().Be("Running");
    }

    [Fact]
    public void PodIp_DefaultsToNull()
    {
        var info = new KubernetesPodInfo { Name = "n", Namespace = "ns", Phase = "p" };
        info.PodIp.Should().BeNull();
    }

    [Fact]
    public void NodeName_DefaultsToNull()
    {
        var info = new KubernetesPodInfo { Name = "n", Namespace = "ns", Phase = "p" };
        info.NodeName.Should().BeNull();
    }

    [Fact]
    public void Labels_DefaultsToEmpty()
    {
        var info = new KubernetesPodInfo { Name = "n", Namespace = "ns", Phase = "p" };
        info.Labels.Should().BeEmpty();
    }

    [Fact]
    public void Containers_DefaultsToEmpty()
    {
        var info = new KubernetesPodInfo { Name = "n", Namespace = "ns", Phase = "p" };
        info.Containers.Should().BeEmpty();
    }

    [Fact]
    public void RestartCount_DefaultsToZero()
    {
        var info = new KubernetesPodInfo { Name = "n", Namespace = "ns", Phase = "p" };
        info.RestartCount.Should().Be(0);
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var info = new KubernetesPodInfo
        {
            Name = "pod",
            Namespace = "prod",
            Phase = "Running",
            PodIp = "10.0.0.1",
            NodeName = "node1",
            Labels = new Dictionary<string, string> { { "app", "web" } },
            Containers = new[] { "nginx", "sidecar" },
            CreatedAt = DateTimeOffset.UtcNow,
            RestartCount = 2,
        };

        info.PodIp.Should().Be("10.0.0.1");
        info.NodeName.Should().Be("node1");
        info.Containers.Should().HaveCount(2);
        info.RestartCount.Should().Be(2);
    }
}
