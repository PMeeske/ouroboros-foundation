using Ouroboros.Providers.Docker;

namespace Ouroboros.Abstractions.Tests.Providers.Docker;

[Trait("Category", "Unit")]
public class DockerRecordTests
{
    [Fact]
    public void DockerContainerInfo_AllPropertiesSet()
    {
        // Act
        var container = new DockerContainerInfo
        {
            Id = "abc123def456789012345678",
            Names = new List<string> { "/my-container" },
            Image = "nginx:latest",
            State = "running",
            Status = "Up 3 hours",
            Ports = new List<DockerPortMapping>
            {
                new DockerPortMapping { HostPort = 8080, ContainerPort = 80, Protocol = "tcp" }
            },
            Labels = new Dictionary<string, string> { ["env"] = "prod" },
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        container.Id.Should().Be("abc123def456789012345678");
        container.Names.Should().Contain("/my-container");
        container.Image.Should().Be("nginx:latest");
        container.State.Should().Be("running");
        container.Status.Should().Be("Up 3 hours");
        container.Ports.Should().HaveCount(1);
        container.Labels.Should().ContainKey("env");
        container.CreatedAt.Should().NotBeNull();
    }

    [Fact]
    public void DockerContainerInfo_ShortId_TruncatesTo12Chars()
    {
        // Arrange
        var container = new DockerContainerInfo
        {
            Id = "abc123def456789012345678",
            Image = "nginx",
            State = "running"
        };

        // Act & Assert
        container.ShortId.Should().Be("abc123def456");
    }

    [Fact]
    public void DockerContainerInfo_ShortId_ShortIdPreserved()
    {
        // Arrange
        var container = new DockerContainerInfo
        {
            Id = "abc123",
            Image = "nginx",
            State = "exited"
        };

        // Act & Assert
        container.ShortId.Should().Be("abc123");
    }

    [Fact]
    public void DockerContainerInfo_DefaultCollections_AreEmpty()
    {
        // Act
        var container = new DockerContainerInfo
        {
            Id = "id1",
            Image = "img",
            State = "created"
        };

        // Assert
        container.Names.Should().BeEmpty();
        container.Ports.Should().BeEmpty();
        container.Labels.Should().BeEmpty();
        container.Status.Should().BeNull();
        container.CreatedAt.Should().BeNull();
    }

    [Fact]
    public void DockerImageInfo_AllPropertiesSet()
    {
        // Act
        var image = new DockerImageInfo
        {
            Id = "sha256:abc123",
            RepoTags = new List<string> { "nginx:latest", "nginx:1.25" },
            Size = 187654321,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Assert
        image.Id.Should().Be("sha256:abc123");
        image.RepoTags.Should().HaveCount(2);
        image.Size.Should().Be(187654321);
        image.CreatedAt.Should().NotBeNull();
    }

    [Fact]
    public void DockerImageInfo_DefaultValues_AreCorrect()
    {
        // Act
        var image = new DockerImageInfo { Id = "sha256:id" };

        // Assert
        image.RepoTags.Should().BeEmpty();
        image.Size.Should().Be(0);
        image.CreatedAt.Should().BeNull();
    }

    [Fact]
    public void DockerNetworkInfo_AllPropertiesSet()
    {
        // Act
        var network = new DockerNetworkInfo
        {
            Id = "net-1",
            Name = "my-network",
            Driver = "bridge",
            Scope = "local"
        };

        // Assert
        network.Id.Should().Be("net-1");
        network.Name.Should().Be("my-network");
        network.Driver.Should().Be("bridge");
        network.Scope.Should().Be("local");
    }

    [Fact]
    public void DockerNetworkInfo_DefaultScope_IsNull()
    {
        // Act
        var network = new DockerNetworkInfo
        {
            Id = "id",
            Name = "name",
            Driver = "overlay"
        };

        // Assert
        network.Scope.Should().BeNull();
    }

    [Fact]
    public void DockerPortMapping_AllPropertiesSet()
    {
        // Act
        var mapping = new DockerPortMapping
        {
            HostIp = "0.0.0.0",
            HostPort = 8080,
            ContainerPort = 80,
            Protocol = "tcp"
        };

        // Assert
        mapping.HostIp.Should().Be("0.0.0.0");
        mapping.HostPort.Should().Be(8080);
        mapping.ContainerPort.Should().Be(80);
        mapping.Protocol.Should().Be("tcp");
    }

    [Fact]
    public void DockerPortMapping_DefaultProtocol_IsTcp()
    {
        // Act
        var mapping = new DockerPortMapping { ContainerPort = 443 };

        // Assert
        mapping.Protocol.Should().Be("tcp");
        mapping.HostIp.Should().BeNull();
        mapping.HostPort.Should().BeNull();
    }

    [Fact]
    public void DockerVolumeInfo_AllPropertiesSet()
    {
        // Act
        var volume = new DockerVolumeInfo
        {
            Name = "my-volume",
            Driver = "local",
            Mountpoint = "/var/lib/docker/volumes/my-volume/_data",
            Labels = new Dictionary<string, string> { ["type"] = "data" }
        };

        // Assert
        volume.Name.Should().Be("my-volume");
        volume.Driver.Should().Be("local");
        volume.Mountpoint.Should().Contain("my-volume");
        volume.Labels.Should().ContainKey("type");
    }

    [Fact]
    public void DockerVolumeInfo_DefaultValues_AreCorrect()
    {
        // Act
        var volume = new DockerVolumeInfo { Name = "vol", Driver = "local" };

        // Assert
        volume.Mountpoint.Should().BeNull();
        volume.Labels.Should().BeEmpty();
    }
}
