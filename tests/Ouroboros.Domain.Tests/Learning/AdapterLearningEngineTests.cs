using Ouroboros.Abstractions;
using Ouroboros.Core.Learning;
using Ouroboros.Core.Monads;
using Ouroboros.Domain.Learning;

namespace Ouroboros.Tests.Learning;

[Trait("Category", "Unit")]
public class AdapterLearningEngineTests
{
    private readonly Mock<IPeftIntegration> _mockPeft = new();
    private readonly Mock<IAdapterStorage> _mockStorage = new();
    private readonly Mock<IAdapterBlobStorage> _mockBlobStorage = new();
    private const string BaseModel = "test-model";

    private AdapterLearningEngine CreateEngine() =>
        new(_mockPeft.Object, _mockStorage.Object, _mockBlobStorage.Object, BaseModel);

    [Fact]
    public async Task CreateAdapterAsync_WithValidInput_ShouldReturnSuccess()
    {
        var config = AdapterConfig.Default();
        _mockPeft.Setup(p => p.InitializeAdapterAsync(BaseModel, config, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[], string>.Success(new byte[1024]));
        _mockPeft.Setup(p => p.ValidateAdapterAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<long, string>.Success(1024));
        _mockBlobStorage.Setup(b => b.StoreWeightsAsync(It.IsAny<AdapterId>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("/path/to/blob"));
        _mockStorage.Setup(s => s.StoreMetadataAsync(It.IsAny<AdapterMetadata>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        var result = await CreateEngine().CreateAdapterAsync("task1", config);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAdapterAsync_WithEmptyTaskName_ShouldReturnFailure()
    {
        var result = await CreateEngine().CreateAdapterAsync("", AdapterConfig.Default());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAdapterAsync_WhenPeftInitFails_ShouldReturnFailure()
    {
        var config = AdapterConfig.Default();
        _mockPeft.Setup(p => p.InitializeAdapterAsync(BaseModel, config, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[], string>.Failure("init failed"));

        var result = await CreateEngine().CreateAdapterAsync("task1", config);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("init");
    }

    [Fact]
    public async Task GenerateWithAdapterAsync_WithEmptyPrompt_ShouldReturnFailure()
    {
        var result = await CreateEngine().GenerateWithAdapterAsync("");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateWithAdapterAsync_WithNoAdapter_ShouldCallPeftWithNullWeights()
    {
        _mockPeft.Setup(p => p.GenerateAsync(BaseModel, null, "test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("response"));

        var result = await CreateEngine().GenerateWithAdapterAsync("test");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("response");
    }

    [Fact]
    public async Task TrainAdapterAsync_WithEmptyExamples_ShouldReturnFailure()
    {
        var result = await CreateEngine().TrainAdapterAsync(AdapterId.NewId(), new List<TrainingExample>(), TrainingConfig.Default());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task MergeAdaptersAsync_WithLessThanTwoAdapters_ShouldReturnFailure()
    {
        var result = await CreateEngine().MergeAdaptersAsync(new List<AdapterId> { AdapterId.NewId() }, MergeStrategy.Average);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithNullPeft_ShouldThrow()
    {
        var act = () => new AdapterLearningEngine(null!, _mockStorage.Object, _mockBlobStorage.Object, BaseModel);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullStorage_ShouldThrow()
    {
        var act = () => new AdapterLearningEngine(_mockPeft.Object, null!, _mockBlobStorage.Object, BaseModel);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullBlobStorage_ShouldThrow()
    {
        var act = () => new AdapterLearningEngine(_mockPeft.Object, _mockStorage.Object, null!, BaseModel);
        act.Should().Throw<ArgumentNullException>();
    }
}
