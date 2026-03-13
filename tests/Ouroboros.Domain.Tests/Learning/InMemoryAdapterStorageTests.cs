using Ouroboros.Abstractions;
using Ouroboros.Core.Learning;
using Ouroboros.Domain.Learning;

namespace Ouroboros.Tests.Learning;

[Trait("Category", "Unit")]
public class InMemoryAdapterStorageTests
{
    private static AdapterMetadata MakeMetadata(AdapterId? id = null, string taskName = "task1") =>
        AdapterMetadata.Create(id ?? AdapterId.NewId(), taskName, AdapterConfig.Default(), "/path/to/blob");

    [Fact]
    public async Task StoreMetadataAsync_ShouldSucceed()
    {
        var storage = new InMemoryAdapterStorage();
        var metadata = MakeMetadata();

        var result = await storage.StoreMetadataAsync(metadata);

        result.IsSuccess.Should().BeTrue();
        storage.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetMetadataAsync_ExistingId_ShouldReturnMetadata()
    {
        var storage = new InMemoryAdapterStorage();
        var metadata = MakeMetadata();
        await storage.StoreMetadataAsync(metadata);

        var result = await storage.GetMetadataAsync(metadata.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.TaskName.Should().Be("task1");
    }

    [Fact]
    public async Task GetMetadataAsync_NonExistentId_ShouldReturnFailure()
    {
        var storage = new InMemoryAdapterStorage();
        var result = await storage.GetMetadataAsync(AdapterId.NewId());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetAdaptersByTaskAsync_ShouldFilterByTaskName()
    {
        var storage = new InMemoryAdapterStorage();
        await storage.StoreMetadataAsync(MakeMetadata(taskName: "alpha"));
        await storage.StoreMetadataAsync(MakeMetadata(taskName: "beta"));
        await storage.StoreMetadataAsync(MakeMetadata(taskName: "alpha"));

        var result = await storage.GetAdaptersByTaskAsync("alpha");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateMetadataAsync_ExistingId_ShouldSucceed()
    {
        var storage = new InMemoryAdapterStorage();
        var metadata = MakeMetadata();
        await storage.StoreMetadataAsync(metadata);

        var updated = metadata.WithTraining(100);
        var result = await storage.UpdateMetadataAsync(updated);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateMetadataAsync_NonExistent_ShouldReturnFailure()
    {
        var storage = new InMemoryAdapterStorage();
        var result = await storage.UpdateMetadataAsync(MakeMetadata());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteMetadataAsync_ShouldRemoveEntry()
    {
        var storage = new InMemoryAdapterStorage();
        var metadata = MakeMetadata();
        await storage.StoreMetadataAsync(metadata);

        var result = await storage.DeleteMetadataAsync(metadata.Id);

        result.IsSuccess.Should().BeTrue();
        storage.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteMetadataAsync_NonExistent_ShouldStillSucceed()
    {
        var storage = new InMemoryAdapterStorage();
        var result = await storage.DeleteMetadataAsync(AdapterId.NewId());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Clear_ShouldRemoveAll()
    {
        var storage = new InMemoryAdapterStorage();
        storage.StoreMetadataAsync(MakeMetadata()).GetAwaiter().GetResult();
        storage.StoreMetadataAsync(MakeMetadata()).GetAwaiter().GetResult();

        storage.Clear();

        storage.Count.Should().Be(0);
    }

    [Fact]
    public async Task StoreMetadataAsync_WithNull_ShouldReturnFailure()
    {
        var storage = new InMemoryAdapterStorage();
        var result = await storage.StoreMetadataAsync(null!);
        result.IsFailure.Should().BeTrue();
    }
}
