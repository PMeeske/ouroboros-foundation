using Ouroboros.Core.DistinctionLearning;
using Ouroboros.Core.Learning;
using Ouroboros.Core.Monads;
using Ouroboros.Domain.DistinctionLearning;
using Moq;

namespace Ouroboros.Tests.Domain.DistinctionLearning;

[Trait("Category", "Unit")]
public sealed class DistinctionPeftAdapterTests
{
    private readonly Mock<IPeftIntegration> _peft = new();
    private readonly Mock<IDistinctionWeightStorage> _storage = new();
    private readonly DistinctionPeftAdapter _sut;

    public DistinctionPeftAdapterTests()
    {
        _sut = new DistinctionPeftAdapter(_peft.Object, _storage.Object);
    }

    [Fact]
    public void Constructor_NullPeft_Throws()
    {
        Action act = () => new DistinctionPeftAdapter(null!, _storage.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("peft");
    }

    [Fact]
    public void Constructor_NullStorage_Throws()
    {
        Action act = () => new DistinctionPeftAdapter(_peft.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("storage");
    }

    [Fact]
    public void ToTrainingExample_Converts_Correctly()
    {
        ActiveDistinction distinction = new("id1", "test content", 0.85, DateTime.UtcNow, "Recognition");

        TrainingExample example = DistinctionPeftAdapter.ToTrainingExample(distinction);

        example.Input.Should().Contain("test content");
        example.Output.Should().Contain("Recognition");
        example.Output.Should().Contain("0.85");
        example.Weight.Should().Be(0.85);
    }

    [Fact]
    public async Task TrainFromDistinctionsAsync_EmptyList_Returns_Success()
    {
        Result<Unit, string> result = await _sut.TrainFromDistinctionsAsync(new List<ActiveDistinction>(), "model");

        result.IsSuccess.Should().BeTrue();
        _peft.Verify(p => p.InitializeAdapterAsync(It.IsAny<string>(), It.IsAny<AdapterConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task TrainFromDistinctionsAsync_Success_StoresWeights()
    {
        List<ActiveDistinction> distinctions = new()
        {
            new("id1", "content1", 0.8, DateTime.UtcNow, "Draft"),
            new("id2", "content2", 0.9, DateTime.UtcNow, "Recognition"),
        };

        byte[] adapterWeights = new byte[] { 1, 2, 3 };
        byte[] trainedWeights = new byte[] { 4, 5, 6 };

        _peft.Setup(p => p.InitializeAdapterAsync(It.IsAny<string>(), It.IsAny<AdapterConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[], string>.Success(adapterWeights));
        _peft.Setup(p => p.TrainAdapterAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<List<TrainingExample>>(), It.IsAny<TrainingConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[], string>.Success(trainedWeights));
        _storage.Setup(s => s.StoreWeightsAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistinctionWeightMetadata>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("/path"));

        Result<Unit, string> result = await _sut.TrainFromDistinctionsAsync(distinctions, "model");

        result.IsSuccess.Should().BeTrue();
        _storage.Verify(s => s.StoreWeightsAsync(It.IsAny<string>(), trainedWeights, It.IsAny<DistinctionWeightMetadata>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TrainFromDistinctionsAsync_InitFails_Returns_Failure()
    {
        List<ActiveDistinction> distinctions = new()
        {
            new("id1", "content1", 0.8, DateTime.UtcNow, "Draft"),
        };

        _peft.Setup(p => p.InitializeAdapterAsync(It.IsAny<string>(), It.IsAny<AdapterConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[], string>.Failure("init error"));

        Result<Unit, string> result = await _sut.TrainFromDistinctionsAsync(distinctions, "model");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Failed to initialize adapter");
    }

    [Fact]
    public async Task TrainFromDistinctionsAsync_TrainingFails_Returns_Failure()
    {
        List<ActiveDistinction> distinctions = new()
        {
            new("id1", "content1", 0.8, DateTime.UtcNow, "Draft"),
        };

        _peft.Setup(p => p.InitializeAdapterAsync(It.IsAny<string>(), It.IsAny<AdapterConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[], string>.Success(new byte[] { 1 }));
        _peft.Setup(p => p.TrainAdapterAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<List<TrainingExample>>(), It.IsAny<TrainingConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[], string>.Failure("training error"));

        Result<Unit, string> result = await _sut.TrainFromDistinctionsAsync(distinctions, "model");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Failed to train adapter");
    }

    [Fact]
    public async Task TrainFromDistinctionsAsync_StoreFailure_Still_Returns_Success()
    {
        List<ActiveDistinction> distinctions = new()
        {
            new("id1", "content1", 0.8, DateTime.UtcNow, "Draft"),
        };

        _peft.Setup(p => p.InitializeAdapterAsync(It.IsAny<string>(), It.IsAny<AdapterConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[], string>.Success(new byte[] { 1 }));
        _peft.Setup(p => p.TrainAdapterAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<List<TrainingExample>>(), It.IsAny<TrainingConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[], string>.Success(new byte[] { 2 }));
        _storage.Setup(s => s.StoreWeightsAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistinctionWeightMetadata>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Failure("store error"));

        Result<Unit, string> result = await _sut.TrainFromDistinctionsAsync(distinctions, "model");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TrainFromDistinctionsAsync_MetadataFitness_Is_Average()
    {
        List<ActiveDistinction> distinctions = new()
        {
            new("id1", "content1", 0.6, DateTime.UtcNow, "Draft"),
            new("id2", "content2", 0.8, DateTime.UtcNow, "Recognition"),
        };

        DistinctionWeightMetadata? capturedMetadata = null;

        _peft.Setup(p => p.InitializeAdapterAsync(It.IsAny<string>(), It.IsAny<AdapterConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[], string>.Success(new byte[] { 1 }));
        _peft.Setup(p => p.TrainAdapterAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<List<TrainingExample>>(), It.IsAny<TrainingConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[], string>.Success(new byte[] { 2 }));
        _storage.Setup(s => s.StoreWeightsAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistinctionWeightMetadata>(), It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistinctionWeightMetadata, CancellationToken>((_, _, m, _) => capturedMetadata = m)
            .ReturnsAsync(Result<string, string>.Success("/path"));

        await _sut.TrainFromDistinctionsAsync(distinctions, "model");

        capturedMetadata.Should().NotBeNull();
        capturedMetadata!.Fitness.Should().Be(0.7);
    }

    [Fact]
    public async Task TrainFromDistinctionsAsync_MetadataLearnedAtStage_Is_LastDistinction()
    {
        List<ActiveDistinction> distinctions = new()
        {
            new("id1", "content1", 0.6, DateTime.UtcNow, "Draft"),
            new("id2", "content2", 0.8, DateTime.UtcNow, "Recognition"),
        };

        DistinctionWeightMetadata? capturedMetadata = null;

        _peft.Setup(p => p.InitializeAdapterAsync(It.IsAny<string>(), It.IsAny<AdapterConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[], string>.Success(new byte[] { 1 }));
        _peft.Setup(p => p.TrainAdapterAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<List<TrainingExample>>(), It.IsAny<TrainingConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[], string>.Success(new byte[] { 2, 3 }));
        _storage.Setup(s => s.StoreWeightsAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistinctionWeightMetadata>(), It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistinctionWeightMetadata, CancellationToken>((_, _, m, _) => capturedMetadata = m)
            .ReturnsAsync(Result<string, string>.Success("/path"));

        await _sut.TrainFromDistinctionsAsync(distinctions, "model");

        capturedMetadata.Should().NotBeNull();
        capturedMetadata!.LearnedAtStage.Should().Be("Recognition");
        capturedMetadata.SizeBytes.Should().Be(2);
    }
}
