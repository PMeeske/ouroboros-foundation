using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class StandardOperationsAdditionalTests
{
    private readonly AtomSpace _space = new();

    #region Retract with Non-Mutable Space

    [Fact]
    public void Retract_WithNonMutableSpace_ReturnsEmpty()
    {
        var mockSpace = new Mock<IAtomSpace>();
        var args = Atom.Expr(Atom.Sym("retract"), Atom.Sym("fact"));

        var results = StandardOperations.Retract(mockSpace.Object, args).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region And - Edge Cases

    [Fact]
    public void And_SingleConjunct_ReturnsTrue_WhenPresent()
    {
        _space.Add(Atom.Sym("a"));

        var args = Atom.Expr(Atom.Sym("and"), Atom.Sym("a"));

        var results = StandardOperations.And(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(Atom.Sym("True"));
    }

    [Fact]
    public void And_ThreeConjuncts_AllPresent_ReturnsTrue()
    {
        _space.Add(Atom.Sym("a"));
        _space.Add(Atom.Sym("b"));
        _space.Add(Atom.Sym("c"));

        var args = Atom.Expr(Atom.Sym("and"), Atom.Sym("a"), Atom.Sym("b"), Atom.Sym("c"));

        var results = StandardOperations.And(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(Atom.Sym("True"));
    }

    [Fact]
    public void And_ThreeConjuncts_OneMissing_ReturnsEmpty()
    {
        _space.Add(Atom.Sym("a"));
        _space.Add(Atom.Sym("c"));

        var args = Atom.Expr(Atom.Sym("and"), Atom.Sym("a"), Atom.Sym("b"), Atom.Sym("c"));

        var results = StandardOperations.And(_space, args).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region Or - Edge Cases

    [Fact]
    public void Or_SingleDisjunct_Present_ReturnsTrue()
    {
        _space.Add(Atom.Sym("a"));

        var args = Atom.Expr(Atom.Sym("or"), Atom.Sym("a"));

        var results = StandardOperations.Or(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(Atom.Sym("True"));
    }

    [Fact]
    public void Or_MultipleDisjuncts_LastPresent_ReturnsTrue()
    {
        _space.Add(Atom.Sym("c"));

        var args = Atom.Expr(Atom.Sym("or"), Atom.Sym("a"), Atom.Sym("b"), Atom.Sym("c"));

        var results = StandardOperations.Or(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(Atom.Sym("True"));
    }

    #endregion

    #region Implies with Patterns

    [Fact]
    public void Implies_WithExpressionPattern_AppliesBindingsToConclusion()
    {
        _space.Add(Atom.Expr(Atom.Sym("parent"), Atom.Sym("Alice"), Atom.Sym("Bob")));

        var args = Atom.Expr(
            Atom.Sym("implies"),
            Atom.Expr(Atom.Sym("parent"), Atom.Var("p"), Atom.Var("c")),
            Atom.Expr(Atom.Sym("child"), Atom.Var("c"), Atom.Var("p")));

        var results = StandardOperations.Implies(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(Atom.Expr(Atom.Sym("child"), Atom.Sym("Bob"), Atom.Sym("Alice")));
    }

    #endregion

    #region Equal - Expression Equality

    [Fact]
    public void Equal_SameExpressions_ReturnsTrue()
    {
        var a = Atom.Expr(Atom.Sym("f"), Atom.Sym("x"));
        var b = Atom.Expr(Atom.Sym("f"), Atom.Sym("x"));
        var args = Atom.Expr(Atom.Sym("equal"), a, b);

        var results = StandardOperations.Equal(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(Atom.Sym("True"));
    }

    [Fact]
    public void Equal_DifferentExpressions_ReturnsEmpty()
    {
        var a = Atom.Expr(Atom.Sym("f"), Atom.Sym("x"));
        var b = Atom.Expr(Atom.Sym("f"), Atom.Sym("y"));
        var args = Atom.Expr(Atom.Sym("equal"), a, b);

        var results = StandardOperations.Equal(_space, args).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region Match with Pattern Variables

    [Fact]
    public void Match_WithPatternVariables_ReturnsMatchingAtoms()
    {
        _space.Add(Atom.Expr(Atom.Sym("color"), Atom.Sym("red")));
        _space.Add(Atom.Expr(Atom.Sym("color"), Atom.Sym("blue")));
        _space.Add(Atom.Expr(Atom.Sym("size"), Atom.Sym("large")));

        var args = Atom.Expr(
            Atom.Sym("match"),
            Atom.Expr(Atom.Sym("color"), Atom.Var("c")));

        var results = StandardOperations.Match(_space, args).ToList();

        results.Should().HaveCount(2);
    }

    #endregion

    #region Quote - Various Types

    [Fact]
    public void Quote_Symbol_ReturnsSymbol()
    {
        var sym = Atom.Sym("test");
        var args = Atom.Expr(Atom.Sym("quote"), sym);

        var results = StandardOperations.Quote(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(sym);
    }

    [Fact]
    public void Quote_Variable_ReturnsVariable()
    {
        var v = Atom.Var("x");
        var args = Atom.Expr(Atom.Sym("quote"), v);

        var results = StandardOperations.Quote(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(v);
    }

    #endregion

    #region Assert - Expression Atoms

    [Fact]
    public void Assert_Expression_AddsExpressionToSpace()
    {
        var expr = Atom.Expr(Atom.Sym("fact"), Atom.Sym("value"));
        var args = Atom.Expr(Atom.Sym("assert"), expr);

        var results = StandardOperations.Assert(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(expr);
        _space.Contains(expr).Should().BeTrue();
    }

    #endregion

    #region Not - Expression Match

    [Fact]
    public void Not_WhenExpressionNotInSpace_ReturnsTrue()
    {
        var args = Atom.Expr(Atom.Sym("not"),
            Atom.Expr(Atom.Sym("nonexistent"), Atom.Sym("x")));

        var results = StandardOperations.Not(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(Atom.Sym("True"));
    }

    [Fact]
    public void Not_WhenExpressionInSpace_ReturnsEmpty()
    {
        _space.Add(Atom.Expr(Atom.Sym("exists"), Atom.Sym("x")));

        var args = Atom.Expr(Atom.Sym("not"),
            Atom.Expr(Atom.Sym("exists"), Atom.Sym("x")));

        var results = StandardOperations.Not(_space, args).ToList();

        results.Should().BeEmpty();
    }

    #endregion
}
