using Ouroboros.Domain.Events;
using Ouroboros.Domain.Persistence;

namespace Ouroboros.Tests.Persistence;

[Trait("Category", "Unit")]
public class InMemoryEventStoreTests
{
    private readonly InMemoryEventStore _store = new();
    private const string BranchId = "test-branch";

    private static StepExecutionEvent CreateEvent(string tokenName = "TestStep") =>
        StepExecutionEvent.Start(tokenName, Array.Empty<string>(), "TestClass", "Test step", null);

    #region AppendEventsAsync

    [Fact]
    public async Task AppendEventsAsync_SingleEvent_ReturnsVersion0()
    {
        var evt = CreateEvent();

        var version = await _store.AppendEventsAsync(BranchId, new[] { evt });

        version.Should().Be(0);
    }

    [Fact]
    public async Task AppendEventsAsync_MultipleEvents_IncrementsVersion()
    {
        var events = new[] { CreateEvent("A"), CreateEvent("B"), CreateEvent("C") };

        var version = await _store.AppendEventsAsync(BranchId, events);

        version.Should().Be(2); // 0, 1, 2
    }

    [Fact]
    public async Task AppendEventsAsync_EmptyList_ReturnsCurrent()
    {
        await _store.AppendEventsAsync(BranchId, new[] { CreateEvent() });

        var version = await _store.AppendEventsAsync(BranchId, Array.Empty<PipelineEvent>());

        version.Should().Be(0);
    }

    [Fact]
    public async Task AppendEventsAsync_EmptyListNewBranch_ReturnsNeg1()
    {
        var version = await _store.AppendEventsAsync("new-branch", Array.Empty<PipelineEvent>());

        version.Should().Be(-1);
    }

    [Fact]
    public async Task AppendEventsAsync_OptimisticConcurrency_CorrectVersion_Succeeds()
    {
        await _store.AppendEventsAsync(BranchId, new[] { CreateEvent() });

        var version = await _store.AppendEventsAsync(BranchId, new[] { CreateEvent() }, expectedVersion: 0);

        version.Should().Be(1);
    }

    [Fact]
    public async Task AppendEventsAsync_OptimisticConcurrency_WrongVersion_ThrowsConcurrencyException()
    {
        await _store.AppendEventsAsync(BranchId, new[] { CreateEvent() });

        var act = () => _store.AppendEventsAsync(BranchId, new[] { CreateEvent() }, expectedVersion: 5);

        await act.Should().ThrowAsync<ConcurrencyException>();
    }

    [Fact]
    public async Task AppendEventsAsync_AnyVersion_AlwaysSucceeds()
    {
        await _store.AppendEventsAsync(BranchId, new[] { CreateEvent() });

        // expectedVersion = -1 means "any version"
        var act = () => _store.AppendEventsAsync(BranchId, new[] { CreateEvent() }, expectedVersion: -1);

        await act.Should().NotThrowAsync();
    }

    #endregion

    #region GetEventsAsync

    [Fact]
    public async Task GetEventsAsync_ReturnsAllEvents()
    {
        await _store.AppendEventsAsync(BranchId, new[] { CreateEvent("A"), CreateEvent("B") });

        var events = await _store.GetEventsAsync(BranchId);

        events.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEventsAsync_FromVersion_ReturnsSubset()
    {
        await _store.AppendEventsAsync(BranchId, new[] { CreateEvent("A"), CreateEvent("B"), CreateEvent("C") });

        var events = await _store.GetEventsAsync(BranchId, fromVersion: 2);

        events.Should().ContainSingle();
    }

    [Fact]
    public async Task GetEventsAsync_NonExistentBranch_ReturnsEmpty()
    {
        var events = await _store.GetEventsAsync("nonexistent");

        events.Should().BeEmpty();
    }

    #endregion

    #region GetVersionAsync

    [Fact]
    public async Task GetVersionAsync_ExistingBranch_ReturnsCurrentVersion()
    {
        await _store.AppendEventsAsync(BranchId, new[] { CreateEvent(), CreateEvent() });

        var version = await _store.GetVersionAsync(BranchId);

        version.Should().Be(1);
    }

    [Fact]
    public async Task GetVersionAsync_NonExistentBranch_ReturnsNeg1()
    {
        var version = await _store.GetVersionAsync("nonexistent");

        version.Should().Be(-1);
    }

    #endregion

    #region BranchExistsAsync

    [Fact]
    public async Task BranchExistsAsync_ExistingBranch_ReturnsTrue()
    {
        await _store.AppendEventsAsync(BranchId, new[] { CreateEvent() });

        var exists = await _store.BranchExistsAsync(BranchId);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task BranchExistsAsync_NonExistentBranch_ReturnsFalse()
    {
        var exists = await _store.BranchExistsAsync("nonexistent");

        exists.Should().BeFalse();
    }

    #endregion

    #region DeleteBranchAsync

    [Fact]
    public async Task DeleteBranchAsync_ExistingBranch_RemovesBranch()
    {
        await _store.AppendEventsAsync(BranchId, new[] { CreateEvent() });

        await _store.DeleteBranchAsync(BranchId);

        var exists = await _store.BranchExistsAsync(BranchId);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteBranchAsync_NonExistentBranch_DoesNotThrow()
    {
        var act = () => _store.DeleteBranchAsync("nonexistent");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DeleteBranchAsync_DoesNotAffectOtherBranches()
    {
        await _store.AppendEventsAsync("branch-1", new[] { CreateEvent() });
        await _store.AppendEventsAsync("branch-2", new[] { CreateEvent() });

        await _store.DeleteBranchAsync("branch-1");

        var exists = await _store.BranchExistsAsync("branch-2");
        exists.Should().BeTrue();
    }

    #endregion

    #region Concurrency

    [Fact]
    public async Task ConcurrentAppends_WithOptimisticConcurrency_OneSucceeds()
    {
        await _store.AppendEventsAsync(BranchId, new[] { CreateEvent() }); // version = 0

        int successCount = 0;
        int failureCount = 0;

        var tasks = Enumerable.Range(0, 5).Select(async _ =>
        {
            try
            {
                await _store.AppendEventsAsync(BranchId, new[] { CreateEvent() }, expectedVersion: 0).ConfigureAwait(false);
                Interlocked.Increment(ref successCount);
            }
            catch (ConcurrencyException)
            {
                Interlocked.Increment(ref failureCount);
            }
        });

        await Task.WhenAll(tasks);

        successCount.Should().BeGreaterThanOrEqualTo(1);
        (successCount + failureCount).Should().Be(5);
    }

    #endregion
}
