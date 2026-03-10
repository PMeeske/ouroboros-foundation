using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class MeTTaSpecTests
{
    #region Core Types

    [Fact]
    public void Type_ReturnsTypeSymbol()
    {
        MeTTaSpec.Type.Name.Should().Be("Type");
    }

    [Fact]
    public void Arrow_ReturnsFunctionArrowSymbol()
    {
        MeTTaSpec.Arrow.Name.Should().Be("->");
    }

    [Fact]
    public void AtomType_ReturnsAtomSymbol()
    {
        MeTTaSpec.AtomType.Name.Should().Be("Atom");
    }

    [Fact]
    public void SymbolType_ReturnsSymbolSymbol()
    {
        MeTTaSpec.SymbolType.Name.Should().Be("Symbol");
    }

    [Fact]
    public void VariableType_ReturnsVariableSymbol()
    {
        MeTTaSpec.VariableType.Name.Should().Be("Variable");
    }

    [Fact]
    public void ExpressionType_ReturnsExpressionSymbol()
    {
        MeTTaSpec.ExpressionType.Name.Should().Be("Expression");
    }

    [Fact]
    public void GroundedType_ReturnsGroundedSymbol()
    {
        MeTTaSpec.GroundedType.Name.Should().Be("Grounded");
    }

    [Fact]
    public void Unit_ReturnsUnitSymbol()
    {
        MeTTaSpec.Unit.Name.Should().Be("Unit");
    }

    [Fact]
    public void Bool_ReturnsBoolSymbol()
    {
        MeTTaSpec.Bool.Name.Should().Be("Bool");
    }

    [Fact]
    public void Number_ReturnsNumberSymbol()
    {
        MeTTaSpec.Number.Name.Should().Be("Number");
    }

    [Fact]
    public void String_ReturnsStringSymbol()
    {
        MeTTaSpec.String.Name.Should().Be("String");
    }

    #endregion

    #region Standard Operations

    [Fact]
    public void TypeOf_CreatesTypeAnnotation()
    {
        var expr = MeTTaSpec.TypeOf(Atom.Sym("x"), Atom.Sym("Int"));

        expr.ToSExpr().Should().Be("(: x Int)");
    }

    [Fact]
    public void FunctionType_CreatesArrowExpression()
    {
        var expr = MeTTaSpec.FunctionType(Atom.Sym("Int"), Atom.Sym("Bool"));

        expr.ToSExpr().Should().Be("(-> Int Bool)");
    }

    [Fact]
    public void Match_CreatesMatchExpression()
    {
        var expr = MeTTaSpec.Match(Atom.Sym("&self"), Atom.Var("x"), Atom.Var("x"));

        expr.ToSExpr().Should().Be("(match &self $x $x)");
    }

    [Fact]
    public void Import_CreatesImportExpression()
    {
        var expr = MeTTaSpec.Import(Atom.Sym("stdlib"));

        expr.ToSExpr().Should().Be("(import! stdlib)");
    }

    [Fact]
    public void AddAtom_CreatesAddAtomExpression()
    {
        var expr = MeTTaSpec.AddAtom(Atom.Sym("&self"), Atom.Sym("fact"));

        expr.ToSExpr().Should().Be("(add-atom &self fact)");
    }

    [Fact]
    public void RemoveAtom_CreatesRemoveAtomExpression()
    {
        var expr = MeTTaSpec.RemoveAtom(Atom.Sym("&self"), Atom.Sym("fact"));

        expr.ToSExpr().Should().Be("(remove-atom &self fact)");
    }

    [Fact]
    public void Collapse_CreatesCollapseExpression()
    {
        var expr = MeTTaSpec.Collapse(Atom.Sym("x"));

        expr.ToSExpr().Should().Be("(collapse x)");
    }

    [Fact]
    public void Superpose_CreatesSuperposExpression()
    {
        var expr = MeTTaSpec.Superpose(Atom.Sym("a"), Atom.Sym("b"), Atom.Sym("c"));

        expr.ToSExpr().Should().Be("(superpose a b c)");
    }

    [Fact]
    public void Superpose_SingleAlternative_FormatsCorrectly()
    {
        var expr = MeTTaSpec.Superpose(Atom.Sym("a"));

        expr.ToSExpr().Should().Be("(superpose a)");
    }

    #endregion

    #region Meta-Level Operations

    [Fact]
    public void Eval_CreatesEvalExpression()
    {
        var expr = MeTTaSpec.Eval(Atom.Sym("x"));

        expr.ToSExpr().Should().Be("(eval x)");
    }

    [Fact]
    public void Quote_CreatesQuoteExpression()
    {
        var inner = Atom.Expr(Atom.Sym("f"), Atom.Sym("x"));
        var expr = MeTTaSpec.Quote(inner);

        expr.ToSExpr().Should().Be("(quote (f x))");
    }

    [Fact]
    public void Unquote_CreatesUnquoteExpression()
    {
        var expr = MeTTaSpec.Unquote(Atom.Sym("x"));

        expr.ToSExpr().Should().Be("(unquote x)");
    }

    [Fact]
    public void GetType_CreatesGetTypeExpression()
    {
        var expr = MeTTaSpec.GetType(Atom.Sym("x"));

        expr.ToSExpr().Should().Be("(get-type x)");
    }

    #endregion

    #region Logic Operations

    [Fact]
    public void Implies_CreatesImpliesExpression()
    {
        var expr = MeTTaSpec.Implies(
            Atom.Expr(Atom.Sym("Human"), Atom.Var("x")),
            Atom.Expr(Atom.Sym("Mortal"), Atom.Var("x")));

        expr.ToSExpr().Should().Be("(implies (Human $x) (Mortal $x))");
    }

    [Fact]
    public void And_CreatesAndExpression()
    {
        var expr = MeTTaSpec.And(Atom.Sym("A"), Atom.Sym("B"));

        expr.ToSExpr().Should().Be("(and A B)");
    }

    [Fact]
    public void Or_CreatesOrExpression()
    {
        var expr = MeTTaSpec.Or(Atom.Sym("A"), Atom.Sym("B"));

        expr.ToSExpr().Should().Be("(or A B)");
    }

    [Fact]
    public void Not_CreatesNotExpression()
    {
        var expr = MeTTaSpec.Not(Atom.Sym("A"));

        expr.ToSExpr().Should().Be("(not A)");
    }

    [Fact]
    public void Let_CreatesLetExpression()
    {
        var expr = MeTTaSpec.Let(Atom.Var("x"), Atom.Sym("5"), Atom.Expr(Atom.Sym("+"), Atom.Var("x"), Atom.Sym("1")));

        expr.ToSExpr().Should().Be("(let $x 5 (+ $x 1))");
    }

    #endregion

    #region Self-Reference Symbols

    [Fact]
    public void Self_ReturnsAndSelfSymbol()
    {
        MeTTaSpec.Self.Name.Should().Be("&self");
    }

    [Fact]
    public void KnowledgeBase_ReturnsAndKbSymbol()
    {
        MeTTaSpec.KnowledgeBase.Name.Should().Be("&kb");
    }

    [Fact]
    public void Empty_ReturnsEmptySymbol()
    {
        MeTTaSpec.Empty.Name.Should().Be("Empty");
    }

    [Fact]
    public void True_ReturnsTrueSymbol()
    {
        MeTTaSpec.True.Name.Should().Be("True");
    }

    [Fact]
    public void False_ReturnsFalseSymbol()
    {
        MeTTaSpec.False.Name.Should().Be("False");
    }

    #endregion
}
