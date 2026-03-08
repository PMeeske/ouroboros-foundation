using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Ouroboros.Roslynator.Pipeline;
using Ouroboros.Roslynator.Pipeline.Steps;

namespace Ouroboros.Tests.Pipeline.Steps;

/// <summary>
/// Deep coverage tests for ThrottlingSteps covering semaphore behavior,
/// cancellation, error propagation, and skip logic.
/// </summary>
[Trait("Category", "Unit")]
public sealed class ThrottlingStepsDeepTests
{
    #region Helpers

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

    #endregion

    #region WithLock - Argument Validation

    [Fact]
    public void WithLock_NullInnerStep_ThrowsArgumentNullException()
    {
        var act = () => ThrottlingSteps.WithLock(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("innerStep");
    }

    [Fact]
    public void WithLock_ValidInnerStep_ReturnsNonNullFunc()
    {
        Func<FixState, Task<FixState>> step = s => Task.FromResult(s);
        var wrapped = ThrottlingSteps.WithLock(step);
        wrapped.Should().NotBeNull();
    }

    #endregion

    #region WithLock - Execution

    [Fact]
    public async Task WithLock_EmptyChanges_InvokesInnerStep()
    {
        var state = CreateState();
        bool called = false;
        Func<FixState, Task<FixState>> inner = s =>
        {
            called = true;
            return Task.FromResult(s);
        };

        var wrapped = ThrottlingSteps.WithLock(inner);
        await wrapped(state);

        called.Should().BeTrue();
    }

    [Fact]
    public async Task WithLock_HasChanges_SkipsInnerStep()
    {
        var state = CreateState();
        var newRoot = CSharpSyntaxTree.ParseText("class X { }").GetRoot();
        var modifiedState = state.WithNewRoot(newRoot, "Previous");

        bool called = false;
        Func<FixState, Task<FixState>> inner = s =>
        {
            called = true;
            return Task.FromResult(s);
        };

        var wrapped = ThrottlingSteps.WithLock(inner);
        var result = await wrapped(modifiedState);

        called.Should().BeFalse();
        result.Should().BeSameAs(modifiedState);
    }

    [Fact]
    public async Task WithLock_InnerStepReturnsModifiedState_ReturnsIt()
    {
        var state = CreateState();
        var newRoot = CSharpSyntaxTree.ParseText("class Fixed { }").GetRoot();
        Func<FixState, Task<FixState>> inner = s =>
            Task.FromResult(s.WithNewRoot(newRoot, "Fixed"));

        var wrapped = ThrottlingSteps.WithLock(inner);
        var result = await wrapped(state);

        result.Changes.Should().Contain("Fixed");
    }

    #endregion

    #region WithLock - Cancellation

    [Fact]
    public async Task WithLock_CancelledToken_ThrowsOperationCancelledException()
    {
        var state = CreateState();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var stateWithCancel = state.WithCancellation(cts.Token);

        Func<FixState, Task<FixState>> inner = s => Task.FromResult(s);
        var wrapped = ThrottlingSteps.WithLock(inner);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => wrapped(stateWithCancel));
    }

    #endregion

    #region WithLock - Error Propagation and Semaphore Release

    [Fact]
    public async Task WithLock_InnerStepThrows_PropagatesExceptionAndReleasesSemaphore()
    {
        var state = CreateState();
        Func<FixState, Task<FixState>> throwing = _ =>
            throw new InvalidOperationException("inner failed");

        var wrapped = ThrottlingSteps.WithLock(throwing);

        // First call throws
        await Assert.ThrowsAsync<InvalidOperationException>(() => wrapped(state));

        // Second call should succeed (semaphore released)
        bool secondCalled = false;
        Func<FixState, Task<FixState>> ok = s =>
        {
            secondCalled = true;
            return Task.FromResult(s);
        };

        var wrappedOk = ThrottlingSteps.WithLock(ok);
        await wrappedOk(state);
        secondCalled.Should().BeTrue();
    }

    [Fact]
    public async Task WithLock_InnerStepThrowsTaskException_PropagatesAndReleasesLock()
    {
        var state = CreateState();
        Func<FixState, Task<FixState>> throwing = _ =>
            Task.FromException<FixState>(new ArgumentException("task error"));

        var wrapped = ThrottlingSteps.WithLock(throwing);

        await Assert.ThrowsAsync<ArgumentException>(() => wrapped(state));

        // Verify semaphore still works
        bool ok = false;
        var wrappedOk = ThrottlingSteps.WithLock(s =>
        {
            ok = true;
            return Task.FromResult(s);
        });
        await wrappedOk(state);
        ok.Should().BeTrue();
    }

    #endregion

    #region WithLock - Concurrency

    [Fact]
    public async Task WithLock_ParallelExecution_LimitsToSemaphoreCount()
    {
        var state = CreateState();
        int concurrentCount = 0;
        int maxConcurrent = 0;
        object lockObj = new();

        Func<FixState, Task<FixState>> inner = async s =>
        {
            lock (lockObj) { concurrentCount++; maxConcurrent = Math.Max(maxConcurrent, concurrentCount); }
            await Task.Delay(30);
            lock (lockObj) { concurrentCount--; }
            return s;
        };

        var wrapped = ThrottlingSteps.WithLock(inner);

        // Run 6 tasks in parallel (semaphore allows 2)
        var tasks = Enumerable.Range(0, 6).Select(_ => wrapped(state)).ToArray();
        await Task.WhenAll(tasks);

        maxConcurrent.Should().BeLessThanOrEqualTo(2);
    }

    #endregion

    #region WithLock - State with Multiple Changes

    [Fact]
    public async Task WithLock_TwoChanges_SkipsExecution()
    {
        var state = CreateState();
        var root1 = CSharpSyntaxTree.ParseText("class A { }").GetRoot();
        var root2 = CSharpSyntaxTree.ParseText("class B { }").GetRoot();
        var multiChangeState = state.WithNewRoot(root1, "C1").WithNewRoot(root2, "C2");

        bool called = false;
        var wrapped = ThrottlingSteps.WithLock(s =>
        {
            called = true;
            return Task.FromResult(s);
        });

        var result = await wrapped(multiChangeState);

        called.Should().BeFalse();
        result.Changes.Should().HaveCount(2);
    }

    #endregion
}
