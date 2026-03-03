using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;
using Ouroboros.Roslynator.Pipeline.Steps;

namespace Ouroboros.Tests.Pipeline.Steps;

/// <summary>
/// Unit tests for OllamaSteps covering the skip-when-changes-exist path,
/// cancellation propagation, and HTTP failure graceful degradation.
/// Note: The actual Ollama API call will fail in tests (no server) but the
/// method handles HttpRequestException gracefully, returning unchanged state.
/// </summary>
[Trait("Category", "Unit")]
public sealed class OllamaStepsTests
{
    #region Helpers

    private static FixState CreateState(string code = "class C { }")
    {
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = document.GetSyntaxRootAsync().GetAwaiter().GetResult()!;
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("CS0168", "Test", "Unused variable", "Test",
                DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, new TextSpan(0, 5)));
        return new FixState(document, diagnostic, root);
    }

    #endregion

    #region Skip When Changes Exist

    [Fact]
    public async Task GenerateFix_StateHasChanges_SkipsAndReturnsUnchanged()
    {
        // Arrange
        var state = CreateState();
        var newRoot = CSharpSyntaxTree.ParseText("class Modified { }").GetRoot();
        var modifiedState = state.WithNewRoot(newRoot, "Previous fix");

        // Act
        var result = await OllamaSteps.GenerateFix(modifiedState);

        // Assert - should return immediately without calling API
        result.Should().BeSameAs(modifiedState);
    }

    [Fact]
    public async Task GenerateFix_StateHasMultipleChanges_SkipsAndReturnsUnchanged()
    {
        // Arrange
        var state = CreateState();
        var root1 = CSharpSyntaxTree.ParseText("class A { }").GetRoot();
        var root2 = CSharpSyntaxTree.ParseText("class B { }").GetRoot();
        var modifiedState = state.WithNewRoot(root1, "Fix 1").WithNewRoot(root2, "Fix 2");

        // Act
        var result = await OllamaSteps.GenerateFix(modifiedState);

        // Assert
        result.Should().BeSameAs(modifiedState);
        result.Changes.Should().HaveCount(2);
    }

    #endregion

    #region Graceful Failure on No Server

    [Fact]
    public async Task GenerateFix_NoOllamaServer_ReturnsUnchangedState()
    {
        // Arrange - no Ollama server running, will get HttpRequestException
        var state = CreateState("class C { void M() { int x; } }");

        // Act - should not throw, should return unchanged state
        var result = await OllamaSteps.GenerateFix(state);

        // Assert
        result.Changes.Should().BeEmpty();
    }

    #endregion

    #region Cancellation

    [Fact]
    public async Task GenerateFix_CancelledToken_ThrowsOperationCancelledException()
    {
        // Arrange
        var state = CreateState("class C { void M() { int x; } }");
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var cancelledState = state.WithCancellation(cts.Token);

        // Act & Assert - OperationCanceledException should propagate
        // The method catches HttpRequestException and JsonException but rethrows OperationCanceledException
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => OllamaSteps.GenerateFix(cancelledState));
    }

    #endregion
}
