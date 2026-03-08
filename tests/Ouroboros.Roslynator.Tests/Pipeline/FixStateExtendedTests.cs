using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;

namespace Ouroboros.Tests.Pipeline;

/// <summary>
/// Extended tests for FixState covering record semantics, immutability,
/// and additional edge cases.
/// </summary>
[Trait("Category", "Unit")]
public sealed class FixStateExtendedTests
{
    private static (Document Document, SyntaxNode Root, Diagnostic Diagnostic) CreateTestContext(
        string code = "class C { }")
    {
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = document.GetSyntaxRootAsync().GetAwaiter().GetResult()!;
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("TEST001", "Test", "Test message", "Test",
                DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, new TextSpan(0, 5)));
        return (document, root, diagnostic);
    }

    [Fact]
    public void WithNewRoot_DoesNotMutateOriginal()
    {
        // Arrange
        var (document, root, diagnostic) = CreateTestContext();
        var state = new FixState(document, diagnostic, root);
        var newRoot = CSharpSyntaxTree.ParseText("class D { }").GetRoot();

        // Act
        var newState = state.WithNewRoot(newRoot, "Change");

        // Assert - original unchanged
        state.CurrentRoot.Should().BeSameAs(root);
        state.Changes.Should().BeEmpty();

        // New state has changes
        newState.CurrentRoot.Should().BeSameAs(newRoot);
        newState.Changes.Should().HaveCount(1);
    }

    [Fact]
    public void WithCancellation_DoesNotMutateOriginal()
    {
        // Arrange
        var (document, root, diagnostic) = CreateTestContext();
        var state = new FixState(document, diagnostic, root);
        using var cts = new CancellationTokenSource();

        // Act
        var newState = state.WithCancellation(cts.Token);

        // Assert
        state.CancellationToken.Should().Be(CancellationToken.None);
        newState.CancellationToken.Should().Be(cts.Token);
    }

    [Fact]
    public void WithNewRoot_ChainedChanges_AccumulateCorrectly()
    {
        // Arrange
        var (document, root, diagnostic) = CreateTestContext();
        var state = new FixState(document, diagnostic, root);

        // Act
        var state1 = state.WithNewRoot(root, "Step 1");
        var state2 = state1.WithNewRoot(root, "Step 2");
        var state3 = state2.WithNewRoot(root, "Step 3");

        // Assert
        state3.Changes.Should().HaveCount(3);
        state3.Changes[0].Should().Be("Step 1");
        state3.Changes[1].Should().Be("Step 2");
        state3.Changes[2].Should().Be("Step 3");
    }

    [Fact]
    public void Changes_IsEmpty_WhenNewlyCreated()
    {
        // Arrange
        var (document, root, diagnostic) = CreateTestContext();

        // Act
        var state = new FixState(document, diagnostic, root);

        // Assert
        state.Changes.IsEmpty.Should().BeTrue();
        state.Changes.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void RecordWithSyntax_CanUpdateDocument()
    {
        // Arrange
        var (document, root, diagnostic) = CreateTestContext();
        var state = new FixState(document, diagnostic, root);

        var workspace2 = new AdhocWorkspace();
        var project2 = workspace2.AddProject("TestProject2", LanguageNames.CSharp);
        var document2 = project2.AddDocument("Test2.cs", SourceText.From("class D { }"));

        // Act
        var newState = state with { Document = document2 };

        // Assert
        newState.Document.Should().BeSameAs(document2);
        newState.Diagnostic.Should().BeSameAs(diagnostic);
        newState.CurrentRoot.Should().BeSameAs(root);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        var (document, root, diagnostic) = CreateTestContext();
        var state1 = new FixState(document, diagnostic, root);
        var state2 = new FixState(document, diagnostic, root);

        // Assert - record equality
        state1.Should().Be(state2);
    }

    [Fact]
    public void Equality_DifferentChanges_AreNotEqual()
    {
        // Arrange
        var (document, root, diagnostic) = CreateTestContext();
        var state1 = new FixState(document, diagnostic, root);
        var state2 = state1.WithNewRoot(root, "Modified");

        // Assert
        state1.Should().NotBe(state2);
    }
}
