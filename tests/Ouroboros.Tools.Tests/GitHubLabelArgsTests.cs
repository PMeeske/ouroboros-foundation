using Ouroboros.Tools;
using Moq;

namespace Ouroboros.Tools.Tests;

[Trait("Category", "Unit")]
public class GitHubLabelArgsTests
{
    [Fact]
    public void GitHubLabelArgs_ShouldBeCreatable()
    {
        typeof(GitHubLabelArgs).Should().NotBeNull();
    }
}
