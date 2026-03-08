using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class InterpreterTests
{
    private readonly AtomSpace _space;
    private readonly Interpreter _sut;

    public InterpreterTests()
    {
        _space = new AtomSpace();
        _sut = new Interpreter(_space);
    }

    [Fact]
    public void Constructor_NullSpace_ThrowsArgumentNullException()
    {
        var act = () => new Interpreter(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullGroundedOps_UsesStandardOps()
    {
        var interpreter = new Interpreter(_space, null);

        interpreter.Should().NotBeNull();
    }

    // --- Evaluate ---

    [Fact]
    public void Evaluate_SymbolInSpace_ReturnsIt()
    {
        var sym = Atom.Sym("hello");
        _space.Add(sym);

        var results = _sut.Evaluate(sym).ToList();

        results.Should().Contain(sym);
    }

    [Fact]
    public void Evaluate_SymbolNotInSpace_ReturnsEmpty()
    {
        var sym = Atom.Sym("nonexistent");

        var results = _sut.Evaluate(sym).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_EqualOperationWithEqualArgs_ReturnsTrue()
    {
        var query = Atom.Expr(Atom.Sym("equal"), Atom.Sym("a"), Atom.Sym("a"));

        var results = _sut.Evaluate(query).ToList();

        results.OfType<Symbol>().Should().Contain(s => s.Name == "True");
    }

    [Fact]
    public void Evaluate_EqualOperationWithUnequalArgs_ReturnsEmpty()
    {
        var query = Atom.Expr(Atom.Sym("equal"), Atom.Sym("a"), Atom.Sym("b"));

        var results = _sut.Evaluate(query).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_NotOperationWhenExprNotInSpace_ReturnsTrue()
    {
        var query = Atom.Expr(Atom.Sym("not"), Atom.Sym("nonexistent"));

        var results = _sut.Evaluate(query).ToList();

        results.OfType<Symbol>().Should().Contain(s => s.Name == "True");
    }

    [Fact]
    public void Evaluate_NotOperationWhenExprInSpace_ReturnsEmpty()
    {
        _space.Add(Atom.Sym("exists"));
        var query = Atom.Expr(Atom.Sym("not"), Atom.Sym("exists"));

        var results = _sut.Evaluate(query).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_QuoteOperation_ReturnsAtomUnchanged()
    {
        var innerAtom = Atom.Sym("something");
        var query = Atom.Expr(Atom.Sym("quote"), innerAtom);

        var results = _sut.Evaluate(query).ToList();

        results.Should().Contain(innerAtom);
    }

    [Fact]
    public void Evaluate_MatchOperation_ReturnsMatchingAtoms()
    {
        _space.Add(Atom.Expr(Atom.Sym("person"), Atom.Sym("alice")));
        _space.Add(Atom.Expr(Atom.Sym("person"), Atom.Sym("bob")));

        var pattern = Atom.Expr(Atom.Sym("person"), Atom.Var("x"));
        var query = Atom.Expr(Atom.Sym("match"), pattern);

        var results = _sut.Evaluate(query).ToList();

        results.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void Evaluate_AssertOperation_AddsAtomToSpace()
    {
        var newAtom = Atom.Sym("new-fact");
        var query = Atom.Expr(Atom.Sym("assert"), newAtom);

        var results = _sut.Evaluate(query).ToList();

        _space.Contains(newAtom).Should().BeTrue();
        results.Should().Contain(newAtom);
    }

    [Fact]
    public void Evaluate_RetractOperation_RemovesAtomFromSpace()
    {
        var atom = Atom.Sym("to-remove");
        _space.Add(atom);

        var query = Atom.Expr(Atom.Sym("retract"), atom);
        var results = _sut.Evaluate(query).ToList();

        _space.Contains(atom).Should().BeFalse();
        results.OfType<Symbol>().Should().Contain(s => s.Name == "True");
    }

    // --- Succeeds / EvaluateFirst ---

    [Fact]
    public void Succeeds_WhenAtomExists_ReturnsTrue()
    {
        _space.Add(Atom.Sym("fact"));

        _sut.Succeeds(Atom.Sym("fact")).Should().BeTrue();
    }

    [Fact]
    public void Succeeds_WhenAtomMissing_ReturnsFalse()
    {
        _sut.Succeeds(Atom.Sym("no-such-fact")).Should().BeFalse();
    }

    [Fact]
    public void EvaluateFirst_WhenAtomExists_ReturnsSome()
    {
        _space.Add(Atom.Sym("fact"));

        var result = _sut.EvaluateFirst(Atom.Sym("fact"));

        result.HasValue.Should().BeTrue();
    }

    [Fact]
    public void EvaluateFirst_WhenAtomMissing_ReturnsNone()
    {
        var result = _sut.EvaluateFirst(Atom.Sym("missing"));

        result.HasValue.Should().BeFalse();
    }

    // --- EvaluateWithBindings ---

    [Fact]
    public void EvaluateWithBindings_PatternQuery_ReturnsBindings()
    {
        _space.Add(Atom.Expr(Atom.Sym("age"), Atom.Sym("alice"), Atom.Sym("30")));

        var pattern = Atom.Expr(Atom.Sym("age"), Atom.Sym("alice"), Atom.Var("x"));

        var results = _sut.EvaluateWithBindings(pattern).ToList();

        results.Should().NotBeEmpty();
    }

    [Fact]
    public void EvaluateWithBindings_GroundQuery_ReturnsEmptyBindings()
    {
        var atom = Atom.Sym("ground-fact");
        _space.Add(atom);

        var results = _sut.EvaluateWithBindings(atom).ToList();

        results.Should().NotBeEmpty();
        results[0].Bindings.Should().Be(Substitution.Empty);
    }

    // --- Static helpers ---

    [Fact]
    public void Query_ReturnsCallable()
    {
        _space.Add(Atom.Sym("test"));
        var query = Interpreter.Query(Atom.Sym("test"));

        var results = query(_space).ToList();

        results.Should().Contain(Atom.Sym("test"));
    }

    [Fact]
    public void Bind_ComposesEnumerables()
    {
        var first = new[] { Atom.Sym("a"), Atom.Sym("b") };
        var results = Interpreter.Bind(first, a => new[] { a }).ToList();

        results.Should().HaveCount(2);
    }

    // --- Implies rule evaluation ---

    [Fact]
    public void Evaluate_ImpliesRule_DerivesFact()
    {
        // Add: (implies (human $x) (mortal $x))
        _space.Add(Atom.Expr(
            Atom.Sym("implies"),
            Atom.Expr(Atom.Sym("human"), Atom.Var("x")),
            Atom.Expr(Atom.Sym("mortal"), Atom.Var("x"))));
        // Add: (human socrates)
        _space.Add(Atom.Expr(Atom.Sym("human"), Atom.Sym("socrates")));

        // Query: (mortal socrates)
        var query = Atom.Expr(Atom.Sym("mortal"), Atom.Sym("socrates"));
        var results = _sut.Evaluate(query).ToList();

        // Should derive (mortal socrates) via implies rule
        results.Should().NotBeEmpty();
    }
}
