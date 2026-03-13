using System.Collections.Immutable;
using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
public class AtomTests
{
    [Fact]
    public void Sym_CreatesSymbol()
    {
        var sym = Atom.Sym("hello");

        sym.Should().BeOfType<Symbol>();
        sym.Name.Should().Be("hello");
    }

    [Fact]
    public void Var_CreatesVariable()
    {
        var v = Atom.Var("x");

        v.Should().BeOfType<Variable>();
        v.Name.Should().Be("x");
    }

    [Fact]
    public void Expr_FromParams_CreatesExpression()
    {
        var expr = Atom.Expr(Atom.Sym("a"), Atom.Sym("b"));

        expr.Should().BeOfType<Expression>();
        expr.Children.Should().HaveCount(2);
    }

    [Fact]
    public void Expr_FromImmutableList_CreatesExpression()
    {
        var children = ImmutableList.Create<Atom>(Atom.Sym("x"));
        var expr = Atom.Expr(children);

        expr.Children.Should().HaveCount(1);
    }

    [Fact]
    public void Expr_FromEnumerable_CreatesExpression()
    {
        IEnumerable<Atom> children = new[] { Atom.Sym("a"), Atom.Sym("b") };
        var expr = Atom.Expr(children);

        expr.Children.Should().HaveCount(2);
    }

    [Fact]
    public void ContainsVariables_DefaultAtom_ReturnsFalse()
    {
        var sym = Atom.Sym("test");

        sym.ContainsVariables().Should().BeFalse();
    }

    [Fact]
    public void ToString_CallsToSExpr()
    {
        var sym = Atom.Sym("test");

        sym.ToString().Should().Be("test");
    }
}
