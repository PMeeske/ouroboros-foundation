using Ouroboros.Core.Synthesis;

namespace Ouroboros.Core.Tests.Synthesis;

[Trait("Category", "Unit")]
public class ASTNodeTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var children = new List<ASTNode>
        {
            new("Variable", "x", new List<ASTNode>()),
            new("Variable", "y", new List<ASTNode>())
        };
        var node = new ASTNode("Apply", "add", children);

        node.NodeType.Should().Be("Apply");
        node.Value.Should().Be("add");
        node.Children.Should().HaveCount(2);
    }

    [Fact]
    public void LeafNode_HasNoChildren()
    {
        var leaf = new ASTNode("Variable", "x", new List<ASTNode>());
        leaf.Children.Should().BeEmpty();
    }

    [Fact]
    public void RecordEquality_ComparesAllFields()
    {
        var children = new List<ASTNode>();
        var a = new ASTNode("Lambda", "f", children);
        var b = new ASTNode("Lambda", "f", children);
        a.Should().Be(b);
    }
}

[Trait("Category", "Unit")]
public class DomainSpecificLanguageTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var dsl = new DomainSpecificLanguage(
            "TestDSL",
            new List<Primitive>(),
            new List<TypeRule>(),
            new List<RewriteRule>());

        dsl.Name.Should().Be("TestDSL");
        dsl.Primitives.Should().BeEmpty();
        dsl.TypeRules.Should().BeEmpty();
        dsl.Optimizations.Should().BeEmpty();
    }
}
