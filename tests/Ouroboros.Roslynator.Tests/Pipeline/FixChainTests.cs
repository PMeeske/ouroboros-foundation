using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;

namespace Ouroboros.Tests.Pipeline;

/// <summary>
/// Tests for the abstract FixChain class via a concrete implementation.
/// </summary>
[Trait("Category", "Unit")]
public sealed class FixChainTests
{
    /// <summary>
    /// Concrete test implementation of FixChain that applies a simple identity pipeline.
    /// </summary>
    private sealed class IdentityChain : FixChain
    {
        public override string Title => "Identity Fix";
        public override string EquivalenceKey => "Test.Identity";

        protected override Future<FixState> DefinePipeline(Future<FixState> input) => input;
    }

    /// <summary>
    /// Concrete test implementation that modifies the syntax tree.
    /// </summary>
    private sealed class ModifyingChain : FixChain
    {
        public override string Title => "Modifying Fix";
        public override string EquivalenceKey => "Test.Modify";

        protected override Future<FixState> DefinePipeline(Future<FixState> input)
        {
            Func<FixState, FixState> modify = state =>
            {
                var newRoot = CSharpSyntaxTree.ParseText("class Modified { }").GetRoot();
                return state.WithNewRoot(newRoot, "Modified by chain");
            };
            return input | modify;
        }
    }

    private static (Document Document, SyntaxNode Root, Diagnostic Diagnostic) CreateTestContext(
        string code = "class C { }")
    {
        using var workspace = new AdhocWorkspace();
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
    public void Title_ReturnsExpectedValue()
    {
        // Arrange
        var chain = new IdentityChain();

        // Assert
        chain.Title.Should().Be("Identity Fix");
    }

    [Fact]
    public void EquivalenceKey_ReturnsExpectedValue()
    {
        // Arrange
        var chain = new IdentityChain();

        // Assert
        chain.EquivalenceKey.Should().Be("Test.Identity");
    }

    [Fact]
    public async Task ExecuteAsync_IdentityPipeline_ReturnsOriginalDocument()
    {
        // Arrange
        var (document, _, diagnostic) = CreateTestContext();
        var chain = new IdentityChain();

        // Act
        var result = await chain.ExecuteAsync(document, diagnostic, CancellationToken.None);

        // Assert
        // Identity pipeline has no changes, so should return original document
        result.Should().BeSameAs(document);
    }

    [Fact]
    public async Task ExecuteAsync_ModifyingPipeline_ReturnsChangedDocument()
    {
        // Arrange
        var (document, _, diagnostic) = CreateTestContext();
        var chain = new ModifyingChain();

        // Act
        var result = await chain.ExecuteAsync(document, diagnostic, CancellationToken.None);

        // Assert
        result.Should().NotBeSameAs(document);
        var newRoot = await result.GetSyntaxRootAsync();
        newRoot!.ToFullString().Should().Contain("Modified");
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_PassesTokenThrough()
    {
        // Arrange
        var (document, _, diagnostic) = CreateTestContext();
        var chain = new IdentityChain();
        using var cts = new CancellationTokenSource();

        // Act - should complete without throwing
        var result = await chain.ExecuteAsync(document, diagnostic, cts.Token);

        // Assert
        result.Should().NotBeNull();
    }
}
