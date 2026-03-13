using Ouroboros.Core.Learning;
using Ouroboros.Domain.Learning;

namespace Ouroboros.Tests.Learning;

[Trait("Category", "Unit")]
public class FileSystemBlobStorageTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FileSystemBlobStorage _storage;

    public FileSystemBlobStorageTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _storage = new FileSystemBlobStorage(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public async Task StoreWeightsAsync_ShouldWriteFile()
    {
        var id = AdapterId.NewId();
        var weights = new byte[] { 1, 2, 3, 4, 5 };

        var result = await _storage.StoreWeightsAsync(id, weights);

        result.IsSuccess.Should().BeTrue();
        File.Exists(result.Value).Should().BeTrue();
    }

    [Fact]
    public async Task GetWeightsAsync_AfterStore_ShouldReturnSameData()
    {
        var id = AdapterId.NewId();
        var weights = new byte[] { 10, 20, 30 };
        var storeResult = await _storage.StoreWeightsAsync(id, weights);

        var result = await _storage.GetWeightsAsync(storeResult.Value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(weights);
    }

    [Fact]
    public async Task GetWeightsAsync_NonExistentPath_ShouldReturnFailure()
    {
        var result = await _storage.GetWeightsAsync("/nonexistent/path.bin");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteWeightsAsync_AfterStore_ShouldDeleteFile()
    {
        var id = AdapterId.NewId();
        var storeResult = await _storage.StoreWeightsAsync(id, new byte[] { 1 });
        var path = storeResult.Value;

        var result = await _storage.DeleteWeightsAsync(path);

        result.IsSuccess.Should().BeTrue();
        File.Exists(path).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteWeightsAsync_NonExistentFile_ShouldReturnSuccess()
    {
        var result = await _storage.DeleteWeightsAsync("/nonexistent/file.bin");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task StoreWeightsAsync_EmptyWeights_ShouldReturnFailure()
    {
        var result = await _storage.StoreWeightsAsync(AdapterId.NewId(), Array.Empty<byte>());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetWeightsSizeAsync_AfterStore_ShouldReturnCorrectSize()
    {
        var id = AdapterId.NewId();
        var weights = new byte[] { 1, 2, 3, 4, 5 };
        var storeResult = await _storage.StoreWeightsAsync(id, weights);

        var result = await _storage.GetWeightsSizeAsync(storeResult.Value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public void Constructor_WithEmptyBaseDirectory_ShouldThrow()
    {
        var act = () => new FileSystemBlobStorage("");
        act.Should().Throw<ArgumentException>();
    }
}
