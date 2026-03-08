using System.Collections.Immutable;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.Synthesis;

namespace Ouroboros.Core.Tests.Synthesis;

[Trait("Category", "Unit")]
[Trait("Category", "Synthesis")]
public class MeTTaDSLBridgeTests
{
    [Fact]
    public void ASTToMeTTa_PrimitiveNode_ReturnsSymbol()
    {
        var node = new ASTNode("Primitive", "add", new List<ASTNode>());

        var result = MeTTaDSLBridge.ASTToMeTTa(node);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Symbol>();
        ((Symbol)result.Value).Name.Should().Be("add");
    }

    [Fact]
    public void ASTToMeTTa_VariableNode_ReturnsVariable()
    {
        var node = new ASTNode("Variable", "$x", new List<ASTNode>());

        var result = MeTTaDSLBridge.ASTToMeTTa(node);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Variable>();
    }

    [Fact]
    public void ASTToMeTTa_ApplyNode_ReturnsExpression()
    {
        var node = new ASTNode("Apply", "add", new List<ASTNode>
        {
            new("Primitive", "x", new List<ASTNode>()),
            new("Primitive", "y", new List<ASTNode>())
        });

        var result = MeTTaDSLBridge.ASTToMeTTa(node);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Expression>();
    }

    [Fact]
    public void MeTTaToAST_Symbol_ReturnsPrimitiveNode()
    {
        var atom = Atom.Sym("add");

        var result = MeTTaDSLBridge.MeTTaToAST(atom);

        result.IsSuccess.Should().BeTrue();
        result.Value.NodeType.Should().Be("Primitive");
        result.Value.Value.Should().Be("add");
    }

    [Fact]
    public void MeTTaToAST_Variable_ReturnsVariableNode()
    {
        var atom = Atom.Var("x");

        var result = MeTTaDSLBridge.MeTTaToAST(atom);

        result.IsSuccess.Should().BeTrue();
        result.Value.NodeType.Should().Be("Variable");
        result.Value.Value.Should().Be("$x");
    }

    [Fact]
    public void MeTTaToAST_Expression_ReturnsApplyNode()
    {
        var atom = Atom.Expr(Atom.Sym("add"), Atom.Sym("1"), Atom.Sym("2"));

        var result = MeTTaDSLBridge.MeTTaToAST(atom);

        result.IsSuccess.Should().BeTrue();
        result.Value.NodeType.Should().Be("Apply");
        result.Value.Value.Should().Be("add");
        result.Value.Children.Should().HaveCount(2);
    }

    [Fact]
    public void MeTTaToAST_EmptyExpression_ReturnsPrimitiveWithEmptyValue()
    {
        var atom = new Expression(ImmutableList<Atom>.Empty);

        var result = MeTTaDSLBridge.MeTTaToAST(atom);

        result.IsSuccess.Should().BeTrue();
        result.Value.NodeType.Should().Be("Primitive");
        result.Value.Value.Should().Be("()");
    }

    [Fact]
    public void RoundTrip_PrimitiveNode_PreservesStructure()
    {
        var originalNode = new ASTNode("Primitive", "inc", new List<ASTNode>());

        var metta = MeTTaDSLBridge.ASTToMeTTa(originalNode);
        var roundtripped = MeTTaDSLBridge.MeTTaToAST(metta.Value);

        roundtripped.IsSuccess.Should().BeTrue();
        roundtripped.Value.NodeType.Should().Be(originalNode.NodeType);
        roundtripped.Value.Value.Should().Be(originalNode.Value);
    }

    [Fact]
    public void ProgramToMeTTa_ConvertsViaAST()
    {
        var root = new ASTNode("Primitive", "identity", new List<ASTNode>());
        var ast = new AbstractSyntaxTree(root, 1, 1);
        var dsl = new DomainSpecificLanguage("test", new List<Primitive>(), new List<TypeRule>(), new List<RewriteRule>());
        var program = new Program("identity", ast, dsl, -1.0);

        var result = MeTTaDSLBridge.ProgramToMeTTa(program);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void PrimitiveToMeTTa_ReturnsTypeDeclaration()
    {
        var primitive = new Primitive("inc", "int -> int", x => x, -0.5);

        var atom = MeTTaDSLBridge.PrimitiveToMeTTa(primitive);

        atom.Should().BeOfType<Expression>();
        atom.ToSExpr().Should().Contain(":");
        atom.ToSExpr().Should().Contain("inc");
    }

    [Fact]
    public void TypeRuleToMeTTa_ReturnsArrowTypeExpression()
    {
        var rule = new TypeRule("add", new List<string> { "int", "int" }, "int");

        var atom = MeTTaDSLBridge.TypeRuleToMeTTa(rule);

        atom.Should().BeOfType<Expression>();
        atom.ToSExpr().Should().Contain("->");
        atom.ToSExpr().Should().Contain("add");
    }

    [Fact]
    public void DSLToMeTTa_ConvertsAllPrimitivesAndRules()
    {
        var primitives = new List<Primitive>
        {
            new("add", "int -> int -> int", x => x, -0.5),
            new("inc", "int -> int", x => x, -0.3)
        };
        var typeRules = new List<TypeRule>
        {
            new("compose", new List<string> { "a -> b", "b -> c" }, "a -> c")
        };
        var dsl = new DomainSpecificLanguage("math", primitives, typeRules, new List<RewriteRule>());

        var atoms = MeTTaDSLBridge.DSLToMeTTa(dsl);

        atoms.Should().HaveCount(3); // 2 primitives + 1 type rule
    }

    [Fact]
    public void DSLToMeTTa_EmptyDSL_ReturnsEmptyList()
    {
        var dsl = new DomainSpecificLanguage("empty", new List<Primitive>(), new List<TypeRule>(), new List<RewriteRule>());

        var atoms = MeTTaDSLBridge.DSLToMeTTa(dsl);

        atoms.Should().BeEmpty();
    }
}
