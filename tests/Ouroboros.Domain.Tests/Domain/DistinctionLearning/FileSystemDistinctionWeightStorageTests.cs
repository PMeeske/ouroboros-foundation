// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.DistinctionLearning;

using Ouroboros.Core.DistinctionLearning;
using Ouroboros.Core.Monads;
using Ouroboros.Domain.DistinctionLearning;

/// <summary>
/// Tests for <see cref="FileSystemDistinctionWeightStorage"/> using temp directories.
/// </summary>
[Trait("Category", "Unit")]
public class FileSystemDistinctionWeightStorageTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FileSystemDistinctionWeightStorage _sut;

    public FileSystemDistinctionWeightStorageTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ouroboros-weights-" + Guid.NewGuid().ToString("N")[..8]);
        var config = new DistinctionStorageConfig(_tempDir);
        _sut = new FileSystemDistinctionWeightStorage(config);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }
        catch (IOException) { }
    }

    // ----------------------------------------------------------------
    // Constructor
    // ----------------------------------------------------------------

    [Fact]
    public void Constructor_NullConfig_Throws()
    {
        Action act = () => new FileSystemDistinctionWeightStorage(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_CreatesStorageDirectory()
    {
        Directory.Exists(_tempDir).Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // StoreWeightsAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task StoreWeightsAsync_ValidWeights_ReturnsSuccessWithFilePath()
    {
        // Arrange
        byte[] weights = new byte[] { 1, 2, 3, 4, 5 };
        var metadata = new DistinctionWeightMetadata("test-id", "TestDistinction", 1024, DateTime.UtcNow);

        // Act
        Result<string, string> result = await _sut.StoreWeightsAsync("test-id", weights, metadata);

        // Assert
        result.IsSuccess.Should().BeTrue();
        File.Exists(result.Value).Should().BeTrue();
    }

    [Fact]
    public async Task StoreWeightsAsync_CreatesFile()
    {
        // Arrange
        byte[] weights = new byte[] { 10, 20, 30 };
        var metadata = new DistinctionWeightMetadata("store-test", "Test", 3, DateTime.UtcNow);

        // Act
        await _sut.StoreWeightsAsync("store-test", weights, metadata);

        // Assert
        string expectedPath = Path.Combine(_tempDir, "store-test.weights");
        File.Exists(expectedPath).Should().BeTrue();
        byte[] stored = await File.ReadAllBytesAsync(expectedPath);
        stored.Should().BeEquivalentTo(weights);
    }

    // ----------------------------------------------------------------
    // LoadWeightsAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task LoadWeightsAsync_ExistingWeights_ReturnsBytes()
    {
        // Arrange
        byte[] weights = new byte[] { 1, 2, 3, 4, 5 };
        var metadata = new DistinctionWeightMetadata("load-test", "Test", 5, DateTime.UtcNow);
        await _sut.StoreWeightsAsync("load-test", weights, metadata);

        // Act
        Result<byte[], string> result = await _sut.LoadWeightsAsync("load-test");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(weights);
    }

    [Fact]
    public async Task LoadWeightsAsync_NonExistentId_ReturnsFailure()
    {
        // Act
        Result<byte[], string> result = await _sut.LoadWeightsAsync("nonexistent");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    // ----------------------------------------------------------------
    // ListWeightsAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task ListWeightsAsync_NoWeightsStored_ReturnsEmptyList()
    {
        // Act
        Result<List<DistinctionWeightMetadata>, string> result = await _sut.ListWeightsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ListWeightsAsync_AfterStore_ContainsMetadata()
    {
        // Arrange
        byte[] weights = new byte[] { 1, 2 };
        var metadata = new DistinctionWeightMetadata("list-test", "Test", 2, DateTime.UtcNow);
        await _sut.StoreWeightsAsync("list-test", weights, metadata);

        // Act
        Result<List<DistinctionWeightMetadata>, string> result = await _sut.ListWeightsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Id.Should().Be("list-test");
    }

    // ----------------------------------------------------------------
    // DissolveWeightsAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task DissolveWeightsAsync_ExistingFile_MovesToDissolved()
    {
        // Arrange
        byte[] weights = new byte[] { 1, 2, 3 };
        var metadata = new DistinctionWeightMetadata("dissolve-test", "Test", 3, DateTime.UtcNow);
        Result<string, string> storeResult = await _sut.StoreWeightsAsync("dissolve-test", weights, metadata);
        string filePath = storeResult.Value;

        // Act
        var result = await _sut.DissolveWeightsAsync(filePath);

        // Assert
        result.IsSuccess.Should().BeTrue();
        File.Exists(filePath).Should().BeFalse();
        File.Exists(filePath + ".dissolved").Should().BeTrue();
    }

    [Fact]
    public async Task DissolveWeightsAsync_NonExistentFile_StillSucceeds()
    {
        // Act
        var result = await _sut.DissolveWeightsAsync("/nonexistent/path");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // GetTotalStorageSizeAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetTotalStorageSizeAsync_EmptyDirectory_ReturnsZero()
    {
        // Act
        Result<long, string> result = await _sut.GetTotalStorageSizeAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact]
    public async Task GetTotalStorageSizeAsync_WithWeights_ReturnsCorrectSize()
    {
        // Arrange
        byte[] weights1 = new byte[] { 1, 2, 3, 4, 5 };
        byte[] weights2 = new byte[] { 10, 20, 30 };
        var metadata1 = new DistinctionWeightMetadata("size-test-1", "Test1", 5, DateTime.UtcNow);
        var metadata2 = new DistinctionWeightMetadata("size-test-2", "Test2", 3, DateTime.UtcNow);
        await _sut.StoreWeightsAsync("size-test-1", weights1, metadata1);
        await _sut.StoreWeightsAsync("size-test-2", weights2, metadata2);

        // Act
        Result<long, string> result = await _sut.GetTotalStorageSizeAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(8); // 5 + 3
    }
}
