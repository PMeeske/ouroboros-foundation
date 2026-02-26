namespace Ouroboros.Tests.Domain.Persistence;

using Ouroboros.Domain.Events;
using Ouroboros.Domain.Persistence;
using Ouroboros.Domain.States;

[Trait("Category", "Unit")]
public class InMemoryEventStoreTests
{
    private readonly InMemoryEventStore _store = new();

    private static ReasoningStep CreateEvent(string kind = "Draft") =>
        new(Guid.NewGuid(), kind, new Draft("text"), DateTime.UtcNow, "prompt");

    [Fact]
    public async Task AppendEventsAsync_SingleEvent_ReturnsVersionZero()
    {
        // Arrange
        var events = new[] { CreateEvent() };

        // Act
        var version = await _store.AppendEventsAsync("branch1", events);

        // Assert
        version.Should().Be(0);
    }

    [Fact]
    public async Task AppendEventsAsync_MultipleEvents_IncrementsVersion()
    {
        // Arrange
        var events = new[] { CreateEvent(), CreateEvent(), CreateEvent() };

        // Act
        var version = await _store.AppendEventsAsync("branch1", events);

        // Assert
        version.Should().Be(2);
    }

    [Fact]
    public async Task AppendEventsAsync_EmptyList_ReturnsCurrentVersion()
    {
        // Arrange
        await _store.AppendEventsAsync("branch1", new[] { CreateEvent() });

        // Act
        var version = await _store.AppendEventsAsync("branch1", Array.Empty<PipelineEvent>());

        // Assert
        version.Should().Be(0);
    }

    [Fact]
    public async Task AppendEventsAsync_EmptyList_NoBranch_ReturnsNegativeOne()
    {
        // Act
        var version = await _store.AppendEventsAsync("nonexistent", Array.Empty<PipelineEvent>());

        // Assert
        version.Should().Be(-1);
    }

    [Fact]
    public async Task AppendEventsAsync_MultipleBranches_TracksIndependently()
    {
        // Act
        await _store.AppendEventsAsync("branch1", new[] { CreateEvent() });
        await _store.AppendEventsAsync("branch2", new[] { CreateEvent(), CreateEvent() });

        // Assert
        var v1 = await _store.GetVersionAsync("branch1");
        var v2 = await _store.GetVersionAsync("branch2");
        v1.Should().Be(0);
        v2.Should().Be(1);
    }

    [Fact]
    public async Task AppendEventsAsync_CorrectExpectedVersion_Succeeds()
    {
        // Arrange
        await _store.AppendEventsAsync("branch1", new[] { CreateEvent() });

        // Act
        var version = await _store.AppendEventsAsync("branch1", new[] { CreateEvent() }, expectedVersion: 0);

        // Assert
        version.Should().Be(1);
    }

    [Fact]
    public async Task AppendEventsAsync_WrongExpectedVersion_ThrowsConcurrencyException()
    {
        // Arrange
        await _store.AppendEventsAsync("branch1", new[] { CreateEvent() });

        // Act
        var act = async () => await _store.AppendEventsAsync(
            "branch1", new[] { CreateEvent() }, expectedVersion: 5);

        // Assert
        await act.Should().ThrowAsync<ConcurrencyException>();
    }

    [Fact]
    public async Task AppendEventsAsync_AnyVersion_AlwaysSucceeds()
    {
        // Arrange
        await _store.AppendEventsAsync("branch1", new[] { CreateEvent() });

        // Act (expectedVersion = -1 means "any version")
        var act = async () => await _store.AppendEventsAsync(
            "branch1", new[] { CreateEvent() }, expectedVersion: -1);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetEventsAsync_ReturnsAllEvents()
    {
        // Arrange
        var e1 = CreateEvent("Draft");
        var e2 = CreateEvent("Critique");
        await _store.AppendEventsAsync("branch1", new PipelineEvent[] { e1, e2 });

        // Act
        var events = await _store.GetEventsAsync("branch1");

        // Assert
        events.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEventsAsync_FromVersion_FiltersCorrectly()
    {
        // Arrange
        await _store.AppendEventsAsync("branch1", new[] { CreateEvent(), CreateEvent(), CreateEvent() });

        // Act
        var events = await _store.GetEventsAsync("branch1", fromVersion: 2);

        // Assert
        events.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetEventsAsync_NonexistentBranch_ReturnsEmpty()
    {
        // Act
        var events = await _store.GetEventsAsync("nonexistent");

        // Assert
        events.Should().BeEmpty();
    }

    [Fact]
    public async Task GetVersionAsync_ExistingBranch_ReturnsVersion()
    {
        // Arrange
        await _store.AppendEventsAsync("branch1", new[] { CreateEvent(), CreateEvent() });

        // Act
        var version = await _store.GetVersionAsync("branch1");

        // Assert
        version.Should().Be(1);
    }

    [Fact]
    public async Task GetVersionAsync_NonexistentBranch_ReturnsNegativeOne()
    {
        // Act
        var version = await _store.GetVersionAsync("nonexistent");

        // Assert
        version.Should().Be(-1);
    }

    [Fact]
    public async Task BranchExistsAsync_ExistingBranch_ReturnsTrue()
    {
        // Arrange
        await _store.AppendEventsAsync("branch1", new[] { CreateEvent() });

        // Act
        var exists = await _store.BranchExistsAsync("branch1");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task BranchExistsAsync_NonexistentBranch_ReturnsFalse()
    {
        // Act
        var exists = await _store.BranchExistsAsync("nonexistent");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteBranchAsync_ExistingBranch_RemovesBranch()
    {
        // Arrange
        await _store.AppendEventsAsync("branch1", new[] { CreateEvent() });

        // Act
        await _store.DeleteBranchAsync("branch1");

        // Assert
        var exists = await _store.BranchExistsAsync("branch1");
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteBranchAsync_NonexistentBranch_DoesNotThrow()
    {
        // Act
        var act = async () => await _store.DeleteBranchAsync("nonexistent");

        // Assert
        await act.Should().NotThrowAsync();
    }
}
