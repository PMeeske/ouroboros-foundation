using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public class StepExecutionExceptionTests
{
    [Fact]
    public void Constructor_WithAllParameters_SetsProperties()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new StepExecutionException(typeof(string), "input", "test message", inner);

        ex.StepType.Should().Be(typeof(string));
        ex.InputValue.Should().Be("input");
        ex.InnerException.Should().Be(inner);
    }

    [Fact]
    public void Constructor_WithNullStepType_ThrowsArgumentNullException()
    {
        var act = () => new StepExecutionException(null!, "input", "message");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullInputValue_SetsInputToNull()
    {
        var ex = new StepExecutionException(typeof(int), null, "message");
        ex.InputValue.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShortForm_GeneratesDefaultMessage()
    {
        var inner = new Exception("root cause");
        var ex = new StepExecutionException(typeof(string), "test input", inner);

        ex.StepType.Should().Be(typeof(string));
        ex.InputValue.Should().Be("test input");
        ex.InnerException.Should().Be(inner);
    }

    [Fact]
    public void Constructor_ShortForm_WithoutInnerException_Works()
    {
        var ex = new StepExecutionException(typeof(int), 42);

        ex.StepType.Should().Be(typeof(int));
        ex.InputValue.Should().Be(42);
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void Message_ContainsStepTypeAndInput()
    {
        var ex = new StepExecutionException(typeof(string), "test input", "custom msg");

        ex.Message.Should().Contain("String");
        ex.Message.Should().Contain("custom msg");
        ex.Message.Should().Contain("test input");
    }

    [Fact]
    public void Message_ShortForm_ContainsStepTypeAndInput()
    {
        var ex = new StepExecutionException(typeof(int), 42);

        ex.Message.Should().Contain("Int32");
        ex.Message.Should().Contain("42");
    }
}
