namespace Ouroboros.Tests;

using Ouroboros.Tools;

[Trait("Category", "Unit")]
public class GitHubToolExtensionsTests
{
    [Fact]
    public void WithGitHubTools_RegistersAllGitHubTools()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act
        var withGitHub = registry.WithGitHubTools("fake-token", "owner", "repo");

        // Assert - 8 GitHub tools
        withGitHub.Contains("github_read_issue").Should().BeTrue();
        withGitHub.Contains("github_create_issue").Should().BeTrue();
        withGitHub.Contains("github_update_issue").Should().BeTrue();
        withGitHub.Contains("github_pr").Should().BeTrue();
        withGitHub.Contains("github_comment").Should().BeTrue();
        withGitHub.Contains("github_label").Should().BeTrue();
        withGitHub.Contains("github_search").Should().BeTrue();
        withGitHub.Contains("github_scope_lock").Should().BeTrue();
    }

    [Fact]
    public void WithGitHubTools_DoesNotMutateOriginal()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act
        var withGitHub = registry.WithGitHubTools("token", "owner", "repo");

        // Assert
        registry.Count.Should().Be(0);
        withGitHub.Count.Should().Be(8);
    }

    [Fact]
    public void WithGitHubTools_CanChainWithOtherTools()
    {
        // Arrange
        var registry = new ToolRegistry()
            .WithTool(new MathTool())
            .WithGitHubTools("token", "owner", "repo");

        // Assert
        registry.Count.Should().Be(9); // 1 math + 8 github
        registry.Contains("math").Should().BeTrue();
        registry.Contains("github_read_issue").Should().BeTrue();
    }
}
