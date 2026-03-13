using Ouroboros.Core.Synthesis;

namespace Ouroboros.Core.Tests.Synthesis;

[Trait("Category", "Unit")]
public class ProgramTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var root = new ASTNode("Primitive", "identity", new List<ASTNode>());
        var ast = new AbstractSyntaxTree(root, 1, 1);
        var dsl = CreateSimpleDSL();
        var trace = new ExecutionTrace(
            new List<ExecutionStep>(), "output", TimeSpan.FromMilliseconds(10));

        var program = new Program("identity", ast, dsl, -0.5, trace);

        program.SourceCode.Should().Be("identity");
        program.AST.Should().BeSameAs(ast);
        program.Language.Should().BeSameAs(dsl);
        program.LogProbability.Should().Be(-0.5);
        program.Trace.Should().BeSameAs(trace);
    }

    [Fact]
    public void Construction_NullTrace_DefaultsToNull()
    {
        var root = new ASTNode("Primitive", "identity", new List<ASTNode>());
        var ast = new AbstractSyntaxTree(root, 1, 1);
        var dsl = CreateSimpleDSL();

        var program = new Program("identity", ast, dsl, -0.5);

        program.Trace.Should().BeNull();
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var root = new ASTNode("Primitive", "identity", new List<ASTNode>());
        var ast = new AbstractSyntaxTree(root, 1, 1);
        var dsl = CreateSimpleDSL();

        var program1 = new Program("identity", ast, dsl, -0.5);
        var program2 = new Program("identity", ast, dsl, -0.5);

        program1.Should().Be(program2);
    }

    [Fact]
    public void Equality_DifferentSourceCode_AreNotEqual()
    {
        var root = new ASTNode("Primitive", "identity", new List<ASTNode>());
        var ast = new AbstractSyntaxTree(root, 1, 1);
        var dsl = CreateSimpleDSL();

        var program1 = new Program("identity", ast, dsl, -0.5);
        var program2 = new Program("reverse", ast, dsl, -0.5);

        program1.Should().NotBe(program2);
    }

    [Fact]
    public void Equality_DifferentLogProbability_AreNotEqual()
    {
        var root = new ASTNode("Primitive", "identity", new List<ASTNode>());
        var ast = new AbstractSyntaxTree(root, 1, 1);
        var dsl = CreateSimpleDSL();

        var program1 = new Program("identity", ast, dsl, -0.5);
        var program2 = new Program("identity", ast, dsl, -1.0);

        program1.Should().NotBe(program2);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var root = new ASTNode("Primitive", "identity", new List<ASTNode>());
        var ast = new AbstractSyntaxTree(root, 1, 1);
        var dsl = CreateSimpleDSL();
        var original = new Program("identity", ast, dsl, -0.5);

        var modified = original with { LogProbability = -1.0 };

        modified.LogProbability.Should().Be(-1.0);
        modified.SourceCode.Should().Be("identity");
        original.LogProbability.Should().Be(-0.5);
    }

    private static DomainSpecificLanguage CreateSimpleDSL()
    {
        var primitives = new List<Primitive>
        {
            new("identity", "a -> a", args => args.FirstOrDefault() ?? new object(), -0.5)
        };

        return new DomainSpecificLanguage(
            "test-dsl", primitives, new List<TypeRule>(), new List<RewriteRule>());
    }
}
