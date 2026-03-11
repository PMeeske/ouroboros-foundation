using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class MeTTaSpecAdditionalTests
{
    #region Superpose Edge Cases

    [Fact]
    public void Superpose_NoAlternatives_CreatesEmptySuperpose()
    {
        var expr = MeTTaSpec.Superpose();

        expr.ToSExpr().Should().Be("(superpose)");
    }

    #endregion

    #region Let - Additional

    [Fact]
    public void Let_WithExpression_CreatesCorrectExpression()
    {
        var expr = MeTTaSpec.Let(
            Atom.Var("result"),
            Atom.Expr(Atom.Sym("compute"), Atom.Sym("input")),
            Atom.Var("result"));

        expr.Children.Count.Should().Be(4);
        expr.Children[0].Should().Be(Atom.Sym("let"));
    }

    #endregion

    #region Match - Additional

    [Fact]
    public void Match_WithExpressions_FormatsCorrectly()
    {
        var expr = MeTTaSpec.Match(
            MeTTaSpec.Self,
            Atom.Expr(Atom.Sym("person"), Atom.Var("name")),
            Atom.Var("name"));

        expr.ToSExpr().Should().Be("(match &self (person $name) $name)");
    }

    #endregion

    #region Type Operations - Additional

    [Fact]
    public void FunctionType_MultiLevel_NestsCorrectly()
    {
        var inner = MeTTaSpec.FunctionType(Atom.Sym("Int"), Atom.Sym("Bool"));
        var outer = MeTTaSpec.FunctionType(Atom.Sym("String"), inner);

        outer.Children.Count.Should().Be(3);
        outer.Children[0].Should().Be(MeTTaSpec.Arrow);
    }

    [Fact]
    public void TypeOf_WithFunctionType_FormatsCorrectly()
    {
        var funcType = MeTTaSpec.FunctionType(Atom.Sym("Int"), Atom.Sym("Bool"));
        var expr = MeTTaSpec.TypeOf(Atom.Sym("isPositive"), funcType);

        expr.ToSExpr().Should().Be("(: isPositive (-> Int Bool))");
    }

    #endregion

    #region AddAtom / RemoveAtom - Additional

    [Fact]
    public void AddAtom_WithExpression_FormatsCorrectly()
    {
        var atom = Atom.Expr(Atom.Sym("person"), Atom.Sym("alice"));
        var expr = MeTTaSpec.AddAtom(MeTTaSpec.Self, atom);

        expr.ToSExpr().Should().Be("(add-atom &self (person alice))");
    }

    [Fact]
    public void RemoveAtom_WithExpression_FormatsCorrectly()
    {
        var atom = Atom.Expr(Atom.Sym("person"), Atom.Sym("alice"));
        var expr = MeTTaSpec.RemoveAtom(MeTTaSpec.Self, atom);

        expr.ToSExpr().Should().Be("(remove-atom &self (person alice))");
    }

    #endregion

    #region Quote/Unquote - Additional

    [Fact]
    public void Quote_NestedExpression_FormatsCorrectly()
    {
        var inner = Atom.Expr(Atom.Sym("add"), Atom.Sym("1"), Atom.Sym("2"));
        var quoted = MeTTaSpec.Quote(inner);

        quoted.ToSExpr().Should().Be("(quote (add 1 2))");
    }

    [Fact]
    public void Unquote_NestedQuote_FormatsCorrectly()
    {
        var quoted = MeTTaSpec.Quote(Atom.Sym("x"));
        var unquoted = MeTTaSpec.Unquote(quoted);

        unquoted.ToSExpr().Should().Be("(unquote (quote x))");
    }

    #endregion

    #region Eval - Additional

    [Fact]
    public void Eval_WithExpression_FormatsCorrectly()
    {
        var inner = Atom.Expr(Atom.Sym("+"), Atom.Sym("1"), Atom.Sym("2"));
        var expr = MeTTaSpec.Eval(inner);

        expr.ToSExpr().Should().Be("(eval (+ 1 2))");
    }

    #endregion

    #region GetType - Additional

    [Fact]
    public void GetType_WithExpression_FormatsCorrectly()
    {
        var inner = Atom.Expr(Atom.Sym("f"), Atom.Sym("x"));
        var expr = MeTTaSpec.GetType(inner);

        expr.ToSExpr().Should().Be("(get-type (f x))");
    }

    #endregion

    #region Collapse - Additional

    [Fact]
    public void Collapse_WithExpression_FormatsCorrectly()
    {
        var inner = Atom.Expr(Atom.Sym("superpose"), Atom.Sym("a"), Atom.Sym("b"));
        var expr = MeTTaSpec.Collapse(inner);

        expr.ToSExpr().Should().Be("(collapse (superpose a b))");
    }

    #endregion

    #region Import - Additional

    [Fact]
    public void Import_WithExpression_FormatsCorrectly()
    {
        var module = Atom.Expr(Atom.Sym("my"), Atom.Sym("module"));
        var expr = MeTTaSpec.Import(module);

        expr.Children.Count.Should().Be(2);
        expr.Children[0].Should().Be(Atom.Sym("import!"));
    }

    #endregion

    #region Logic Operations - Additional

    [Fact]
    public void And_WithExpressions_FormatsCorrectly()
    {
        var left = Atom.Expr(Atom.Sym("human"), Atom.Var("x"));
        var right = Atom.Expr(Atom.Sym("mortal"), Atom.Var("x"));
        var expr = MeTTaSpec.And(left, right);

        expr.ToSExpr().Should().Be("(and (human $x) (mortal $x))");
    }

    [Fact]
    public void Or_WithExpressions_FormatsCorrectly()
    {
        var left = Atom.Expr(Atom.Sym("cat"), Atom.Var("x"));
        var right = Atom.Expr(Atom.Sym("dog"), Atom.Var("x"));
        var expr = MeTTaSpec.Or(left, right);

        expr.ToSExpr().Should().Be("(or (cat $x) (dog $x))");
    }

    [Fact]
    public void Not_WithExpression_FormatsCorrectly()
    {
        var inner = Atom.Expr(Atom.Sym("alive"), Atom.Var("x"));
        var expr = MeTTaSpec.Not(inner);

        expr.ToSExpr().Should().Be("(not (alive $x))");
    }

    [Fact]
    public void Implies_SimpleSymbols_FormatsCorrectly()
    {
        var expr = MeTTaSpec.Implies(Atom.Sym("A"), Atom.Sym("B"));

        expr.ToSExpr().Should().Be("(implies A B)");
    }

    #endregion

    #region All properties return correct type

    [Fact]
    public void AllCoreTypes_AreSymbols()
    {
        MeTTaSpec.Type.Should().BeOfType<Symbol>();
        MeTTaSpec.Arrow.Should().BeOfType<Symbol>();
        MeTTaSpec.AtomType.Should().BeOfType<Symbol>();
        MeTTaSpec.SymbolType.Should().BeOfType<Symbol>();
        MeTTaSpec.VariableType.Should().BeOfType<Symbol>();
        MeTTaSpec.ExpressionType.Should().BeOfType<Symbol>();
        MeTTaSpec.GroundedType.Should().BeOfType<Symbol>();
        MeTTaSpec.Unit.Should().BeOfType<Symbol>();
        MeTTaSpec.Bool.Should().BeOfType<Symbol>();
        MeTTaSpec.Number.Should().BeOfType<Symbol>();
        MeTTaSpec.String.Should().BeOfType<Symbol>();
        MeTTaSpec.Self.Should().BeOfType<Symbol>();
        MeTTaSpec.KnowledgeBase.Should().BeOfType<Symbol>();
        MeTTaSpec.Empty.Should().BeOfType<Symbol>();
        MeTTaSpec.True.Should().BeOfType<Symbol>();
        MeTTaSpec.False.Should().BeOfType<Symbol>();
    }

    #endregion
}
