using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class MeTTaAtomExtensionsTests
{
    #region WithType

    [Fact]
    public void WithType_CreatesTypeAnnotationExpression()
    {
        var atom = Atom.Sym("x");
        var type = Atom.Sym("Number");

        var result = atom.WithType(type);

        result.Should().Be(Atom.Expr(Atom.Sym(":"), Atom.Sym("x"), Atom.Sym("Number")));
    }

    [Fact]
    public void WithType_WithExpressionAtom_CreatesAnnotation()
    {
        var atom = Atom.Expr(Atom.Sym("f"), Atom.Sym("a"));
        var type = Atom.Sym("Type");

        var result = atom.WithType(type);

        result.Children.Should().HaveCount(3);
        result.Children[0].Should().Be(Atom.Sym(":"));
        result.Children[1].Should().Be(atom);
        result.Children[2].Should().Be(type);
    }

    #endregion

    #region To

    [Fact]
    public void To_CreatesFunctionTypeExpression()
    {
        var input = Atom.Sym("Number");
        var output = Atom.Sym("String");

        var result = input.To(output);

        result.Should().Be(Atom.Expr(Atom.Sym("->"), Atom.Sym("Number"), Atom.Sym("String")));
    }

    [Fact]
    public void To_WithComplexTypes_CreatesFunctionType()
    {
        var input = Atom.Sym("A");
        var output = Atom.Sym("B");

        var result = input.To(output);

        result.Children.Should().HaveCount(3);
        result.Children[0].Should().Be(Atom.Sym("->"));
    }

    #endregion

    #region Quoted

    [Fact]
    public void Quoted_CreatesQuoteExpression()
    {
        var atom = Atom.Sym("hello");

        var result = atom.Quoted();

        result.Should().Be(Atom.Expr(Atom.Sym("quote"), Atom.Sym("hello")));
    }

    [Fact]
    public void Quoted_WithExpression_WrapsInQuote()
    {
        var expr = Atom.Expr(Atom.Sym("f"), Atom.Sym("x"));

        var result = expr.Quoted();

        result.Children.Should().HaveCount(2);
        result.Children[0].Should().Be(Atom.Sym("quote"));
        result.Children[1].Should().Be(expr);
    }

    #endregion

    #region ImpliesThat

    [Fact]
    public void ImpliesThat_CreatesImplicationExpression()
    {
        var condition = Atom.Expr(Atom.Sym("Human"), Atom.Var("x"));
        var conclusion = Atom.Expr(Atom.Sym("Mortal"), Atom.Var("x"));

        var result = condition.ImpliesThat(conclusion);

        result.Should().Be(Atom.Expr(
            Atom.Sym("implies"),
            Atom.Expr(Atom.Sym("Human"), Atom.Var("x")),
            Atom.Expr(Atom.Sym("Mortal"), Atom.Var("x"))));
    }

    [Fact]
    public void ImpliesThat_WithSimpleSymbols_CreatesImplication()
    {
        var condition = Atom.Sym("A");
        var conclusion = Atom.Sym("B");

        var result = condition.ImpliesThat(conclusion);

        result.Children.Should().HaveCount(3);
        result.Children[0].Should().Be(Atom.Sym("implies"));
        result.Children[1].Should().Be(Atom.Sym("A"));
        result.Children[2].Should().Be(Atom.Sym("B"));
    }

    #endregion

    #region ToMeTTa (Form -> Atom)

    [Fact]
    public void ToMeTTa_MarkForm_ReturnsMarkSymbol()
    {
        var form = Form.Mark;

        var result = form.ToMeTTa();

        result.Should().Be(Atom.Sym("Mark"));
    }

    [Fact]
    public void ToMeTTa_VoidForm_ReturnsVoidSymbol()
    {
        var form = Form.Void;

        var result = form.ToMeTTa();

        result.Should().Be(Atom.Sym("Void"));
    }

    [Fact]
    public void ToMeTTa_ImaginaryForm_ReturnsImaginarySymbol()
    {
        var form = Form.Imaginary;

        var result = form.ToMeTTa();

        result.Should().Be(Atom.Sym("Imaginary"));
    }

    #endregion

    #region ToForm (Atom -> Form)

    [Fact]
    public void ToForm_MarkSymbol_ReturnsSomeMark()
    {
        var atom = Atom.Sym("Mark");

        var result = atom.ToForm();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(Form.Mark);
    }

    [Fact]
    public void ToForm_TrueSymbol_ReturnsSomeMark()
    {
        var atom = Atom.Sym("True");

        var result = atom.ToForm();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(Form.Mark);
    }

    [Fact]
    public void ToForm_CrossSymbol_ReturnsSomeMark()
    {
        var atom = Atom.Sym("\u2310"); // ⌐

        var result = atom.ToForm();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(Form.Mark);
    }

    [Fact]
    public void ToForm_VoidSymbol_ReturnsSomeVoid()
    {
        var atom = Atom.Sym("Void");

        var result = atom.ToForm();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(Form.Void);
    }

    [Fact]
    public void ToForm_FalseSymbol_ReturnsSomeVoid()
    {
        var atom = Atom.Sym("False");

        var result = atom.ToForm();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(Form.Void);
    }

    [Fact]
    public void ToForm_EmptySetSymbol_ReturnsSomeVoid()
    {
        var atom = Atom.Sym("\u2205"); // ∅

        var result = atom.ToForm();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(Form.Void);
    }

    [Fact]
    public void ToForm_ImaginarySymbol_ReturnsSomeImaginary()
    {
        var atom = Atom.Sym("Imaginary");

        var result = atom.ToForm();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void ToForm_ISymbol_ReturnsSomeImaginary()
    {
        var atom = Atom.Sym("i");

        var result = atom.ToForm();

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void ToForm_UnknownSymbol_ReturnsNone()
    {
        var atom = Atom.Sym("Unknown");

        var result = atom.ToForm();

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToForm_Variable_ReturnsNone()
    {
        Atom atom = Atom.Var("x");

        var result = atom.ToForm();

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToForm_Expression_ReturnsNone()
    {
        Atom atom = Atom.Expr(Atom.Sym("f"), Atom.Sym("x"));

        var result = atom.ToForm();

        result.HasValue.Should().BeFalse();
    }

    #endregion

    #region Roundtrip

    [Fact]
    public void Roundtrip_Mark_ToMeTTa_ToForm_PreservesIdentity()
    {
        var form = Form.Mark;

        var atom = form.ToMeTTa();
        var backToForm = atom.ToForm();

        backToForm.HasValue.Should().BeTrue();
        backToForm.Value.Should().Be(form);
    }

    [Fact]
    public void Roundtrip_Void_ToMeTTa_ToForm_PreservesIdentity()
    {
        var form = Form.Void;

        var atom = form.ToMeTTa();
        var backToForm = atom.ToForm();

        backToForm.HasValue.Should().BeTrue();
        backToForm.Value.Should().Be(form);
    }

    [Fact]
    public void Roundtrip_Imaginary_ToMeTTa_ToForm_PreservesIdentity()
    {
        var form = Form.Imaginary;

        var atom = form.ToMeTTa();
        var backToForm = atom.ToForm();

        backToForm.HasValue.Should().BeTrue();
        backToForm.Value.Should().Be(form);
    }

    #endregion
}
