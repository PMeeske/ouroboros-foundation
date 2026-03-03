using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;
using Ouroboros.Roslynator.Pipeline.Steps;

namespace Ouroboros.Tests.Pipeline.Steps;

/// <summary>
/// Extended tests for ThrottlingSteps covering cancellation, error propagation,
/// and edge cases.
/// </summary>
[Trait("Category", "Unit")]
public sealed class ThrottlingStepsExtendedTests
{
    private static FixState CreateState(string code = "class C { }")
    {
        var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = document.GetSyntaxRootAsync().GetAwaiter().GetResult()!;
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("TEST001", "Test", "Test message", "Test",
                DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, new TextSpan(0, 5)));
        return new FixState(document, diagnostic, root);
    }

    [Fact]
    public async Task WithLock_CancelledToken_ThrowsOperationCancelledException()
    {
        // Arrange
        var state = CreateState();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var stateWithCancellation = state.WithCancellation(cts.Token);

        Func<FixState, Task<FixState>> innerStep = s => Task.FromResult(s);
        var lockedStep = ThrottlingSteps.WithLock(innerStep);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => lockedStep(stateWithCancellation));
    }

    [Fact]
    public async Task WithLock_InnerStepThrows_ReleasesLock()
    {
        // Arrange
        var state = CreateState();
        Func<FixState, Task<FixState>> throwingStep = _ =>
            throw new InvalidOperationException("inner error");

        var lockedStep = ThrottlingSteps.WithLock(throwingStep);

        // Act & Assert - should throw but release the semaphore
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => lockedStep(state));

        // Verify semaphore was released by running another step successfully
        bool secondCallCompleted = false;
        Func<FixState, Task<FixState>> okStep = s =>
        {
            secondCallCompleted = true;
            return Task.FromResult(s);
        };

        var lockedOkStep = ThrottlingSteps.WithLock(okStep);
        await lockedOkStep(state);
        secondCallCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task WithLock_MultipleChangesExist_SkipsExecution()
    {
        // Arrange
        var state = CreateState();
        var root1 = CSharpSyntaxTree.ParseText("class A { }").GetRoot();
        var root2 = CSharpSyntaxTree.ParseText("class B { }").GetRoot();
        var stateWithMultipleChanges = state
            .WithNewRoot(root1, "Change 1")
            .WithNewRoot(root2, "Change 2");

        bool innerCalled = false;
        Func<FixState, Task<FixState>> innerStep = s =>
        {
            innerCalled = true;
            return Task.FromResult(s);
        };

        // Act
        var lockedStep = ThrottlingSteps.WithLock(innerStep);
        var result = await lockedStep(stateWithMultipleChanges);

        // Assert
        innerCalled.Should().BeFalse();
        result.Changes.Should().HaveCount(2);
    }

    [Fact]
    public async Task WithLock_InnerStepModifiesState_ReturnsModifiedState()
    {
        // Arrange
        var state = CreateState();
        var newRoot = CSharpSyntaxTree.ParseText("class Fixed { }").GetRoot();
        Func<FixState, Task<FixState>> fixStep = s =>
            Task.FromResult(s.WithNewRoot(newRoot, "Applied fix"));

        // Act
        var lockedStep = ThrottlingSteps.WithLock(fixStep);
        var result = await lockedStep(state);

        // Assert
        result.Changes.Should().Contain("Applied fix");
        result.CurrentRoot.ToFullString().Should().Contain("Fixed");
    }
}
