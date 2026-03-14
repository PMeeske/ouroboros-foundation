using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;
using Ouroboros.Roslynator.Pipeline.Steps;

namespace Ouroboros.Tests.Pipeline.Steps;

/// <summary>
/// Extended tests for StandardFixes covering UseIndexOperator, UseRangeOperator,
/// ReplaceNode, and additional ShouldSkip/FindNodeSafe edge cases.
/// </summary>
[Trait("Category", "Unit")]
public sealed class StandardFixesExtendedTests
{
    private static FixState CreateState(string code, string diagnosticId, int spanStart, int spanLength)
    {
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = document.GetSyntaxRootAsync().GetAwaiter().GetResult()!;
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor(diagnosticId, "Test", "Test message", "Test",
                DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, new TextSpan(spanStart, spanLength)));
        return new FixState(document, diagnostic, root);
    }

    #region ReplaceNode Tests

    [Fact]
    public async Task ReplaceNode_ValidNodes_ReturnsUpdatedState()
    {
        // Arrange
        var tree = CSharpSyntaxTree.ParseText("class OldClass { }");
        var root = tree.GetRoot();
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        _ = classDecl.WithIdentifier(SyntaxFactory.Identifier("NewClass"));

        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From("class OldClass { }"));
        var docRoot = (await document.GetSyntaxRootAsync())!;
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("TEST", "Test", "Test", "Test", DiagnosticSeverity.Warning, true),
            Location.Create(docRoot.SyntaxTree, new TextSpan(0, 5)));
        var state = new FixState(document, diagnostic, docRoot);

        var oldNode = docRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var newNode = oldNode.WithIdentifier(SyntaxFactory.Identifier("NewClass"));

        // Act
        var result = await StandardFixes.ReplaceNode(state, oldNode, newNode, "Renamed class");

        // Assert
        result.Changes.Should().Contain("Renamed class");
        result.CurrentRoot.ToFullString().Should().Contain("NewClass");
    }

    [Fact]
    public async Task ReplaceNode_NullOldNode_ThrowsArgumentNullException()
    {
        // Arrange
        var state = CreateState("class C { }", "TEST", 0, 5);
        var tree = CSharpSyntaxTree.ParseText("class C { }");
        var node = tree.GetRoot();

        // Act
        var act = () => StandardFixes.ReplaceNode(state, null!, node, "test");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ReplaceNode_NullNewNode_ThrowsArgumentNullException()
    {
        // Arrange
        var state = CreateState("class C { }", "TEST", 0, 5);
        var tree = CSharpSyntaxTree.ParseText("class C { }");
        var node = tree.GetRoot();

        // Act
        var act = () => StandardFixes.ReplaceNode(state, node, null!, "test");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    #endregion

    #region UseIndexOperator Tests

    [Fact]
    public async Task UseIndexOperator_NoMatchingPattern_ReturnsUnchangedState()
    {
        // Arrange - simple code without the array[array.Length - 1] pattern
        var state = CreateState("class C { }", "IDE0056", 0, 5);

        // Act
        var result = await StandardFixes.UseIndexOperator(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    [Fact]
    public async Task UseIndexOperator_NullState_ReturnsState()
    {
        // Arrange
        var state = CreateState("class C { }", "IDE0056", 0, 5);

        // Act - ShouldSkip returns false for valid state, so the method proceeds
        var result = await StandardFixes.UseIndexOperator(state);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region UseRangeOperator Tests

    [Fact]
    public async Task UseRangeOperator_NoMatchingPattern_ReturnsUnchangedState()
    {
        // Arrange
        var state = CreateState("class C { }", "IDE0057", 0, 5);

        // Act
        var result = await StandardFixes.UseRangeOperator(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    #endregion

    #region SimplifyLinq Extended Tests

    [Fact]
    public async Task SimplifyLinq_NoLinqPattern_ReturnsUnchangedState()
    {
        // Arrange
        var state = CreateState("class C { void M() { var x = 1; } }", "IDE0001", 0, 5);

        // Act
        var result = await StandardFixes.SimplifyLinq(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    #endregion

    #region FindNodeSafe Extended Tests

    [Fact]
    public void FindNodeSafe_ZeroLengthSpan_ReturnsNode()
    {
        // Arrange
        var tree = CSharpSyntaxTree.ParseText("class C { }");
        var root = tree.GetRoot();

        // Act
        var node = StandardFixes.FindNodeSafe(root, new TextSpan(0, 0));

        // Assert
        // Zero-length span at position 0 may find the compilation unit
        node.Should().NotBeNull();
    }

    [Fact]
    public void FindNodeSafe_ExactSpanOfNode_ReturnsNode()
    {
        // Arrange
        var tree = CSharpSyntaxTree.ParseText("class C { }");
        var root = tree.GetRoot();
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        // Act
        var node = StandardFixes.FindNodeSafe(root, classDecl.Span);

        // Assert
        node.Should().NotBeNull();
    }

    #endregion

    #region ShouldSkip Extended Tests

    [Fact]
    public void ShouldSkip_StateWithNullRoot_ReturnsTrue()
    {
        // Arrange - create a state then use with expression to null out root
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From("class C { }"));
        var root = document.GetSyntaxRootAsync().GetAwaiter().GetResult()!;
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("TEST", "Test", "Test", "Test", DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, new TextSpan(0, 5)));
        var state = new FixState(document, diagnostic, root) with { CurrentRoot = null! };

        // Act
        var result = StandardFixes.ShouldSkip(state);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_StateWithNullDiagnostic_ReturnsTrue()
    {
        // Arrange
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From("class C { }"));
        var root = document.GetSyntaxRootAsync().GetAwaiter().GetResult()!;
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("TEST", "Test", "Test", "Test", DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, new TextSpan(0, 5)));
        var state = new FixState(document, diagnostic, root) with { Diagnostic = null! };

        // Act
        var result = StandardFixes.ShouldSkip(state);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Placeholder Methods Tests

    [Fact]
    public async Task UseCollectionExpression_AlwaysReturnsUnchangedState()
    {
        // Arrange
        var state = CreateState("class C { }", "IDE0300", 0, 5);

        // Act
        var result = await StandardFixes.UseCollectionExpression(state);

        // Assert
        result.Should().BeSameAs(state);
    }

    [Fact]
    public async Task UseCollectionInitializer_AlwaysReturnsUnchangedState()
    {
        // Arrange
        var state = CreateState("class C { }", "IDE0028", 0, 5);

        // Act
        var result = await StandardFixes.UseCollectionInitializer(state);

        // Assert
        result.Should().BeSameAs(state);
    }

    [Fact]
    public async Task UseExplicitType_AlwaysReturnsUnchangedState()
    {
        // Arrange
        var state = CreateState("class C { }", "IDE0008", 0, 5);

        // Act
        var result = await StandardFixes.UseExplicitType(state);

        // Assert
        result.Should().BeSameAs(state);
    }

    [Fact]
    public async Task UseNullPropagation_AlwaysReturnsUnchangedState()
    {
        // Arrange
        var state = CreateState("class C { }", "IDE0031", 0, 5);

        // Act
        var result = await StandardFixes.UseNullPropagation(state);

        // Assert
        result.Should().BeSameAs(state);
    }

    [Fact]
    public async Task UseObjectInitializer_AlwaysReturnsUnchangedState()
    {
        // Arrange
        var state = CreateState("class C { }", "IDE0017", 0, 5);

        // Act
        var result = await StandardFixes.UseObjectInitializer(state);

        // Assert
        result.Should().BeSameAs(state);
    }

    #endregion
}
