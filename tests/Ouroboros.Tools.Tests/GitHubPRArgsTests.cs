using Ouroboros.Tools;
using Moq;

namespace Ouroboros.Tools.Tests;

[Trait("Category", "Unit")]
public class GitHubPRArgsTests
{
    [Fact]
    public void GitHubPRArgs_ShouldBeCreatable()
    {
        typeof(GitHubPRArgs).Should().NotBeNull();
    }
}
