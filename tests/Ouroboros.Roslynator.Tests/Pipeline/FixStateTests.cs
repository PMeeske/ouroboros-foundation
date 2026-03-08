using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;

namespace Ouroboros.Tests.Pipeline;

[Trait("Category", "Unit")]
public sealed class FixStateTests
{
    private static (Document document, SyntaxNode root, Diagnostic diagnostic) CreateTestContext(string code = "class C { }")
    {
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = document.GetSyntaxRootAsync().GetAwaiter().GetResult()!;
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("TEST001", "Test", "Test message", "Test", DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, new TextSpan(0, 5)));
        return (document, root, diagnostic);
    }

    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var (document, root, diagnostic) = CreateTestContext();

        // Act
        var state = new FixState(document, diagnostic, root);

        // Assert
        state.Document.Should().BeSameAs(document);
        state.Diagnostic.Should().BeSameAs(diagnostic);
        state.CurrentRoot.Should().BeSameAs(root);
        state.CancellationToken.Should().Be(CancellationToken.None);
        state.Changes.Should().BeEmpty();
    }

    [Fact]
    public void WithNewRoot_AddsChangeAndUpdatesRoot()
    {
        // Arrange
        var (document, root, diagnostic) = CreateTestContext();
        var state = new FixState(document, diagnostic, root);
        var newRoot = CSharpSyntaxTree.ParseText("class D { }").GetRoot();

        // Act
        var newState = state.WithNewRoot(newRoot, "Renamed class");

        // Assert
        newState.CurrentRoot.Should().BeSameAs(newRoot);
        newState.Changes.Should().HaveCount(1);
        newState.Changes[0].Should().Be("Renamed class");
    }

    [Fact]
    public void WithNewRoot_MultipleChanges_AccumulateChanges()
    {
        // Arrange
        var (document, root, diagnostic) = CreateTestContext();
        var state = new FixState(document, diagnostic, root);
        var newRoot1 = CSharpSyntaxTree.ParseText("class D { }").GetRoot();
        var newRoot2 = CSharpSyntaxTree.ParseText("class E { }").GetRoot();

        // Act
        var state1 = state.WithNewRoot(newRoot1, "Change 1");
        var state2 = state1.WithNewRoot(newRoot2, "Change 2");

        // Assert
        state2.Changes.Should().HaveCount(2);
        state2.Changes[0].Should().Be("Change 1");
        state2.Changes[1].Should().Be("Change 2");
    }

    [Fact]
    public void WithCancellation_SetsCancellationToken()
    {
        // Arrange
        var (document, root, diagnostic) = CreateTestContext();
        var state = new FixState(document, diagnostic, root);
        var cts = new CancellationTokenSource();

        // Act
        var newState = state.WithCancellation(cts.Token);

        // Assert
        newState.CancellationToken.Should().Be(cts.Token);
    }

    [Fact]
    public void WithCancellation_PreservesOtherProperties()
    {
        // Arrange
        var (document, root, diagnostic) = CreateTestContext();
        var state = new FixState(document, diagnostic, root);
        var cts = new CancellationTokenSource();

        // Act
        var newState = state.WithCancellation(cts.Token);

        // Assert
        newState.Document.Should().BeSameAs(document);
        newState.Diagnostic.Should().BeSameAs(diagnostic);
        newState.CurrentRoot.Should().BeSameAs(root);
        newState.Changes.Should().BeEmpty();
    }

    [Fact]
    public void DefaultChanges_IsEmptyImmutableArray()
    {
        // Arrange
        var (document, root, diagnostic) = CreateTestContext();

        // Act
        var state = new FixState(document, diagnostic, root);

        // Assert
        state.Changes.IsEmpty.Should().BeTrue();
        state.Changes.Length.Should().Be(0);
    }
}
