namespace Ouroboros.Abstractions.Tests.Providers.Kubernetes;

using Ouroboros.Providers.Kubernetes;

[Trait("Category", "Unit")]
public class KubernetesPortInfoTests
{
    [Fact]
    public void Name_DefaultsToNull()
    {
        var info = new KubernetesPortInfo();
        info.Name.Should().BeNull();
    }

    [Fact]
    public void Protocol_DefaultsToTcp()
    {
        var info = new KubernetesPortInfo();
        info.Protocol.Should().Be("TCP");
    }

    [Fact]
    public void Port_DefaultsToZero()
    {
        var info = new KubernetesPortInfo();
        info.Port.Should().Be(0);
    }

    [Fact]
    public void TargetPort_DefaultsToZero()
    {
        var info = new KubernetesPortInfo();
        info.TargetPort.Should().Be(0);
    }

    [Fact]
    public void NodePort_DefaultsToNull()
    {
        var info = new KubernetesPortInfo();
        info.NodePort.Should().BeNull();
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var info = new KubernetesPortInfo
        {
            Name = "http",
            Protocol = "TCP",
            Port = 80,
            TargetPort = 8080,
            NodePort = 30080,
        };

        info.Name.Should().Be("http");
        info.Protocol.Should().Be("TCP");
        info.Port.Should().Be(80);
        info.TargetPort.Should().Be(8080);
        info.NodePort.Should().Be(30080);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var p1 = new KubernetesPortInfo { Port = 80, TargetPort = 8080 };
        var p2 = new KubernetesPortInfo { Port = 80, TargetPort = 8080 };
        p1.Should().Be(p2);
    }
}
