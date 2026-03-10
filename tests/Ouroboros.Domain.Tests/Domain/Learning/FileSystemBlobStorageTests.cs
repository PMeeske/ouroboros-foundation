// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Learning;

using Ouroboros.Abstractions;
using Ouroboros.Core.Learning;
using Ouroboros.Core.Monads;
using Ouroboros.Domain.Learning;

/// <summary>
/// Tests for <see cref="FileSystemBlobStorage"/> using temp directories.
/// </summary>
[Trait("Category", "Unit")]
public class FileSystemBlobStorageTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FileSystemBlobStorage _sut;

    public FileSystemBlobStorageTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "ouroboros-blob-" + Guid.NewGuid().ToString("N")[..8]);
        _sut = new FileSystemBlobStorage(_tempDir);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
        catch (IOException) { }
    }

    // ----------------------------------------------------------------
    // Constructor
    // ----------------------------------------------------------------

    [Fact]
    public void Constructor_EmptyDirectory_Throws()
    {
        Action act = () => new FileSystemBlobStorage("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WhitespaceDirectory_Throws()
    {
        Action act = () => new FileSystemBlobStorage("   ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_CreatesBaseDirectory()
    {
        Directory.Exists(_tempDir).Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // StoreWeightsAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task StoreWeightsAsync_ValidWeights_ReturnsSuccessWithPath()
    {
        // Arrange
        var adapterId = AdapterId.NewId();
        byte[] weights = new byte[] { 1, 2, 3, 4 };

        // Act
        Result<string, string> result = await _sut.StoreWeightsAsync(adapterId, weights);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain(_tempDir);
        File.Exists(result.Value).Should().BeTrue();
    }

    [Fact]
    public async Task StoreWeightsAsync_NullWeights_ReturnsFailure()
    {
        // Arrange
        var adapterId = AdapterId.NewId();

        // Act
        Result<string, string> result = await _sut.StoreWeightsAsync(adapterId, null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task StoreWeightsAsync_EmptyWeights_ReturnsFailure()
    {
        // Arrange
        var adapterId = AdapterId.NewId();

        // Act
        Result<string, string> result = await _sut.StoreWeightsAsync(adapterId, Array.Empty<byte>());

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // GetWeightsAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetWeightsAsync_StoredWeights_ReturnsSameBytes()
    {
        // Arrange
        var adapterId = AdapterId.NewId();
        byte[] weights = new byte[] { 10, 20, 30, 40, 50 };
        Result<string, string> storeResult = await _sut.StoreWeightsAsync(adapterId, weights);

        // Act
        Result<byte[], string> result = await _sut.GetWeightsAsync(storeResult.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(weights);
    }

    [Fact]
    public async Task GetWeightsAsync_EmptyPath_ReturnsFailure()
    {
        // Act
        Result<byte[], string> result = await _sut.GetWeightsAsync("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task GetWeightsAsync_NonExistentFile_ReturnsFailure()
    {
        // Act
        Result<byte[], string> result = await _sut.GetWeightsAsync("/nonexistent/path.bin");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    // ----------------------------------------------------------------
    // DeleteWeightsAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task DeleteWeightsAsync_ExistingFile_DeletesAndReturnsSuccess()
    {
        // Arrange
        var adapterId = AdapterId.NewId();
        byte[] weights = new byte[] { 1, 2, 3 };
        Result<string, string> storeResult = await _sut.StoreWeightsAsync(adapterId, weights);
        string path = storeResult.Value;

        // Act
        Result<Unit, string> result = await _sut.DeleteWeightsAsync(path);

        // Assert
        result.IsSuccess.Should().BeTrue();
        File.Exists(path).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteWeightsAsync_NonExistentFile_ReturnsSuccess()
    {
        // Act - idempotent delete
        Result<Unit, string> result = await _sut.DeleteWeightsAsync("/nonexistent/path.bin");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteWeightsAsync_EmptyPath_ReturnsFailure()
    {
        // Act
        Result<Unit, string> result = await _sut.DeleteWeightsAsync("");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // GetWeightsSizeAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetWeightsSizeAsync_StoredWeights_ReturnsCorrectSize()
    {
        // Arrange
        var adapterId = AdapterId.NewId();
        byte[] weights = new byte[] { 1, 2, 3, 4, 5 };
        Result<string, string> storeResult = await _sut.StoreWeightsAsync(adapterId, weights);

        // Act
        Result<long, string> result = await _sut.GetWeightsSizeAsync(storeResult.Value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task GetWeightsSizeAsync_EmptyPath_ReturnsFailure()
    {
        // Act
        Result<long, string> result = await _sut.GetWeightsSizeAsync("");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetWeightsSizeAsync_NonExistentFile_ReturnsFailure()
    {
        // Act
        Result<long, string> result = await _sut.GetWeightsSizeAsync("/nonexistent/path.bin");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }
}
