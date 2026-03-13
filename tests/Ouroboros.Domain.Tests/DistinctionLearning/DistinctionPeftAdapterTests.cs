using Ouroboros.Abstractions;
using Ouroboros.Core.DistinctionLearning;
using Ouroboros.Core.Learning;
using Ouroboros.Core.Monads;
using Ouroboros.Domain.DistinctionLearning;

namespace Ouroboros.Tests.DistinctionLearning;

[Trait("Category", "Unit")]
public class DistinctionPeftAdapterTests
{
    private readonly Mock<IPeftIntegration> _mockPeft = new();
    private readonly Mock<IDistinctionWeightStorage> _mockStorage = new();

    [Fact]
    public void Constructor_WithNullPeft_ShouldThrow()
    {
        var act = () => new DistinctionPeftAdapter(null!, _mockStorage.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullStorage_ShouldThrow()
    {
        var act = () => new DistinctionPeftAdapter(_mockPeft.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToTrainingExample_ShouldConvertDistinction()
    {
        var distinction = new ActiveDistinction("id1", "content text", 0.85, DateTime.UtcNow, "Distinction");

        var example = DistinctionPeftAdapter.ToTrainingExample(distinction);

        example.Input.Should().Contain("content text");
        example.Weight.Should().Be(0.85);
    }

    [Fact]
    public async Task TrainFromDistinctionsAsync_EmptyList_ShouldReturnSuccess()
    {
        var adapter = new DistinctionPeftAdapter(_mockPeft.Object, _mockStorage.Object);

        var result = await adapter.TrainFromDistinctionsAsync(new List<ActiveDistinction>(), "model");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TrainFromDistinctionsAsync_WhenPeftInitFails_ShouldReturnFailure()
    {
        _mockPeft.Setup(p => p.InitializeAdapterAsync(It.IsAny<string>(), It.IsAny<AdapterConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<byte[], string>.Failure("init failed"));

        var adapter = new DistinctionPeftAdapter(_mockPeft.Object, _mockStorage.Object);
        var distinctions = new List<ActiveDistinction>
        {
            new("id1", "content", 0.8, DateTime.UtcNow, "Distinction"),
        };

        var result = await adapter.TrainFromDistinctionsAsync(distinctions, "model");

        result.IsFailure.Should().BeTrue();
    }
}
