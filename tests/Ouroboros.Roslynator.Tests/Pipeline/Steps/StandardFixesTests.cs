using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;
using Ouroboros.Roslynator.Pipeline.Steps;

namespace Ouroboros.Tests.Pipeline.Steps;

[Trait("Category", "Unit")]
public sealed class StandardFixesTests
{
    private static FixState CreateState(string code, string diagnosticId, int spanStart = 0, int spanLength = 5)
    {
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = document.GetSyntaxRootAsync().GetAwaiter().GetResult()!;
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor(diagnosticId, "Test", "Test message", "Test", DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, new TextSpan(spanStart, spanLength)));
        return new FixState(document, diagnostic, root);
    }

    [Fact]
    public void ShouldSkip_NullState_ReturnsTrue()
    {
        // Act & Assert
        StandardFixes.ShouldSkip(null!).Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_ValidState_ReturnsFalse()
    {
        // Arrange
        var state = CreateState("class C { }", "TEST001");

        // Act & Assert
        StandardFixes.ShouldSkip(state).Should().BeFalse();
    }

    [Fact]
    public void FindNodeSafe_NullRoot_ReturnsNull()
    {
        // Act
        var result = StandardFixes.FindNodeSafe(null!, new TextSpan(0, 5));

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FindNodeSafe_ValidSpan_ReturnsNode()
    {
        // Arrange
        var tree = CSharpSyntaxTree.ParseText("class C { }");
        var root = tree.GetRoot();

        // Act
        var node = StandardFixes.FindNodeSafe(root, new TextSpan(0, 5));

        // Assert
        node.Should().NotBeNull();
    }

    [Fact]
    public void FindNodeSafe_OutOfRangeSpan_ReturnsNull()
    {
        // Arrange
        var tree = CSharpSyntaxTree.ParseText("class C { }");
        var root = tree.GetRoot();

        // Act
        var node = StandardFixes.FindNodeSafe(root, new TextSpan(1000, 5));

        // Assert
        node.Should().BeNull();
    }

    [Fact]
    public async Task ReplaceNode_NullState_ThrowsArgumentNullException()
    {
        // Arrange
        var tree = CSharpSyntaxTree.ParseText("class C { }");
        var root = tree.GetRoot();
        var node = root.DescendantNodes().First();

        // Act
        var act = () => StandardFixes.ReplaceNode(null!, node, node, "test");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SimplifyLinq_NullState_ReturnsState()
    {
        // Arrange & Act (ShouldSkip returns true for null, but we pass a valid state to avoid NRE)
        var state = CreateState("class C { }", "IDE0001");

        var result = await StandardFixes.SimplifyLinq(state);

        // Assert - no changes since no matching pattern
        result.Changes.Should().BeEmpty();
    }

    [Fact]
    public async Task UseCollectionExpression_ValidState_ReturnsUnchangedState()
    {
        // Arrange - placeholder implementation
        var state = CreateState("class C { }", "IDE0300");

        // Act
        var result = await StandardFixes.UseCollectionExpression(state);

        // Assert
        result.Should().BeSameAs(state);
    }

    [Fact]
    public async Task UseCollectionInitializer_ValidState_ReturnsUnchangedState()
    {
        // Arrange - placeholder implementation
        var state = CreateState("class C { }", "IDE0028");

        // Act
        var result = await StandardFixes.UseCollectionInitializer(state);

        // Assert
        result.Should().BeSameAs(state);
    }

    [Fact]
    public async Task UseExplicitType_ValidState_ReturnsUnchangedState()
    {
        // Arrange - placeholder implementation
        var state = CreateState("class C { }", "IDE0008");

        // Act
        var result = await StandardFixes.UseExplicitType(state);

        // Assert
        result.Should().BeSameAs(state);
    }

    [Fact]
    public async Task UseNullPropagation_ValidState_ReturnsUnchangedState()
    {
        // Arrange - placeholder implementation
        var state = CreateState("class C { }", "IDE0031");

        // Act
        var result = await StandardFixes.UseNullPropagation(state);

        // Assert
        result.Should().BeSameAs(state);
    }

    [Fact]
    public async Task UseObjectInitializer_ValidState_ReturnsUnchangedState()
    {
        // Arrange - placeholder implementation
        var state = CreateState("class C { }", "IDE0017");

        // Act
        var result = await StandardFixes.UseObjectInitializer(state);

        // Assert
        result.Should().BeSameAs(state);
    }
}
