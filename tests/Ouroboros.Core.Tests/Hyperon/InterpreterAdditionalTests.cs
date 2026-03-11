using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class InterpreterAdditionalTests
{
    private readonly AtomSpace _space;
    private readonly Interpreter _sut;

    public InterpreterAdditionalTests()
    {
        _space = new AtomSpace();
        _sut = new Interpreter(_space);
    }

    #region Constructor

    [Fact]
    public void Constructor_WithCustomGroundedOps_UsesProvided()
    {
        var registry = new GroundedRegistry();
        registry.Register("custom-op", (space, args) => new[] { Atom.Sym("custom-result") });

        var interpreter = new Interpreter(_space, registry);
        var results = interpreter.Evaluate(Atom.Expr(Atom.Sym("custom-op"))).ToList();

        results.Should().Contain(Atom.Sym("custom-result"));
    }

    #endregion

    #region Evaluate - Additional

    [Fact]
    public void Evaluate_ExpressionInSpace_ReturnsMatch()
    {
        var expr = Atom.Expr(Atom.Sym("fact"), Atom.Sym("a"));
        _space.Add(expr);

        var results = _sut.Evaluate(expr).ToList();

        results.Should().NotBeEmpty();
    }

    [Fact]
    public void Evaluate_PatternMatch_ReturnsBindings()
    {
        _space.Add(Atom.Expr(Atom.Sym("likes"), Atom.Sym("alice"), Atom.Sym("cats")));

        var pattern = Atom.Expr(Atom.Sym("likes"), Atom.Sym("alice"), Atom.Var("x"));

        var results = _sut.Evaluate(pattern).ToList();

        results.Should().NotBeEmpty();
    }

    [Fact]
    public void Evaluate_ImpliesRule_WithMultipleFacts_DerivesMultiple()
    {
        _space.Add(Atom.Expr(
            Atom.Sym("implies"),
            Atom.Expr(Atom.Sym("bird"), Atom.Var("x")),
            Atom.Expr(Atom.Sym("canfly"), Atom.Var("x"))));
        _space.Add(Atom.Expr(Atom.Sym("bird"), Atom.Sym("robin")));
        _space.Add(Atom.Expr(Atom.Sym("bird"), Atom.Sym("eagle")));

        var query = Atom.Expr(Atom.Sym("canfly"), Atom.Var("y"));
        var results = _sut.Evaluate(query).ToList();

        results.Should().NotBeEmpty();
    }

    [Fact]
    public void Evaluate_AndOperation_AllPresent_ReturnsTrue()
    {
        _space.Add(Atom.Sym("a"));
        _space.Add(Atom.Sym("b"));

        var query = Atom.Expr(Atom.Sym("and"), Atom.Sym("a"), Atom.Sym("b"));
        var results = _sut.Evaluate(query).ToList();

        results.Should().Contain(Atom.Sym("True"));
    }

    [Fact]
    public void Evaluate_OrOperation_OnePresent_ReturnsTrue()
    {
        _space.Add(Atom.Sym("a"));

        var query = Atom.Expr(Atom.Sym("or"), Atom.Sym("a"), Atom.Sym("b"));
        var results = _sut.Evaluate(query).ToList();

        results.Should().Contain(Atom.Sym("True"));
    }

    [Fact]
    public void Evaluate_ImpliesOperation_MatchesCondition_DerivesConclusion()
    {
        _space.Add(Atom.Expr(Atom.Sym("dog"), Atom.Sym("rex")));

        var query = Atom.Expr(
            Atom.Sym("implies"),
            Atom.Expr(Atom.Sym("dog"), Atom.Var("x")),
            Atom.Expr(Atom.Sym("animal"), Atom.Var("x")));

        var results = _sut.Evaluate(query).ToList();

        results.Should().NotBeEmpty();
    }

    [Fact]
    public void Evaluate_NonExistentExpression_ReturnsEmpty()
    {
        var query = Atom.Expr(Atom.Sym("nonexistent"), Atom.Sym("arg"));

        var results = _sut.Evaluate(query).ToList();

        results.Should().BeEmpty();
    }

    #endregion

    #region EvaluateWithBindings - Additional

    [Fact]
    public void EvaluateWithBindings_GroundQueryNotInSpace_ReturnsEmpty()
    {
        var results = _sut.EvaluateWithBindings(Atom.Sym("missing")).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateWithBindings_PatternWithMultipleMatches_ReturnsAll()
    {
        _space.Add(Atom.Expr(Atom.Sym("fruit"), Atom.Sym("apple")));
        _space.Add(Atom.Expr(Atom.Sym("fruit"), Atom.Sym("banana")));

        var pattern = Atom.Expr(Atom.Sym("fruit"), Atom.Var("x"));
        var results = _sut.EvaluateWithBindings(pattern).ToList();

        results.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void EvaluateWithBindings_WithImpliesRule_ReturnsRuleResults()
    {
        _space.Add(Atom.Expr(
            Atom.Sym("implies"),
            Atom.Expr(Atom.Sym("cat"), Atom.Var("x")),
            Atom.Expr(Atom.Sym("pet"), Atom.Var("x"))));
        _space.Add(Atom.Expr(Atom.Sym("cat"), Atom.Sym("whiskers")));

        var pattern = Atom.Expr(Atom.Sym("pet"), Atom.Var("y"));
        var results = _sut.EvaluateWithBindings(pattern).ToList();

        results.Should().NotBeEmpty();
    }

    [Fact]
    public void EvaluateWithBindings_GroundExpression_EvaluatesViaInterpreter()
    {
        var query = Atom.Expr(Atom.Sym("equal"), Atom.Sym("a"), Atom.Sym("a"));

        var results = _sut.EvaluateWithBindings(query).ToList();

        results.Should().NotBeEmpty();
        results[0].Bindings.Should().Be(Substitution.Empty);
    }

    #endregion

    #region Succeeds / EvaluateFirst - Additional

    [Fact]
    public void Succeeds_WithGroundedOp_ReturnsTrue()
    {
        var query = Atom.Expr(Atom.Sym("equal"), Atom.Sym("a"), Atom.Sym("a"));

        _sut.Succeeds(query).Should().BeTrue();
    }

    [Fact]
    public void EvaluateFirst_MultipleResults_ReturnsFirst()
    {
        _space.Add(Atom.Sym("first"));
        _space.Add(Atom.Sym("second"));

        // Query with grounded op that produces results
        var result = _sut.EvaluateFirst(Atom.Expr(Atom.Sym("equal"), Atom.Sym("x"), Atom.Sym("x")));

        result.HasValue.Should().BeTrue();
    }

    [Fact]
    public void EvaluateFirst_NoResults_ReturnsNone()
    {
        var result = _sut.EvaluateFirst(Atom.Sym("nonexistent"));

        result.HasValue.Should().BeFalse();
    }

    #endregion

    #region Static Helpers - Additional

    [Fact]
    public void Query_EmptySpace_ReturnsEmpty()
    {
        var emptySpace = new AtomSpace();
        var query = Interpreter.Query(Atom.Sym("missing"));

        var results = query(emptySpace).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Bind_EmptyFirst_ReturnsEmpty()
    {
        var results = Interpreter.Bind(
            Enumerable.Empty<Atom>(),
            a => new[] { a }).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Bind_WithTransformation_TransformsResults()
    {
        var first = new[] { "a", "b" };

        var results = Interpreter.Bind(first, s => new[] { Atom.Sym(s) }).ToList();

        results.Should().HaveCount(2);
        results[0].Should().Be(Atom.Sym("a"));
        results[1].Should().Be(Atom.Sym("b"));
    }

    [Fact]
    public void Bind_OneToMany_FlattensResults()
    {
        var first = new[] { "x" };

        var results = Interpreter.Bind(first, s => new[] { Atom.Sym(s + "1"), Atom.Sym(s + "2") }).ToList();

        results.Should().HaveCount(2);
    }

    #endregion
}
