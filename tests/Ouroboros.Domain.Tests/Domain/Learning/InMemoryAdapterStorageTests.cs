using FluentAssertions;
using Ouroboros.Core.Learning;
using Ouroboros.Domain.Learning;
using Xunit;

namespace Ouroboros.Tests.Domain.Learning;

[Trait("Category", "Unit")]
public class InMemoryAdapterStorageTests
{
    private readonly InMemoryAdapterStorage _sut = new();

    private static AdapterMetadata CreateMetadata(string taskName = "test-task", AdapterId? id = null)
    {
        return AdapterMetadata.Create(
            id ?? AdapterId.NewId(),
            taskName,
            AdapterConfig.Default(),
            "/fake/path");
    }

    // ===== StoreMetadataAsync =====

    [Fact]
    public async Task StoreMetadataAsync_ValidMetadata_ShouldSucceed()
    {
        var metadata = CreateMetadata();

        var result = await _sut.StoreMetadataAsync(metadata);

        result.IsSuccess.Should().BeTrue();
        _sut.Count.Should().Be(1);
    }

    [Fact]
    public async Task StoreMetadataAsync_Null_ShouldFail()
    {
        var result = await _sut.StoreMetadataAsync(null!);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("null");
    }

    // ===== GetMetadataAsync =====

    [Fact]
    public async Task GetMetadataAsync_ExistingId_ShouldReturnMetadata()
    {
        var metadata = CreateMetadata();
        await _sut.StoreMetadataAsync(metadata);

        var result = await _sut.GetMetadataAsync(metadata.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.TaskName.Should().Be("test-task");
    }

    [Fact]
    public async Task GetMetadataAsync_NonExistentId_ShouldFail()
    {
        var result = await _sut.GetMetadataAsync(AdapterId.NewId());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    // ===== GetAdaptersByTaskAsync =====

    [Fact]
    public async Task GetAdaptersByTaskAsync_ShouldFilterByTask()
    {
        await _sut.StoreMetadataAsync(CreateMetadata("task-a"));
        await _sut.StoreMetadataAsync(CreateMetadata("task-b"));
        await _sut.StoreMetadataAsync(CreateMetadata("task-a"));

        var result = await _sut.GetAdaptersByTaskAsync("task-a");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAdaptersByTaskAsync_CaseInsensitive_ShouldMatch()
    {
        await _sut.StoreMetadataAsync(CreateMetadata("MyTask"));

        var result = await _sut.GetAdaptersByTaskAsync("mytask");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
    }

    [Fact]
    public async Task GetAdaptersByTaskAsync_EmptyName_ShouldFail()
    {
        var result = await _sut.GetAdaptersByTaskAsync("");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("empty");
    }

    // ===== UpdateMetadataAsync =====

    [Fact]
    public async Task UpdateMetadataAsync_ExistingAdapter_ShouldSucceed()
    {
        var metadata = CreateMetadata();
        await _sut.StoreMetadataAsync(metadata);

        var updated = metadata.WithTraining(10);
        var result = await _sut.UpdateMetadataAsync(updated);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateMetadataAsync_NonExistentAdapter_ShouldFail()
    {
        var metadata = CreateMetadata();

        var result = await _sut.UpdateMetadataAsync(metadata);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task UpdateMetadataAsync_Null_ShouldFail()
    {
        var result = await _sut.UpdateMetadataAsync(null!);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("null");
    }

    // ===== DeleteMetadataAsync =====

    [Fact]
    public async Task DeleteMetadataAsync_ExistingAdapter_ShouldSucceed()
    {
        var metadata = CreateMetadata();
        await _sut.StoreMetadataAsync(metadata);

        var result = await _sut.DeleteMetadataAsync(metadata.Id);

        result.IsSuccess.Should().BeTrue();
        _sut.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteMetadataAsync_NonExistentId_ShouldStillSucceed()
    {
        // Idempotent delete
        var result = await _sut.DeleteMetadataAsync(AdapterId.NewId());

        result.IsSuccess.Should().BeTrue();
    }

    // ===== Clear =====

    [Fact]
    public async Task Clear_ShouldRemoveAll()
    {
        await _sut.StoreMetadataAsync(CreateMetadata());
        await _sut.StoreMetadataAsync(CreateMetadata());

        _sut.Clear();

        _sut.Count.Should().Be(0);
    }

    // ===== Count =====

    [Fact]
    public void Count_WhenEmpty_ShouldBeZero()
    {
        _sut.Count.Should().Be(0);
    }

    [Fact]
    public async Task Count_AfterAdding_ShouldReflectCount()
    {
        await _sut.StoreMetadataAsync(CreateMetadata());
        await _sut.StoreMetadataAsync(CreateMetadata());

        _sut.Count.Should().Be(2);
    }
}
