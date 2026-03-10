using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class InterpreterExtensionsTests
{
    #region ToOption

    [Fact]
    public void ToOption_Symbol_ReturnsSomeContainingAtom()
    {
        var atom = Atom.Sym("test");

        var option = atom.ToOption();

        option.HasValue.Should().BeTrue();
        option.Value.Should().Be(atom);
    }

    [Fact]
    public void ToOption_Variable_ReturnsSomeContainingAtom()
    {
        var atom = Atom.Var("x");

        var option = atom.ToOption();

        option.HasValue.Should().BeTrue();
        option.Value.Should().Be(atom);
    }

    [Fact]
    public void ToOption_Expression_ReturnsSomeContainingAtom()
    {
        var atom = Atom.Expr(Atom.Sym("f"), Atom.Sym("a"));

        var option = atom.ToOption();

        option.HasValue.Should().BeTrue();
        option.Value.Should().Be(atom);
    }

    #endregion

    #region EvaluateAndThen

    [Fact]
    public void EvaluateAndThen_AppliesContinuationToResults()
    {
        var space = new AtomSpace();
        space.Add(Atom.Sym("hello"));
        var interpreter = new Interpreter(space);

        var results = interpreter.EvaluateAndThen(
            Atom.Sym("hello"),
            atoms => atoms.Select(a => a.ToSExpr()));

        results.Should().Contain("hello");
    }

    [Fact]
    public void EvaluateAndThen_EmptyResults_ContinuationReceivesEmpty()
    {
        var space = new AtomSpace();
        var interpreter = new Interpreter(space);

        var results = interpreter.EvaluateAndThen(
            Atom.Sym("missing"),
            atoms => atoms.Select(a => a.ToSExpr())).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateAndThen_TransformsToIntegers()
    {
        var space = new AtomSpace();
        space.Add(Atom.Sym("a"));
        space.Add(Atom.Sym("bb"));
        var interpreter = new Interpreter(space);

        var results = interpreter.EvaluateAndThen(
            Atom.Sym("a"),
            atoms => atoms.Select(a => a.ToSExpr().Length)).ToList();

        results.Should().Contain(1);
    }

    #endregion

    #region EvaluateAll

    [Fact]
    public void EvaluateAll_CombinesResultsFromMultipleQueries()
    {
        var space = new AtomSpace();
        space.Add(Atom.Sym("a"));
        space.Add(Atom.Sym("b"));
        var interpreter = new Interpreter(space);

        var results = interpreter.EvaluateAll(Atom.Sym("a"), Atom.Sym("b")).ToList();

        results.Should().HaveCount(2);
        results.Should().Contain(Atom.Sym("a"));
        results.Should().Contain(Atom.Sym("b"));
    }

    [Fact]
    public void EvaluateAll_SomeQueriesHaveNoResults_ReturnsPartialResults()
    {
        var space = new AtomSpace();
        space.Add(Atom.Sym("a"));
        var interpreter = new Interpreter(space);

        var results = interpreter.EvaluateAll(Atom.Sym("a"), Atom.Sym("missing")).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(Atom.Sym("a"));
    }

    [Fact]
    public void EvaluateAll_NoQueries_ReturnsEmpty()
    {
        var space = new AtomSpace();
        var interpreter = new Interpreter(space);

        var results = interpreter.EvaluateAll().ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void EvaluateAll_SingleQuery_WorksCorrectly()
    {
        var space = new AtomSpace();
        space.Add(Atom.Sym("only"));
        var interpreter = new Interpreter(space);

        var results = interpreter.EvaluateAll(Atom.Sym("only")).ToList();

        results.Should().ContainSingle();
        results[0].Should().Be(Atom.Sym("only"));
    }

    #endregion
}
