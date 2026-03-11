using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class StandardOperationsTests
{
    private readonly AtomSpace _space = new();

    #region RegisterAll

    [Fact]
    public void RegisterAll_RegistersAllStandardOperations()
    {
        var registry = new GroundedRegistry();

        StandardOperations.RegisterAll(registry);

        registry.Contains("implies").Should().BeTrue();
        registry.Contains("equal").Should().BeTrue();
        registry.Contains("not").Should().BeTrue();
        registry.Contains("and").Should().BeTrue();
        registry.Contains("or").Should().BeTrue();
        registry.Contains("assert").Should().BeTrue();
        registry.Contains("retract").Should().BeTrue();
        registry.Contains("match").Should().BeTrue();
        registry.Contains("quote").Should().BeTrue();
    }

    #endregion

    #region Implies

    [Fact]
    public void Implies_WhenConditionMatchesFact_DeriveConclusion()
    {
        _space.Add(Atom.Expr(Atom.Sym("Human"), Atom.Sym("Socrates")));

        var args = Atom.Expr(
            Atom.Sym("implies"),
            Atom.Expr(Atom.Sym("Human"), Atom.Var("x")),
            Atom.Expr(Atom.Sym("Mortal"), Atom.Var("x")));

        var results = StandardOperations.Implies(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(Atom.Expr(Atom.Sym("Mortal"), Atom.Sym("Socrates")));
    }

    [Fact]
    public void Implies_WhenNoConditionMatches_YieldsNothing()
    {
        var args = Atom.Expr(
            Atom.Sym("implies"),
            Atom.Expr(Atom.Sym("Human"), Atom.Var("x")),
            Atom.Expr(Atom.Sym("Mortal"), Atom.Var("x")));

        var results = StandardOperations.Implies(_space, args).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Implies_TooFewChildren_YieldsNothing()
    {
        var args = Atom.Expr(Atom.Sym("implies"), Atom.Sym("only_one"));

        var results = StandardOperations.Implies(_space, args).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Implies_MultipleMatches_YieldsMultipleConclusions()
    {
        _space.Add(Atom.Expr(Atom.Sym("Human"), Atom.Sym("Socrates")));
        _space.Add(Atom.Expr(Atom.Sym("Human"), Atom.Sym("Plato")));

        var args = Atom.Expr(
            Atom.Sym("implies"),
            Atom.Expr(Atom.Sym("Human"), Atom.Var("x")),
            Atom.Expr(Atom.Sym("Mortal"), Atom.Var("x")));

        var results = StandardOperations.Implies(_space, args).ToList();

        results.Should().HaveCount(2);
    }

    #endregion

    #region Equal

    [Fact]
    public void Equal_SameAtoms_ReturnsTrue()
    {
        var args = Atom.Expr(Atom.Sym("equal"), Atom.Sym("a"), Atom.Sym("a"));

        var results = StandardOperations.Equal(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(Atom.Sym("True"));
    }

    [Fact]
    public void Equal_DifferentAtoms_ReturnsEmpty()
    {
        var args = Atom.Expr(Atom.Sym("equal"), Atom.Sym("a"), Atom.Sym("b"));

        var results = StandardOperations.Equal(_space, args).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Equal_TooFewChildren_ReturnsEmpty()
    {
        var args = Atom.Expr(Atom.Sym("equal"), Atom.Sym("a"));

        var results = StandardOperations.Equal(_space, args).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region Not

    [Fact]
    public void Not_WhenExprNotInSpace_ReturnsTrue()
    {
        var args = Atom.Expr(Atom.Sym("not"), Atom.Sym("missing"));

        var results = StandardOperations.Not(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(Atom.Sym("True"));
    }

    [Fact]
    public void Not_WhenExprInSpace_ReturnsEmpty()
    {
        _space.Add(Atom.Sym("present"));

        var args = Atom.Expr(Atom.Sym("not"), Atom.Sym("present"));

        var results = StandardOperations.Not(_space, args).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Not_TooFewChildren_ReturnsEmpty()
    {
        var args = Atom.Expr(Atom.Sym("not"));

        var results = StandardOperations.Not(_space, args).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region And

    [Fact]
    public void And_AllConjunctsMatch_ReturnsTrue()
    {
        _space.Add(Atom.Sym("a"));
        _space.Add(Atom.Sym("b"));

        var args = Atom.Expr(Atom.Sym("and"), Atom.Sym("a"), Atom.Sym("b"));

        var results = StandardOperations.And(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(Atom.Sym("True"));
    }

    [Fact]
    public void And_SomeConjunctMissing_ReturnsEmpty()
    {
        _space.Add(Atom.Sym("a"));

        var args = Atom.Expr(Atom.Sym("and"), Atom.Sym("a"), Atom.Sym("b"));

        var results = StandardOperations.And(_space, args).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void And_TooFewChildren_ReturnsEmpty()
    {
        var args = Atom.Expr(Atom.Sym("and"));

        var results = StandardOperations.And(_space, args).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region Or

    [Fact]
    public void Or_AtLeastOneMatches_ReturnsTrue()
    {
        _space.Add(Atom.Sym("a"));

        var args = Atom.Expr(Atom.Sym("or"), Atom.Sym("a"), Atom.Sym("b"));

        var results = StandardOperations.Or(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(Atom.Sym("True"));
    }

    [Fact]
    public void Or_NoneMatch_ReturnsEmpty()
    {
        var args = Atom.Expr(Atom.Sym("or"), Atom.Sym("a"), Atom.Sym("b"));

        var results = StandardOperations.Or(_space, args).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Or_TooFewChildren_ReturnsEmpty()
    {
        var args = Atom.Expr(Atom.Sym("or"));

        var results = StandardOperations.Or(_space, args).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region Assert

    [Fact]
    public void Assert_AddsAtomToSpace_ReturnsAtom()
    {
        var atom = Atom.Sym("newFact");
        var args = Atom.Expr(Atom.Sym("assert"), atom);

        var results = StandardOperations.Assert(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(atom);
        _space.Contains(atom).Should().BeTrue();
    }

    [Fact]
    public void Assert_TooFewChildren_ReturnsEmpty()
    {
        var args = Atom.Expr(Atom.Sym("assert"));

        var results = StandardOperations.Assert(_space, args).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Assert_WithNonMutableSpace_ReturnsEmpty()
    {
        var mockSpace = new Mock<IAtomSpace>();
        var args = Atom.Expr(Atom.Sym("assert"), Atom.Sym("fact"));

        var results = StandardOperations.Assert(mockSpace.Object, args).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region Retract

    [Fact]
    public void Retract_ExistingAtom_RemovesAndReturnsTrue()
    {
        var atom = Atom.Sym("toRemove");
        _space.Add(atom);
        var args = Atom.Expr(Atom.Sym("retract"), atom);

        var results = StandardOperations.Retract(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(Atom.Sym("True"));
        _space.Contains(atom).Should().BeFalse();
    }

    [Fact]
    public void Retract_NonExistingAtom_ReturnsEmpty()
    {
        var args = Atom.Expr(Atom.Sym("retract"), Atom.Sym("missing"));

        var results = StandardOperations.Retract(_space, args).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Retract_TooFewChildren_ReturnsEmpty()
    {
        var args = Atom.Expr(Atom.Sym("retract"));

        var results = StandardOperations.Retract(_space, args).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region Match

    [Fact]
    public void Match_PatternMatchesAtoms_ReturnsMatches()
    {
        _space.Add(Atom.Expr(Atom.Sym("Human"), Atom.Sym("Socrates")));
        _space.Add(Atom.Expr(Atom.Sym("Human"), Atom.Sym("Plato")));

        var args = Atom.Expr(
            Atom.Sym("match"),
            Atom.Expr(Atom.Sym("Human"), Atom.Var("x")));

        var results = StandardOperations.Match(_space, args).ToList();

        results.Should().HaveCount(2);
    }

    [Fact]
    public void Match_NoMatchingAtoms_ReturnsEmpty()
    {
        var args = Atom.Expr(
            Atom.Sym("match"),
            Atom.Expr(Atom.Sym("Human"), Atom.Var("x")));

        var results = StandardOperations.Match(_space, args).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Match_TooFewChildren_ReturnsEmpty()
    {
        var args = Atom.Expr(Atom.Sym("match"));

        var results = StandardOperations.Match(_space, args).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region Quote

    [Fact]
    public void Quote_ReturnsAtomWithoutEvaluation()
    {
        var atom = Atom.Expr(Atom.Sym("f"), Atom.Sym("x"));
        var args = Atom.Expr(Atom.Sym("quote"), atom);

        var results = StandardOperations.Quote(_space, args).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(atom);
    }

    [Fact]
    public void Quote_TooFewChildren_ReturnsEmpty()
    {
        var args = Atom.Expr(Atom.Sym("quote"));

        var results = StandardOperations.Quote(_space, args).ToList();

        results.Should().BeEmpty();
    }

    #endregion
}
