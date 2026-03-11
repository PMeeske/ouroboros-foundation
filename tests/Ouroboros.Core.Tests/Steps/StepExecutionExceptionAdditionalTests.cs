namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public sealed class StepExecutionExceptionAdditionalTests
{
    [Fact]
    public void Constructor_WithAllParameters_NullInnerException_Works()
    {
        var ex = new StepExecutionException(typeof(string), "input", "message", null);

        ex.StepType.Should().Be(typeof(string));
        ex.InputValue.Should().Be("input");
        ex.InnerException.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShortForm_NullStepType_ThrowsArgumentNullException()
    {
        var act = () => new StepExecutionException(null!, "input");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Message_ShortForm_ContainsDefaultFormat()
    {
        var ex = new StepExecutionException(typeof(string), "test input");

        ex.Message.Should().Contain("Step execution failed");
        ex.Message.Should().Contain("String");
        ex.Message.Should().Contain("test input");
    }

    [Fact]
    public void IsException_DerivedFromException()
    {
        var ex = new StepExecutionException(typeof(int), 42, "msg");

        ex.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void Message_WithNullInput_HandlesGracefully()
    {
        var ex = new StepExecutionException(typeof(int), null, "message");

        ex.Message.Should().Contain("Int32");
        ex.Message.Should().Contain("message");
    }
}
