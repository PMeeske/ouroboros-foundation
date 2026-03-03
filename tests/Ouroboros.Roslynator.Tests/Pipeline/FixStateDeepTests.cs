using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;

namespace Ouroboros.Tests.Pipeline;

/// <summary>
/// Deep coverage tests for FixState record covering construction,
/// init-only properties, with-expressions, equality, and edge cases.
/// </summary>
[Trait("Category", "Unit")]
public sealed class FixStateDeepTests
{
    #region Helpers

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

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var (document, root, diagnostic) = CreateTestContext();

        var state = new FixState(document, diagnostic, root);

        state.Document.Should().BeSameAs(document);
        state.Diagnostic.Should().BeSameAs(diagnostic);
        state.CurrentRoot.Should().BeSameAs(root);
        state.CancellationToken.Should().Be(CancellationToken.None);
        state.Changes.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ChangesDefaultToEmptyNotDefault()
    {
        var (document, root, diagnostic) = CreateTestContext();
        var state = new FixState(document, diagnostic, root);

        state.Changes.IsDefault.Should().BeFalse();
        state.Changes.IsEmpty.Should().BeTrue();
        state.Changes.Length.Should().Be(0);
    }

    #endregion

    #region WithNewRoot Tests

    [Fact]
    public void WithNewRoot_UpdatesRootAndAddsChange()
    {
        var (document, root, diagnostic) = CreateTestContext();
        var state = new FixState(document, diagnostic, root);
        var newRoot = CSharpSyntaxTree.ParseText("class D { }").GetRoot();

        var result = state.WithNewRoot(newRoot, "Renamed");

        result.CurrentRoot.Should().BeSameAs(newRoot);
        result.Changes.Should().Equal("Renamed");
    }

    [Fact]
    public void WithNewRoot_DoesNotMutateOriginal()
    {
        var (document, root, diagnostic) = CreateTestContext();
        var state = new FixState(document, diagnostic, root);
        var newRoot = CSharpSyntaxTree.ParseText("class D { }").GetRoot();

        _ = state.WithNewRoot(newRoot, "Change");

        state.CurrentRoot.Should().BeSameAs(root);
        state.Changes.Should().BeEmpty();
    }

    [Fact]
    public void WithNewRoot_ChainingThreeChanges_Accumulates()
    {
        var (document, root, diagnostic) = CreateTestContext();
        var state = new FixState(document, diagnostic, root);

        var s1 = state.WithNewRoot(root, "A");
        var s2 = s1.WithNewRoot(root, "B");
        var s3 = s2.WithNewRoot(root, "C");

        s3.Changes.Should().Equal("A", "B", "C");
        s3.Changes.Length.Should().Be(3);
    }

    [Fact]
    public void WithNewRoot_PreservesDocument()
    {
        var (document, root, diagnostic) = CreateTestContext();
        var state = new FixState(document, diagnostic, root);
        var newRoot = CSharpSyntaxTree.ParseText("class D { }").GetRoot();

        var result = state.WithNewRoot(newRoot, "Change");

        result.Document.Should().BeSameAs(document);
    }

    [Fact]
    public void WithNewRoot_PreservesDiagnostic()
    {
        var (document, root, diagnostic) = CreateTestContext();
        var state = new FixState(document, diagnostic, root);
        var newRoot = CSharpSyntaxTree.ParseText("class D { }").GetRoot();

        var result = state.WithNewRoot(newRoot, "Change");

        result.Diagnostic.Should().BeSameAs(diagnostic);
    }

    [Fact]
    public void WithNewRoot_PreservesCancellationToken()
    {
        var (document, root, diagnostic) = CreateTestContext();
        using var cts = new CancellationTokenSource();
        var state = new FixState(document, diagnostic, root).WithCancellation(cts.Token);
        var newRoot = CSharpSyntaxTree.ParseText("class D { }").GetRoot();

        var result = state.WithNewRoot(newRoot, "Change");

        result.CancellationToken.Should().Be(cts.Token);
    }

    #endregion

    #region WithCancellation Tests

    [Fact]
    public void WithCancellation_SetsToken()
    {
        var (document, root, diagnostic) = CreateTestContext();
        using var cts = new CancellationTokenSource();
        var state = new FixState(document, diagnostic, root);

        var result = state.WithCancellation(cts.Token);

        result.CancellationToken.Should().Be(cts.Token);
    }

    [Fact]
    public void WithCancellation_DoesNotMutateOriginal()
    {
        var (document, root, diagnostic) = CreateTestContext();
        using var cts = new CancellationTokenSource();
        var state = new FixState(document, diagnostic, root);

        _ = state.WithCancellation(cts.Token);

        state.CancellationToken.Should().Be(CancellationToken.None);
    }

    [Fact]
    public void WithCancellation_PreservesAllOtherProperties()
    {
        var (document, root, diagnostic) = CreateTestContext();
        using var cts = new CancellationTokenSource();
        var state = new FixState(document, diagnostic, root)
            .WithNewRoot(root, "Existing change");

        var result = state.WithCancellation(cts.Token);

        result.Document.Should().BeSameAs(document);
        result.Diagnostic.Should().BeSameAs(diagnostic);
        result.CurrentRoot.Should().BeSameAs(root);
        result.Changes.Should().Equal("Existing change");
    }

    [Fact]
    public void WithCancellation_CancelledToken_SetsCorrectly()
    {
        var (document, root, diagnostic) = CreateTestContext();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var state = new FixState(document, diagnostic, root);

        var result = state.WithCancellation(cts.Token);

        result.CancellationToken.IsCancellationRequested.Should().BeTrue();
    }

    #endregion

    #region Record With-Expression Tests

    [Fact]
    public void WithExpression_CanUpdateDocument()
    {
        var (document, root, diagnostic) = CreateTestContext();
        var state = new FixState(document, diagnostic, root);

        var workspace2 = new AdhocWorkspace();
        var project2 = workspace2.AddProject("P2", LanguageNames.CSharp);
        var doc2 = project2.AddDocument("T2.cs", SourceText.From("class D { }"));

        var result = state with { Document = doc2 };

        result.Document.Should().BeSameAs(doc2);
        result.CurrentRoot.Should().BeSameAs(root);
    }

    [Fact]
    public void WithExpression_CanUpdateChanges()
    {
        var (document, root, diagnostic) = CreateTestContext();
        var state = new FixState(document, diagnostic, root);

        var result = state with { Changes = ImmutableArray.Create("manual") };

        result.Changes.Should().Equal("manual");
    }

    [Fact]
    public void WithExpression_CanUpdateCancellationToken()
    {
        var (document, root, diagnostic) = CreateTestContext();
        using var cts = new CancellationTokenSource();
        var state = new FixState(document, diagnostic, root);

        var result = state with { CancellationToken = cts.Token };

        result.CancellationToken.Should().Be(cts.Token);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var (document, root, diagnostic) = CreateTestContext();
        var state1 = new FixState(document, diagnostic, root);
        var state2 = new FixState(document, diagnostic, root);

        state1.Should().Be(state2);
        (state1 == state2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentChanges_AreNotEqual()
    {
        var (document, root, diagnostic) = CreateTestContext();
        var state1 = new FixState(document, diagnostic, root);
        var state2 = state1.WithNewRoot(root, "Change");

        state1.Should().NotBe(state2);
        (state1 != state2).Should().BeTrue();
    }

    [Fact]
    public void Equality_SameChanges_AreEqual()
    {
        var (document, root, diagnostic) = CreateTestContext();
        var state1 = new FixState(document, diagnostic, root).WithNewRoot(root, "X");
        var state2 = new FixState(document, diagnostic, root).WithNewRoot(root, "X");

        // ImmutableArray equality is structural
        state1.Should().Be(state2);
    }

    [Fact]
    public void GetHashCode_EqualStates_SameHashCode()
    {
        var (document, root, diagnostic) = CreateTestContext();
        var state1 = new FixState(document, diagnostic, root);
        var state2 = new FixState(document, diagnostic, root);

        state1.GetHashCode().Should().Be(state2.GetHashCode());
    }

    #endregion

    #region ToString Test

    [Fact]
    public void ToString_ReturnsNonEmpty()
    {
        var (document, root, diagnostic) = CreateTestContext();
        var state = new FixState(document, diagnostic, root);

        state.ToString().Should().NotBeNullOrEmpty();
    }

    #endregion
}
