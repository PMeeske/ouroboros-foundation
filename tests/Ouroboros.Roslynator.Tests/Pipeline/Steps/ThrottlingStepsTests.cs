using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;
using Ouroboros.Roslynator.Pipeline.Steps;

namespace Ouroboros.Tests.Pipeline.Steps;

[Trait("Category", "Unit")]
public sealed class ThrottlingStepsTests
{
    private static FixState CreateState(string code = "class C { }")
    {
        using var workspace = new AdhocWorkspace();
        var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
        var document = project.AddDocument("Test.cs", SourceText.From(code));
        var root = document.GetSyntaxRootAsync().GetAwaiter().GetResult()!;
        var diagnostic = Diagnostic.Create(
            new DiagnosticDescriptor("TEST001", "Test", "Test message", "Test", DiagnosticSeverity.Warning, true),
            Location.Create(root.SyntaxTree, new TextSpan(0, 5)));
        return new FixState(document, diagnostic, root);
    }

    [Fact]
    public void WithLock_NullInnerStep_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ThrottlingSteps.WithLock(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task WithLock_InvokesInnerStep()
    {
        // Arrange
        var state = CreateState();
        bool innerCalled = false;
        Func<FixState, Task<FixState>> innerStep = s =>
        {
            innerCalled = true;
            return Task.FromResult(s);
        };

        // Act
        var lockedStep = ThrottlingSteps.WithLock(innerStep);
        await lockedStep(state);

        // Assert
        innerCalled.Should().BeTrue();
    }

    [Fact]
    public async Task WithLock_SkipsWhenChangesExist()
    {
        // Arrange
        var state = CreateState();
        var newRoot = CSharpSyntaxTree.ParseText("class D { }").GetRoot();
        var modifiedState = state.WithNewRoot(newRoot, "Previous change");

        bool innerCalled = false;
        Func<FixState, Task<FixState>> innerStep = s =>
        {
            innerCalled = true;
            return Task.FromResult(s);
        };

        // Act
        var lockedStep = ThrottlingSteps.WithLock(innerStep);
        var result = await lockedStep(modifiedState);

        // Assert
        innerCalled.Should().BeFalse();
        result.Should().BeSameAs(modifiedState);
    }

    [Fact]
    public async Task WithLock_ReturnsInnerStepResult()
    {
        // Arrange
        var state = CreateState();
        var newRoot = CSharpSyntaxTree.ParseText("class Modified { }").GetRoot();
        Func<FixState, Task<FixState>> innerStep = s =>
            Task.FromResult(s.WithNewRoot(newRoot, "Inner fix"));

        // Act
        var lockedStep = ThrottlingSteps.WithLock(innerStep);
        var result = await lockedStep(state);

        // Assert
        result.Changes.Should().Contain("Inner fix");
    }

    [Fact]
    public async Task WithLock_ConcurrentExecution_LimitsParallelism()
    {
        // Arrange
        var state = CreateState();
        int concurrentCount = 0;
        int maxConcurrent = 0;
        object lockObj = new();

        Func<FixState, Task<FixState>> innerStep = async s =>
        {
            lock (lockObj) { concurrentCount++; maxConcurrent = Math.Max(maxConcurrent, concurrentCount); }
            await Task.Delay(50);
            lock (lockObj) { concurrentCount--; }
            return s;
        };

        var lockedStep = ThrottlingSteps.WithLock(innerStep);

        // Act - run 4 tasks in parallel (semaphore allows 2)
        var tasks = Enumerable.Range(0, 4)
            .Select(_ => lockedStep(state))
            .ToArray();
        await Task.WhenAll(tasks);

        // Assert - semaphore is 2, so max concurrent should be <= 2
        maxConcurrent.Should().BeLessThanOrEqualTo(2);
    }
}
