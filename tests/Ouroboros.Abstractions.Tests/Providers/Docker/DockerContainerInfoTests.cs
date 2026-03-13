namespace Ouroboros.Abstractions.Tests.Providers.Docker;

using Ouroboros.Providers.Docker;

[Trait("Category", "Unit")]
public class DockerContainerInfoTests
{
    [Fact]
    public void Constructor_RequiredProperties_Set()
    {
        var info = new DockerContainerInfo
        {
            Id = "abc123def456ghi789",
            Image = "nginx:latest",
            State = "running",
        };

        info.Id.Should().Be("abc123def456ghi789");
        info.Image.Should().Be("nginx:latest");
        info.State.Should().Be("running");
    }

    [Fact]
    public void ShortId_TruncatesTo12Chars()
    {
        var info = new DockerContainerInfo
        {
            Id = "abc123def456ghi789",
            Image = "test",
            State = "running",
        };

        info.ShortId.Should().Be("abc123def456");
    }

    [Fact]
    public void ShortId_ShortId_ReturnsAsIs()
    {
        var info = new DockerContainerInfo
        {
            Id = "short",
            Image = "test",
            State = "running",
        };

        info.ShortId.Should().Be("short");
    }

    [Fact]
    public void Names_DefaultsToEmpty()
    {
        var info = new DockerContainerInfo { Id = "id", Image = "img", State = "s" };
        info.Names.Should().BeEmpty();
    }

    [Fact]
    public void Status_DefaultsToNull()
    {
        var info = new DockerContainerInfo { Id = "id", Image = "img", State = "s" };
        info.Status.Should().BeNull();
    }

    [Fact]
    public void Ports_DefaultsToEmpty()
    {
        var info = new DockerContainerInfo { Id = "id", Image = "img", State = "s" };
        info.Ports.Should().BeEmpty();
    }

    [Fact]
    public void Labels_DefaultsToEmpty()
    {
        var info = new DockerContainerInfo { Id = "id", Image = "img", State = "s" };
        info.Labels.Should().BeEmpty();
    }

    [Fact]
    public void CreatedAt_DefaultsToNull()
    {
        var info = new DockerContainerInfo { Id = "id", Image = "img", State = "s" };
        info.CreatedAt.Should().BeNull();
    }
}
