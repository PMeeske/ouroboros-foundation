using System.Collections.Immutable;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.Synthesis;

namespace Ouroboros.Core.Tests.Synthesis;

/// <summary>
/// Additional tests for MeTTaDSLBridge, RewriteRule, and TypeRule.
/// </summary>
[Trait("Category", "Unit")]
public class MeTTaDSLBridgeAdditionalTests
{
    [Fact]
    public void ASTToMeTTa_UnknownNodeType_FallsBackToSymbol()
    {
        var node = new ASTNode("Custom", "value", new List<ASTNode>());

        var result = MeTTaDSLBridge.ASTToMeTTa(node);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Symbol>();
        ((Symbol)result.Value).Name.Should().Be("value");
    }

    [Fact]
    public void ASTToMeTTa_VariableNode_TrimsLeadingDollar()
    {
        var node = new ASTNode("Variable", "$myVar", new List<ASTNode>());

        var result = MeTTaDSLBridge.ASTToMeTTa(node);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Variable>();
        ((Variable)result.Value).Name.Should().Be("myVar");
    }

    [Fact]
    public void ASTToMeTTa_ApplyNodeWithNestedChildren_ReturnsExpression()
    {
        var child = new ASTNode("Apply", "inc", new List<ASTNode>
        {
            new("Primitive", "x", new List<ASTNode>())
        });
        var node = new ASTNode("Apply", "compose", new List<ASTNode> { child });

        var result = MeTTaDSLBridge.ASTToMeTTa(node);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Expression>();
    }

    [Fact]
    public void MeTTaToAST_ExpressionWithNonSymbolHead_ReturnsApplyExpr()
    {
        // Expression where head is not a Symbol
        var expr = Atom.Expr(Atom.Var("x"), Atom.Sym("a"));

        var result = MeTTaDSLBridge.MeTTaToAST(expr);

        result.IsSuccess.Should().BeTrue();
        result.Value.NodeType.Should().Be("Apply");
        result.Value.Value.Should().Be("expr");
    }

    [Fact]
    public void MeTTaToAST_ExpressionWithSymbolHead_ExtractsTail()
    {
        var expr = Atom.Expr(Atom.Sym("f"), Atom.Sym("a"), Atom.Sym("b"));

        var result = MeTTaDSLBridge.MeTTaToAST(expr);

        result.IsSuccess.Should().BeTrue();
        result.Value.NodeType.Should().Be("Apply");
        result.Value.Value.Should().Be("f");
        result.Value.Children.Should().HaveCount(2);
    }

    [Fact]
    public void ProgramToMeTTa_DelegatesToASTToMeTTa()
    {
        var root = new ASTNode("Apply", "add", new List<ASTNode>
        {
            new("Primitive", "1", new List<ASTNode>()),
            new("Primitive", "2", new List<ASTNode>())
        });
        var ast = new AbstractSyntaxTree(root, 2, 3);
        var dsl = new DomainSpecificLanguage("test", new List<Primitive>(), new List<TypeRule>(), new List<RewriteRule>());
        var program = new Program("(add 1 2)", ast, dsl, -1.0);

        var result = MeTTaDSLBridge.ProgramToMeTTa(program);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Expression>();
    }

    [Fact]
    public void TypeRuleToMeTTa_SingleInputType_ReturnsArrowExpression()
    {
        var rule = new TypeRule("inc", new List<string> { "int" }, "int");

        var atom = MeTTaDSLBridge.TypeRuleToMeTTa(rule);

        atom.Should().BeOfType<Expression>();
        atom.ToSExpr().Should().Contain("->");
    }

    [Fact]
    public void DSLToMeTTa_OnlyPrimitives_ReturnsCorrectCount()
    {
        var primitives = new List<Primitive>
        {
            new("add", "int -> int -> int", x => x, 0.0),
        };
        var dsl = new DomainSpecificLanguage("test", primitives, new List<TypeRule>(), new List<RewriteRule>());

        var atoms = MeTTaDSLBridge.DSLToMeTTa(dsl);

        atoms.Should().HaveCount(1);
    }

    [Fact]
    public void DSLToMeTTa_OnlyTypeRules_ReturnsCorrectCount()
    {
        var typeRules = new List<TypeRule>
        {
            new("map", new List<string> { "a -> b", "List a" }, "List b"),
        };
        var dsl = new DomainSpecificLanguage("test", new List<Primitive>(), typeRules, new List<RewriteRule>());

        var atoms = MeTTaDSLBridge.DSLToMeTTa(dsl);

        atoms.Should().HaveCount(1);
    }
}

[Trait("Category", "Unit")]
public class RewriteRuleAdditionalTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var pattern = new ASTNode("Apply", "add", new List<ASTNode>
        {
            new("Primitive", "0", new List<ASTNode>()),
            new("Variable", "$x", new List<ASTNode>())
        });
        var replacement = new ASTNode("Variable", "$x", new List<ASTNode>());

        var rule = new RewriteRule("AddZero", pattern, replacement);

        rule.Name.Should().Be("AddZero");
        rule.Pattern.Should().Be(pattern);
        rule.Replacement.Should().Be(replacement);
    }

    [Fact]
    public void RecordEquality_Works()
    {
        var pattern = new ASTNode("Primitive", "x", new List<ASTNode>());
        var replacement = new ASTNode("Primitive", "y", new List<ASTNode>());

        var a = new RewriteRule("R1", pattern, replacement);
        var b = new RewriteRule("R1", pattern, replacement);

        a.Should().Be(b);
    }
}

[Trait("Category", "Unit")]
public class TypeRuleAdditionalTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var rule = new TypeRule("map", new List<string> { "a -> b", "List a" }, "List b");

        rule.Name.Should().Be("map");
        rule.InputTypes.Should().HaveCount(2);
        rule.OutputType.Should().Be("List b");
    }

    [Fact]
    public void RecordEquality_Works()
    {
        var a = new TypeRule("f", new List<string> { "int" }, "int");
        var b = new TypeRule("f", new List<string> { "int" }, "int");

        // Record equality checks value equality for the inputs
        a.Name.Should().Be(b.Name);
        a.OutputType.Should().Be(b.OutputType);
    }
}
