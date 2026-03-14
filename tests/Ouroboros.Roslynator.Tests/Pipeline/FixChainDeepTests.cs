using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;

namespace Ouroboros.Tests.Pipeline;

/// <summary>
/// Deep coverage tests for FixChain covering pipeline execution,
/// cancellation, chaining, and the PipelineBuilder virtual property.
/// </summary>
[Trait("Category", "Unit")]
public sealed class FixChainDeepTests
{
    #region Test Implementations

    private sealed class IdentityChain : FixChain
    {
        public override string Title => "Identity";
        public override string EquivalenceKey => "Test.Identity";
        protected override Future<FixState> DefinePipeline(Future<FixState> input) => input;
    }

    private sealed class ModifyingChain : FixChain
    {
        public override string Title => "Modify";
        public override string EquivalenceKey => "Test.Modify";

        protected override Future<FixState> DefinePipeline(Future<FixState> input)
        {
            Func<FixState, FixState> modify = state =>
            {
                var newRoot = CSharpSyntaxTree.ParseText("class Modified { }").GetRoot();
                return state.WithNewRoot(newRoot, "Modified");
            };
            return input | modify;
        }
    }

    private sealed class MultiStepChain : FixChain
    {
        public override string Title => "MultiStep";
        public override string EquivalenceKey => "Test.MultiStep";

        protected override Future<FixState> DefinePipeline(Future<FixState> input)
        {
            Func<FixState, FixState> step1 = state =>
            {
                var newRoot = CSharpSyntaxTree.ParseText("class Step1 { }").GetRoot();
                return state.WithNewRoot(newRoot, "Step 1");
            };
            Func<FixState, Task<FixState>> step2 = state =>
            {
                var newRoot = CSharpSyntaxTree.ParseText("class Step2 { }").GetRoot();
                return Task.FromResult(state.WithNewRoot(newRoot, "Step 2"));
            };
            return input | step1 | step2;
        }
    }

    private sealed class CancellationAwareChain : FixChain
    {
        public override string Title => "CancelAware";
        public override string EquivalenceKey => "Test.CancelAware";

        protected override Future<FixState> DefinePipeline(Future<FixState> input)
        {
            Func<FixState, Task<FixState>> checkCancel = async state =>
            {
                state.CancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
                return state.WithNewRoot(state.CurrentRoot, "Checked");
            };
            return input | checkCancel;
        }
    }

    private sealed class ThrowingChain : FixChain
    {
        public override string Title => "Throwing";
        public override string EquivalenceKey => "Test.Throwing";

        protected override Future<FixState> DefinePipeline(Future<FixState> input)
        {
            Func<FixState, FixState> throwStep = _ =>
                throw new InvalidOperationException("Pipeline step failed");
            return input | throwStep;
        }
    }

    #endregion

    #region Helpers

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

    #endregion

    #region ExecuteAsync Tests

    [Fact]
    public async Task ExecuteAsync_IdentityPipeline_ReturnsOriginalDocument()
    {
        var (document, _, diagnostic) = CreateTestContext();
        var chain = new IdentityChain();

        var result = await chain.ExecuteAsync(document, diagnostic, CancellationToken.None);

        result.Should().BeSameAs(document);
    }

    [Fact]
    public async Task ExecuteAsync_ModifyingPipeline_ReturnsNewDocument()
    {
        var (document, _, diagnostic) = CreateTestContext();
        var chain = new ModifyingChain();

        var result = await chain.ExecuteAsync(document, diagnostic, CancellationToken.None);

        result.Should().NotBeSameAs(document);
        var newRoot = await result.GetSyntaxRootAsync();
        newRoot!.ToFullString().Should().Contain("Modified");
    }

    [Fact]
    public async Task ExecuteAsync_MultiStepPipeline_ExecutesBothSteps()
    {
        var (document, _, diagnostic) = CreateTestContext();
        var chain = new MultiStepChain();

        var result = await chain.ExecuteAsync(document, diagnostic, CancellationToken.None);

        result.Should().NotBeSameAs(document);
        var newRoot = await result.GetSyntaxRootAsync();
        // The final step produces "class Step2 { }"
        newRoot!.ToFullString().Should().Contain("Step2");
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellationToken_PassesTokenToState()
    {
        var (document, _, diagnostic) = CreateTestContext();
        var chain = new CancellationAwareChain();
        using var cts = new CancellationTokenSource();

        // Should not throw since token is not cancelled
        var result = await chain.ExecuteAsync(document, diagnostic, cts.Token);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithCancelledToken_Throws()
    {
        var (document, _, diagnostic) = CreateTestContext();
        var chain = new CancellationAwareChain();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => chain.ExecuteAsync(document, diagnostic, cts.Token));
    }

    [Fact]
    public async Task ExecuteAsync_ThrowingPipeline_PropagatesException()
    {
        var (document, _, diagnostic) = CreateTestContext();
        var chain = new ThrowingChain();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => chain.ExecuteAsync(document, diagnostic, CancellationToken.None));
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Title_ReturnsExpectedValue()
    {
        new IdentityChain().Title.Should().Be("Identity");
        new ModifyingChain().Title.Should().Be("Modify");
        new MultiStepChain().Title.Should().Be("MultiStep");
    }

    [Fact]
    public void EquivalenceKey_ReturnsExpectedValue()
    {
        new IdentityChain().EquivalenceKey.Should().Be("Test.Identity");
        new ModifyingChain().EquivalenceKey.Should().Be("Test.Modify");
        new MultiStepChain().EquivalenceKey.Should().Be("Test.MultiStep");
    }

    #endregion

    #region Document Integrity

    [Fact]
    public async Task ExecuteAsync_NoChanges_ReturnsSameDocumentInstance()
    {
        var (document, _, diagnostic) = CreateTestContext("class C { int x; }");
        var chain = new IdentityChain();

        var result = await chain.ExecuteAsync(document, diagnostic, CancellationToken.None);

        // Identity chain produces no changes, so the exact same document instance is returned
        result.Should().BeSameAs(document);
    }

    [Fact]
    public async Task ExecuteAsync_WithChanges_ReturnsDocumentWithNewRoot()
    {
        var (document, _, diagnostic) = CreateTestContext();
        var chain = new ModifyingChain();

        var result = await chain.ExecuteAsync(document, diagnostic, CancellationToken.None);
        var resultRoot = await result.GetSyntaxRootAsync();
        var originalRoot = await document.GetSyntaxRootAsync();

        resultRoot!.ToFullString().Should().NotBe(originalRoot!.ToFullString());
    }

    #endregion
}
