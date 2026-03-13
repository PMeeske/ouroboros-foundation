using Ouroboros.Tools;
using Moq;

namespace Ouroboros.Tools.Tests;

[Trait("Category", "Unit")]
public class GitHubSearchArgsTests
{
    [Fact]
    public void GitHubSearchArgs_ShouldBeCreatable()
    {
        typeof(GitHubSearchArgs).Should().NotBeNull();
    }
}
