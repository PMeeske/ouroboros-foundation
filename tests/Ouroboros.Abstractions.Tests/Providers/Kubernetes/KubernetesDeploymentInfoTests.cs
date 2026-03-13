namespace Ouroboros.Abstractions.Tests.Providers.Kubernetes;

using Ouroboros.Providers.Kubernetes;

[Trait("Category", "Unit")]
public class KubernetesDeploymentInfoTests
{
    [Fact]
    public void Constructor_RequiredProperties_Set()
    {
        var info = new KubernetesDeploymentInfo
        {
            Name = "my-deploy",
            Namespace = "default",
        };

        info.Name.Should().Be("my-deploy");
        info.Namespace.Should().Be("default");
    }

    [Fact]
    public void Replicas_DefaultsToZero()
    {
        var info = new KubernetesDeploymentInfo { Name = "n", Namespace = "ns" };
        info.Replicas.Should().Be(0);
    }

    [Fact]
    public void ReadyReplicas_DefaultsToZero()
    {
        var info = new KubernetesDeploymentInfo { Name = "n", Namespace = "ns" };
        info.ReadyReplicas.Should().Be(0);
    }

    [Fact]
    public void AvailableReplicas_DefaultsToZero()
    {
        var info = new KubernetesDeploymentInfo { Name = "n", Namespace = "ns" };
        info.AvailableReplicas.Should().Be(0);
    }

    [Fact]
    public void Labels_DefaultsToEmpty()
    {
        var info = new KubernetesDeploymentInfo { Name = "n", Namespace = "ns" };
        info.Labels.Should().BeEmpty();
    }

    [Fact]
    public void CreatedAt_DefaultsToNull()
    {
        var info = new KubernetesDeploymentInfo { Name = "n", Namespace = "ns" };
        info.CreatedAt.Should().BeNull();
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var created = DateTimeOffset.UtcNow;
        var info = new KubernetesDeploymentInfo
        {
            Name = "deploy",
            Namespace = "prod",
            Replicas = 3,
            ReadyReplicas = 3,
            AvailableReplicas = 3,
            Labels = new Dictionary<string, string> { { "app", "web" } },
            CreatedAt = created,
        };

        info.Replicas.Should().Be(3);
        info.ReadyReplicas.Should().Be(3);
        info.Labels.Should().ContainKey("app");
        info.CreatedAt.Should().Be(created);
    }
}
