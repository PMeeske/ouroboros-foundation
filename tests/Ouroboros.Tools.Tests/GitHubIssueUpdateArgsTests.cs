using Ouroboros.Tools;
using Moq;

namespace Ouroboros.Tools.Tests;

[Trait("Category", "Unit")]
public class GitHubIssueUpdateArgsTests
{
    [Fact]
    public void GitHubIssueUpdateArgs_ShouldBeCreatable()
    {
        typeof(GitHubIssueUpdateArgs).Should().NotBeNull();
    }
}
