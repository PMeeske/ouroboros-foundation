using Ouroboros.Tools;
using Moq;

namespace Ouroboros.Tools.Tests;

[Trait("Category", "Unit")]
public class ToolInfoTests
{
    [Fact]
    public void ToolInfo_ShouldBeCreatable()
    {
        typeof(ToolInfo).Should().NotBeNull();
    }
}
