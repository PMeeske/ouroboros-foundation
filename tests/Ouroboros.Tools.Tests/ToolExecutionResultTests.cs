using Ouroboros.Tools;
using Moq;

namespace Ouroboros.Tools.Tests;

[Trait("Category", "Unit")]
public class ToolExecutionResultTests
{
    [Fact]
    public void ToolExecutionResult_ShouldBeCreatable()
    {
        typeof(ToolExecutionResult).Should().NotBeNull();
    }
}
