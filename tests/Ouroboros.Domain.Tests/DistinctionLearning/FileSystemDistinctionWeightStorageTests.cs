using Ouroboros.Core.DistinctionLearning;
using Ouroboros.Domain.DistinctionLearning;

namespace Ouroboros.Tests.DistinctionLearning;

[Trait("Category", "Unit")]
public class FileSystemDistinctionWeightStorageTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FileSystemDistinctionWeightStorage _storage;

    public FileSystemDistinctionWeightStorageTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _storage = new FileSystemDistinctionWeightStorage(new DistinctionStorageConfig(_tempDir));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void Constructor_WithNullConfig_ShouldThrow()
    {
        var act = () => new FileSystemDistinctionWeightStorage(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task StoreWeightsAsync_ShouldWriteFile()
    {
        var metadata = new DistinctionWeightMetadata("id1", "", 0.8, "Distinction", DateTime.UtcNow, false, 100);
        var result = await _storage.StoreWeightsAsync("id1", new byte[] { 1, 2, 3 }, metadata);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task LoadWeightsAsync_AfterStore_ShouldReturnData()
    {
        var metadata = new DistinctionWeightMetadata("id2", "", 0.8, "Distinction", DateTime.UtcNow, false, 5);
        await _storage.StoreWeightsAsync("id2", new byte[] { 10, 20, 30 }, metadata);

        var result = await _storage.LoadWeightsAsync("id2");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new byte[] { 10, 20, 30 });
    }

    [Fact]
    public async Task LoadWeightsAsync_NonExistent_ShouldReturnFailure()
    {
        var result = await _storage.LoadWeightsAsync("nonexistent");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ListWeightsAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        var result = await _storage.ListWeightsAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTotalStorageSizeAsync_ShouldReturnSize()
    {
        var metadata = new DistinctionWeightMetadata("id3", "", 0.8, "Distinction", DateTime.UtcNow, false, 5);
        await _storage.StoreWeightsAsync("id3", new byte[] { 1, 2, 3, 4, 5 }, metadata);

        var result = await _storage.GetTotalStorageSizeAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThanOrEqualTo(5);
    }
}
