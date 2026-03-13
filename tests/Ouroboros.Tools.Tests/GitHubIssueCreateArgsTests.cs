using Ouroboros.Tools;
using Moq;

namespace Ouroboros.Tools.Tests;

[Trait("Category", "Unit")]
public class GitHubIssueCreateArgsTests
{
    [Fact]
    public void GitHubIssueCreateArgs_ShouldBeCreatable()
    {
        typeof(GitHubIssueCreateArgs).Should().NotBeNull();
    }
}
