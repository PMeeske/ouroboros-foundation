using Ouroboros.Core.Hyperon;
using Ouroboros.Core.Hyperon.Parsing;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class SExpressionParserTests
{
    private readonly SExpressionParser _sut = new();

    [Fact]
    public void Parse_SimpleSymbol_ReturnsSymbol()
    {
        var result = _sut.Parse("hello");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Symbol>();
        ((Symbol)result.Value).Name.Should().Be("hello");
    }

    [Fact]
    public void Parse_Variable_ReturnsVariable()
    {
        var result = _sut.Parse("$x");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Variable>();
        ((Variable)result.Value).Name.Should().Be("x");
    }

    [Fact]
    public void Parse_SimpleExpression_ReturnsExpression()
    {
        var result = _sut.Parse("(add 1 2)");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Expression>();
        var expr = (Expression)result.Value;
        expr.Children.Should().HaveCount(3);
    }

    [Fact]
    public void Parse_NestedExpression_PreservesStructure()
    {
        var result = _sut.Parse("(f (g x) y)");

        result.IsSuccess.Should().BeTrue();
        var expr = (Expression)result.Value;
        expr.Children.Should().HaveCount(3);
        expr.Children[1].Should().BeOfType<Expression>();
    }

    [Fact]
    public void Parse_EmptyExpression_ReturnsExpressionWithNoChildren()
    {
        var result = _sut.Parse("()");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Expression>();
        ((Expression)result.Value).Children.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Parse_EmptyOrNull_ReturnsFailure(string? input)
    {
        var result = _sut.Parse(input!);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Parse_UnmatchedOpenParen_ReturnsFailure()
    {
        var result = _sut.Parse("(hello");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Parse_UnmatchedCloseParen_ReturnsFailure()
    {
        var result = _sut.Parse(")");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Parse_QuotedString_ReturnsSymbolWithContent()
    {
        var result = _sut.Parse("\"hello world\"");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Symbol>();
        ((Symbol)result.Value).Name.Should().Be("hello world");
    }

    [Fact]
    public void Parse_TrailingTokens_ReturnsFailure()
    {
        var result = _sut.Parse("hello world");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Parse_EmptyVariableName_ReturnsFailure()
    {
        var result = _sut.Parse("$");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Parse_CommentLine_IgnoresComment()
    {
        var result = _sut.Parse("; this is a comment\nhello");

        result.IsSuccess.Should().BeTrue();
        ((Symbol)result.Value).Name.Should().Be("hello");
    }

    // --- ParseMultiple ---

    [Fact]
    public void ParseMultiple_MultipleExpressions_ReturnsAll()
    {
        var result = _sut.ParseMultiple("(a b) (c d)");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public void ParseMultiple_EmptyInput_ReturnsFailure()
    {
        var result = _sut.ParseMultiple("");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ParseMultiple_SingleExpression_ReturnsSingleItem()
    {
        var result = _sut.ParseMultiple("hello");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    // --- TryParse ---

    [Fact]
    public void TryParse_ValidInput_ReturnsTrueAndSetsAtom()
    {
        bool success = _sut.TryParse("hello", out Atom? atom);

        success.Should().BeTrue();
        atom.Should().NotBeNull();
    }

    [Fact]
    public void TryParse_InvalidInput_ReturnsFalseAndSetsNull()
    {
        bool success = _sut.TryParse("", out Atom? atom);

        success.Should().BeFalse();
        atom.Should().BeNull();
    }

    // --- Edge cases ---

    [Fact]
    public void Parse_VariableInExpression_Parsed()
    {
        var result = _sut.Parse("(implies (human $x) (mortal $x))");

        result.IsSuccess.Should().BeTrue();
        var expr = (Expression)result.Value;
        expr.Children.Should().HaveCount(3);
    }

    [Fact]
    public void Parse_DeeplyNested_Succeeds()
    {
        var result = _sut.Parse("(a (b (c (d e))))");

        result.IsSuccess.Should().BeTrue();
    }
}
