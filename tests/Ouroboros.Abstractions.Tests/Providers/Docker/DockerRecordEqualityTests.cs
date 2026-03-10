using Ouroboros.Providers.Docker;

namespace Ouroboros.Abstractions.Tests.Providers.Docker;

/// <summary>
/// Additional tests for Docker record types covering edge cases
/// and property behaviors not tested in DockerRecordTests.
/// </summary>
[Trait("Category", "Unit")]
public class DockerRecordEqualityTests
{
    [Fact]
    public void DockerContainerInfo_ShortId_NullId_ReturnsNull()
    {
        // Arrange
        var container = new DockerContainerInfo
        {
            Id = null!,
            Image = "nginx",
            State = "running"
        };

        // Act & Assert
        container.ShortId.Should().BeNull();
    }

    [Fact]
    public void DockerContainerInfo_ShortId_EmptyId_ReturnsEmpty()
    {
        // Arrange
        var container = new DockerContainerInfo
        {
            Id = "",
            Image = "nginx",
            State = "running"
        };

        // Act & Assert
        container.ShortId.Should().Be("");
    }

    [Fact]
    public void DockerContainerInfo_ShortId_ExactlyTwelveChars_ReturnsAll()
    {
        // Arrange
        var container = new DockerContainerInfo
        {
            Id = "abcdef123456",
            Image = "nginx",
            State = "running"
        };

        // Act & Assert
        container.ShortId.Should().Be("abcdef123456");
    }

    [Fact]
    public void DockerPortMapping_DefaultValues()
    {
        // Act
        var mapping = new DockerPortMapping();

        // Assert
        mapping.HostIp.Should().BeNull();
        mapping.HostPort.Should().BeNull();
        mapping.ContainerPort.Should().Be(0);
        mapping.Protocol.Should().Be("tcp");
    }

    [Fact]
    public void DockerImageInfo_SizeInMB_CanBeCalculated()
    {
        // Arrange
        var image = new DockerImageInfo
        {
            Id = "sha256:abc",
            Size = 104857600 // 100 MB in bytes
        };

        // Act
        var sizeInMb = image.Size / (1024.0 * 1024.0);

        // Assert
        sizeInMb.Should().Be(100.0);
    }

    [Fact]
    public void DockerNetworkInfo_AllDefaultValues()
    {
        // Act
        var network = new DockerNetworkInfo
        {
            Id = "id",
            Name = "name",
            Driver = "bridge"
        };

        // Assert
        network.Scope.Should().BeNull();
    }

    [Fact]
    public void DockerVolumeInfo_WithLabels_LabelsAreAccessible()
    {
        // Arrange
        var volume = new DockerVolumeInfo
        {
            Name = "data-vol",
            Driver = "local",
            Labels = new Dictionary<string, string>
            {
                ["env"] = "prod",
                ["team"] = "backend"
            }
        };

        // Assert
        volume.Labels.Should().HaveCount(2);
        volume.Labels["env"].Should().Be("prod");
    }
}
