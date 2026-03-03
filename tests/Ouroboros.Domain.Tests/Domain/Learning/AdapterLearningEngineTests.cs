namespace Ouroboros.Tests.Domain.Learning;

using Ouroboros.Abstractions;
using Ouroboros.Core.Learning;
using Ouroboros.Core.Monads;
using Ouroboros.Domain.Learning;

[Trait("Category", "Unit")]
public class AdapterLearningEngineTests
{
    private readonly Mock<IPeftIntegration> _peft = new();
    private readonly Mock<IAdapterStorage> _storage = new();
    private readonly Mock<IAdapterBlobStorage> _blobStorage = new();
    private readonly AdapterLearningEngine _sut;

    public AdapterLearningEngineTests()
    {
        _sut = new AdapterLearningEngine(_peft.Object, _storage.Object, _blobStorage.Object, "base-model");
    }

    [Fact]
    public void Constructor_NullPeft_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AdapterLearningEngine(null!, _storage.Object, _blobStorage.Object, "model"));
    }

    [Fact]
    public void Constructor_NullStorage_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AdapterLearningEngine(_peft.Object, null!, _blobStorage.Object, "model"));
    }

    [Fact]
    public void Constructor_NullBlobStorage_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AdapterLearningEngine(_peft.Object, _storage.Object, null!, "model"));
    }

    [Fact]
    public void Constructor_NullBaseModelName_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AdapterLearningEngine(_peft.Object, _storage.Object, _blobStorage.Object, null!));
    }

    [Fact]
    public async Task CreateAdapterAsync_EmptyTaskName_ReturnsFailure()
    {
        // Act
        var result = await _sut.CreateAdapterAsync("", AdapterConfig.Default());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task CreateAdapterAsync_WhitespaceTaskName_ReturnsFailure()
    {
        // Act
        var result = await _sut.CreateAdapterAsync("   ", AdapterConfig.Default());

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAdapterAsync_PeftInitFails_ReturnsFailure()
    {
        // Arrange
        _peft.Setup(p => p.InitializeAdapterAsync(It.IsAny<string>(), It.IsAny<AdapterConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[], string>.Failure("Init failed"));

        // Act
        var result = await _sut.CreateAdapterAsync("task1", AdapterConfig.Default());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("initialize");
    }

    [Fact]
    public async Task CreateAdapterAsync_OversizedAdapter_ReturnsFailure()
    {
        // Arrange
        byte[] weights = new byte[100];
        _peft.Setup(p => p.InitializeAdapterAsync(It.IsAny<string>(), It.IsAny<AdapterConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[], string>.Success(weights));
        _peft.Setup(p => p.ValidateAdapterAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<long, string>.Success(11L * 1024 * 1024)); // 11 MB

        // Act
        var result = await _sut.CreateAdapterAsync("task1", AdapterConfig.Default());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("exceeds");
    }

    [Fact]
    public async Task CreateAdapterAsync_Success_ReturnsAdapterId()
    {
        // Arrange
        byte[] weights = new byte[100];
        _peft.Setup(p => p.InitializeAdapterAsync(It.IsAny<string>(), It.IsAny<AdapterConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[], string>.Success(weights));
        _peft.Setup(p => p.ValidateAdapterAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<long, string>.Success(1024L)); // 1 KB
        _blobStorage.Setup(b => b.StoreWeightsAsync(It.IsAny<AdapterId>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("path/to/weights"));
        _storage.Setup(s => s.StoreMetadataAsync(It.IsAny<AdapterMetadata>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        // Act
        var result = await _sut.CreateAdapterAsync("task1", AdapterConfig.Default());

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TrainAdapterAsync_EmptyExamples_ReturnsFailure()
    {
        // Act
        var result = await _sut.TrainAdapterAsync(AdapterId.NewId(), new List<TrainingExample>(), TrainingConfig.Default());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task TrainAdapterAsync_NullExamples_ReturnsFailure()
    {
        // Act
        var result = await _sut.TrainAdapterAsync(AdapterId.NewId(), null!, TrainingConfig.Default());

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateWithAdapterAsync_EmptyPrompt_ReturnsFailure()
    {
        // Act
        var result = await _sut.GenerateWithAdapterAsync("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task GenerateWithAdapterAsync_NoAdapter_CallsWithNullWeights()
    {
        // Arrange
        _peft.Setup(p => p.GenerateAsync(It.IsAny<string>(), null, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("Generated text"));

        // Act
        var result = await _sut.GenerateWithAdapterAsync("prompt text");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Generated text");
    }

    [Fact]
    public async Task MergeAdaptersAsync_LessThanTwoAdapters_ReturnsFailure()
    {
        // Act
        var result = await _sut.MergeAdaptersAsync(new List<AdapterId> { AdapterId.NewId() }, MergeStrategy.Average);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("At least 2");
    }

    [Fact]
    public async Task MergeAdaptersAsync_NullList_ReturnsFailure()
    {
        // Act
        var result = await _sut.MergeAdaptersAsync(null!, MergeStrategy.Average);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
