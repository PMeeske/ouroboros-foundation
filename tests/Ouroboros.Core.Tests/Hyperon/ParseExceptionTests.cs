using Ouroboros.Core.Hyperon.Parsing;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class ParseExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var ex = new ParseException("unexpected token");

        ex.Message.Should().Be("unexpected token");
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsBoth()
    {
        var inner = new InvalidOperationException("inner error");
        var ex = new ParseException("parse failed", inner);

        ex.Message.Should().Be("parse failed");
        ex.InnerException.Should().BeSameAs(inner);
    }

    [Fact]
    public void InheritsFromException()
    {
        var ex = new ParseException("test");

        ex.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void CanBeCaughtAsException()
    {
        Action act = () => throw new ParseException("test error");

        act.Should().Throw<ParseException>()
            .WithMessage("test error");
    }

    [Fact]
    public void CanBeCaughtAsBaseException()
    {
        Action act = () => throw new ParseException("test error");

        act.Should().Throw<Exception>()
            .WithMessage("test error");
    }

    [Fact]
    public void InnerException_DefaultsToNull_WhenNotProvided()
    {
        var ex = new ParseException("msg");

        ex.InnerException.Should().BeNull();
    }
}
