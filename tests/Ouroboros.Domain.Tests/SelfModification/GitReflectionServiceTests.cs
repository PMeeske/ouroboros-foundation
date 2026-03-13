using Ouroboros.Domain.SelfModification;

namespace Ouroboros.Tests.SelfModification;

[Trait("Category", "Unit")]
public class GitReflectionServiceTests
{
    [Fact]
    public void SafeModificationPaths_ShouldContainExpectedPaths()
    {
        GitReflectionService.SafeModificationPaths.Should().Contain("src/Ouroboros.Domain");
        GitReflectionService.SafeModificationPaths.Should().Contain("src/Ouroboros.Tools");
        GitReflectionService.SafeModificationPaths.Should().Contain("docs");
        GitReflectionService.SafeModificationPaths.Should().Contain("examples");
    }

    [Fact]
    public void ImmutablePaths_ShouldContainEthicsPath()
    {
        GitReflectionService.ImmutablePaths.Should().Contain("src/Ouroboros.Core/Ethics/");
    }

    [Fact]
    public void ImmutablePaths_ShouldContainConstitutionPath()
    {
        GitReflectionService.ImmutablePaths.Should().Contain("constitution/");
    }

    [Fact]
    public void ImmutablePaths_ShouldContainSelfReference()
    {
        GitReflectionService.ImmutablePaths.Should().Contain(
            "src/Ouroboros.Domain/Domain/SelfModification/GitReflectionService.cs");
    }

    [Fact]
    public void AllowedExtensions_ShouldContainCSharp()
    {
        GitReflectionService.AllowedExtensions.Should().Contain(".cs");
    }

    [Fact]
    public void AllowedExtensions_ShouldContainCommonFormats()
    {
        GitReflectionService.AllowedExtensions.Should().Contain(".json");
        GitReflectionService.AllowedExtensions.Should().Contain(".md");
        GitReflectionService.AllowedExtensions.Should().Contain(".yaml");
        GitReflectionService.AllowedExtensions.Should().Contain(".yml");
        GitReflectionService.AllowedExtensions.Should().Contain(".xml");
    }

    [Fact]
    public void Constructor_ShouldNotThrow()
    {
        using var service = new GitReflectionService("/tmp/fake-repo");

        service.Should().NotBeNull();
    }

    [Fact]
    public void Proposals_Initially_ShouldBeEmpty()
    {
        using var service = new GitReflectionService("/tmp/fake-repo");

        service.Proposals.Should().BeEmpty();
    }

    [Fact]
    public void Dispose_MultipleTimes_ShouldNotThrow()
    {
        var service = new GitReflectionService("/tmp/fake-repo");
        service.Dispose();

        var act = () => service.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void GitCommitInfo_Constructor_ShouldSetProperties()
    {
        var timestamp = DateTime.UtcNow;
        var info = new GitCommitInfo("abc123", "feat: add feature", timestamp);

        info.Hash.Should().Be("abc123");
        info.Message.Should().Be("feat: add feature");
        info.Timestamp.Should().Be(timestamp);
    }
}
