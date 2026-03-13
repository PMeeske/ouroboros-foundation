using System.Collections.Immutable;
using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
public class ExpressionTests
{
    [Fact]
    public void ToSExpr_EmptyExpression_ReturnsEmptyParens()
    {
        var sut = new Expression(ImmutableList<Atom>.Empty);

        sut.ToSExpr().Should().Be("()");
    }

    [Fact]
    public void ToSExpr_SingleChild_FormatsCorrectly()
    {
        var sut = Atom.Expr(Atom.Sym("hello"));

        sut.ToSExpr().Should().Be("(hello)");
    }

    [Fact]
    public void ToSExpr_MultipleChildren_FormatsWithSpaces()
    {
        var sut = Atom.Expr(Atom.Sym("Human"), Atom.Sym("Socrates"));

        sut.ToSExpr().Should().Be("(Human Socrates)");
    }

    [Fact]
    public void ToSExpr_NestedExpressions_FormatsCorrectly()
    {
        var inner = Atom.Expr(Atom.Sym("Human"), Atom.Var("x"));
        var sut = Atom.Expr(Atom.Sym("implies"), inner);

        sut.ToSExpr().Should().Be("(implies (Human $x))");
    }

    [Fact]
    public void ContainsVariables_NoVariables_ReturnsFalse()
    {
        var sut = Atom.Expr(Atom.Sym("a"), Atom.Sym("b"));

        sut.ContainsVariables().Should().BeFalse();
    }

    [Fact]
    public void ContainsVariables_WithVariable_ReturnsTrue()
    {
        var sut = Atom.Expr(Atom.Sym("a"), Atom.Var("x"));

        sut.ContainsVariables().Should().BeTrue();
    }

    [Fact]
    public void ContainsVariables_NestedVariable_ReturnsTrue()
    {
        var inner = Atom.Expr(Atom.Var("x"));
        var sut = Atom.Expr(Atom.Sym("a"), inner);

        sut.ContainsVariables().Should().BeTrue();
    }

    [Fact]
    public void Head_NonEmpty_ReturnsSome()
    {
        var sut = Atom.Expr(Atom.Sym("head"), Atom.Sym("tail"));

        var head = sut.Head();

        head.HasValue.Should().BeTrue();
        head.Value.Should().Be(Atom.Sym("head"));
    }

    [Fact]
    public void Head_Empty_ReturnsNone()
    {
        var sut = new Expression(ImmutableList<Atom>.Empty);

        sut.Head().HasValue.Should().BeFalse();
    }

    [Fact]
    public void Tail_NonEmpty_ReturnsRestOfChildren()
    {
        var sut = Atom.Expr(Atom.Sym("a"), Atom.Sym("b"), Atom.Sym("c"));

        var tail = sut.Tail();

        tail.Should().HaveCount(2);
        tail[0].Should().Be(Atom.Sym("b"));
        tail[1].Should().Be(Atom.Sym("c"));
    }

    [Fact]
    public void Tail_Empty_ReturnsEmptyList()
    {
        var sut = new Expression(ImmutableList<Atom>.Empty);

        sut.Tail().Should().BeEmpty();
    }

    [Fact]
    public void Equals_SameStructure_AreEqual()
    {
        var a = Atom.Expr(Atom.Sym("x"), Atom.Sym("y"));
        var b = Atom.Expr(Atom.Sym("x"), Atom.Sym("y"));

        a.Should().Be(b);
    }

    [Fact]
    public void Equals_DifferentStructure_AreNotEqual()
    {
        var a = Atom.Expr(Atom.Sym("x"), Atom.Sym("y"));
        var b = Atom.Expr(Atom.Sym("x"), Atom.Sym("z"));

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equals_DifferentCount_AreNotEqual()
    {
        var a = Atom.Expr(Atom.Sym("x"));
        var b = Atom.Expr(Atom.Sym("x"), Atom.Sym("y"));

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var a = Atom.Expr(Atom.Sym("x"));

        a.Equals(null as Expression).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameStructure_SameHash()
    {
        var a = Atom.Expr(Atom.Sym("x"), Atom.Sym("y"));
        var b = Atom.Expr(Atom.Sym("x"), Atom.Sym("y"));

        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
