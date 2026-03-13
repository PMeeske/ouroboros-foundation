using Ouroboros.Abstractions;
using Ouroboros.Core.DistinctionLearning;
using Ouroboros.Core.Monads;
using Ouroboros.Domain.DistinctionLearning;

namespace Ouroboros.Tests.DistinctionLearning;

[Trait("Category", "Unit")]
public class DistinctionLearnerTests
{
    private readonly Mock<IDistinctionWeightStorage> _mockStorage = new();

    private DistinctionLearner CreateLearner() => new(_mockStorage.Object);

    [Fact]
    public void Constructor_WithNullStorage_ShouldThrow()
    {
        var act = () => new DistinctionLearner(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateFromDistinctionAsync_ShouldReturnUpdatedState()
    {
        var learner = CreateLearner();
        var state = DistinctionState.Initial();
        var observation = new Observation("test content", DateTime.UtcNow, 0.8, new Dictionary<string, object> { ["source"] = "test" });

        var result = await learner.UpdateFromDistinctionAsync(state, observation, "Distinction");

        result.IsSuccess.Should().BeTrue();
        result.Value.ActiveDistinctions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RecognizeAsync_ShouldBoostCertainty()
    {
        var learner = CreateLearner();
        var state = DistinctionState.Initial();

        var result = await learner.RecognizeAsync(state, "test circumstance");

        result.IsSuccess.Should().BeTrue();
        result.Value.EpistemicCertainty.Should().BeGreaterThanOrEqualTo(state.EpistemicCertainty);
    }

    [Fact]
    public async Task DissolveAsync_WhenStorageReturnsEmpty_ShouldSucceed()
    {
        _mockStorage.Setup(s => s.ListWeightsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<DistinctionWeightMetadata>, string>.Success(new List<DistinctionWeightMetadata>()));

        var result = await CreateLearner().DissolveAsync(DistinctionState.Initial(), DissolutionStrategy.FitnessThreshold);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateDistinctionFitnessAsync_EmptyObservations_ShouldReturnZero()
    {
        var result = await CreateLearner().EvaluateDistinctionFitnessAsync("test", Enumerable.Empty<Observation>());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0.0);
    }

    [Fact]
    public async Task EvaluateDistinctionFitnessAsync_WithMatches_ShouldReturnPositiveFitness()
    {
        var observations = new[]
        {
            new Observation("this contains test data", DateTime.UtcNow, 0.9, new Dictionary<string, object> { ["source"] = "test" }),
            new Observation("no match here", DateTime.UtcNow, 0.8, new Dictionary<string, object> { ["source"] = "test" }),
        };

        var result = await CreateLearner().EvaluateDistinctionFitnessAsync("test", observations);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0);
    }
}
