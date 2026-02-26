using FluentAssertions;
using Ouroboros.Core.Configuration;
using Xunit;

namespace Ouroboros.Tests.Configuration;

[Trait("Category", "Unit")]
public class ExecutionConfigurationTests
{
    [Fact]
    public void Default_MaxTurns_ShouldBe5()
    {
        var config = new ExecutionConfiguration();
        config.MaxTurns.Should().Be(5);
    }

    [Fact]
    public void Default_MaxParallelToolExecutions_ShouldBe5()
    {
        var config = new ExecutionConfiguration();
        config.MaxParallelToolExecutions.Should().Be(5);
    }

    [Fact]
    public void Default_EnableDebugOutput_ShouldBeFalse()
    {
        var config = new ExecutionConfiguration();
        config.EnableDebugOutput.Should().BeFalse();
    }

    [Fact]
    public void Default_ToolExecutionTimeoutSeconds_ShouldBe60()
    {
        var config = new ExecutionConfiguration();
        config.ToolExecutionTimeoutSeconds.Should().Be(60);
    }

    [Fact]
    public void SetMaxTurns_ShouldPersist()
    {
        var config = new ExecutionConfiguration { MaxTurns = 10 };
        config.MaxTurns.Should().Be(10);
    }

    [Fact]
    public void SetEnableDebugOutput_ShouldPersist()
    {
        var config = new ExecutionConfiguration { EnableDebugOutput = true };
        config.EnableDebugOutput.Should().BeTrue();
    }

    [Fact]
    public void SetToolExecutionTimeoutSeconds_ShouldPersist()
    {
        var config = new ExecutionConfiguration { ToolExecutionTimeoutSeconds = 120 };
        config.ToolExecutionTimeoutSeconds.Should().Be(120);
    }
}
