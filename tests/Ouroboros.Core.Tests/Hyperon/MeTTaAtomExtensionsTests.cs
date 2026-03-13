using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using LoF = Ouroboros.Core.LawsOfForm.Form;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
public class MeTTaAtomExtensionsTests
{
    [Fact]
    public void WithType_CreatesTypeAnnotation()
    {
        var atom = Atom.Sym("Socrates");
        var type = Atom.Sym("Human");

        var result = atom.WithType(type);

        result.Should().BeOfType<Expression>();
        result.ToSExpr().Should().Contain(":");
    }

    [Fact]
    public void To_CreatesFunctionType()
    {
        var input = Atom.Sym("Int");
        var output = Atom.Sym("Bool");

        var result = input.To(output);

        result.Should().BeOfType<Expression>();
        result.ToSExpr().Should().Contain("->");
    }

    [Fact]
    public void Quoted_CreatesQuotedExpression()
    {
        var atom = Atom.Sym("test");

        var result = atom.Quoted();

        result.Should().BeOfType<Expression>();
        result.ToSExpr().Should().Contain("quote");
    }

    [Fact]
    public void ImpliesThat_CreatesImplication()
    {
        var condition = Atom.Expr(Atom.Sym("Human"), Atom.Var("x"));
        var conclusion = Atom.Expr(Atom.Sym("Mortal"), Atom.Var("x"));

        var result = condition.ImpliesThat(conclusion);

        result.Should().BeOfType<Expression>();
        result.ToSExpr().Should().Contain("=");
    }

    [Fact]
    public void ToMeTTa_Mark_ReturnsMarkSymbol()
    {
        var result = LoF.Mark.ToMeTTa();

        result.Should().BeOfType<Symbol>();
        result.ToSExpr().Should().Be("Mark");
    }

    [Fact]
    public void ToMeTTa_Void_ReturnsVoidSymbol()
    {
        var result = LoF.Void.ToMeTTa();

        result.Should().BeOfType<Symbol>();
        result.ToSExpr().Should().Be("Void");
    }

    [Fact]
    public void ToMeTTa_Imaginary_ReturnsImaginarySymbol()
    {
        var result = LoF.Imaginary.ToMeTTa();

        result.Should().BeOfType<Symbol>();
        result.ToSExpr().Should().Be("Imaginary");
    }

    [Theory]
    [InlineData("Mark")]
    [InlineData("True")]
    [InlineData("⌐")]
    public void ToForm_MarkSymbols_ReturnsMark(string name)
    {
        var atom = Atom.Sym(name);

        var result = atom.ToForm();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(LoF.Mark);
    }

    [Theory]
    [InlineData("Void")]
    [InlineData("False")]
    [InlineData("∅")]
    public void ToForm_VoidSymbols_ReturnsVoid(string name)
    {
        var atom = Atom.Sym(name);

        var result = atom.ToForm();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(LoF.Void);
    }

    [Theory]
    [InlineData("Imaginary")]
    [InlineData("i")]
    public void ToForm_ImaginarySymbols_ReturnsImaginary(string name)
    {
        var atom = Atom.Sym(name);

        var result = atom.ToForm();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(LoF.Imaginary);
    }

    [Fact]
    public void ToForm_UnknownSymbol_ReturnsNone()
    {
        var atom = Atom.Sym("unknown");

        var result = atom.ToForm();

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToForm_NonSymbolAtom_ReturnsNone()
    {
        var atom = Atom.Var("x");

        var result = atom.ToForm();

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToForm_Expression_ReturnsNone()
    {
        var atom = Atom.Expr(Atom.Sym("a"));

        var result = atom.ToForm();

        result.HasValue.Should().BeFalse();
    }
}
