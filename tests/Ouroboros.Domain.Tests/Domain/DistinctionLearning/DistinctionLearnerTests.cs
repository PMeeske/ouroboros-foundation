using Ouroboros.Core.DistinctionLearning;
using Ouroboros.Core.Monads;
using Ouroboros.Domain.DistinctionLearning;
using Moq;

namespace Ouroboros.Tests.Domain.DistinctionLearning;

[Trait("Category", "Unit")]
public sealed class DistinctionLearnerTests
{
    private readonly Mock<IDistinctionWeightStorage> _storage = new();
    private readonly DistinctionLearner _sut;

    public DistinctionLearnerTests()
    {
        _sut = new DistinctionLearner(_storage.Object);
    }

    [Fact]
    public void Constructor_NullStorage_Throws()
    {
        Action act = () => new DistinctionLearner(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("storage");
    }

    [Fact]
    public async Task UpdateFromDistinctionAsync_Returns_Success_With_NewDistinction()
    {
        DistinctionState state = DistinctionState.Initial();
        Observation obs = Observation.WithCertainPrior("some content about learning");

        Result<DistinctionState, string> result = await _sut.UpdateFromDistinctionAsync(state, obs, "Draft");

        result.IsSuccess.Should().BeTrue();
        result.Value.ActiveDistinctions.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateFromDistinctionAsync_Preserves_Existing_Distinctions()
    {
        DistinctionState state = DistinctionState.Initial()
            .AddDistinction("existing", 0.8);
        Observation obs = Observation.WithCertainPrior("new observation");

        Result<DistinctionState, string> result = await _sut.UpdateFromDistinctionAsync(state, obs, "Draft");

        result.IsSuccess.Should().BeTrue();
        result.Value.ActiveDistinctions.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateFromDistinctionAsync_Updates_EpistemicCertainty()
    {
        DistinctionState state = DistinctionState.Initial().WithCertainty(0.5);
        Observation obs = Observation.WithCertainPrior("test content");

        Result<DistinctionState, string> result = await _sut.UpdateFromDistinctionAsync(state, obs, "Draft");

        result.IsSuccess.Should().BeTrue();
        result.Value.EpistemicCertainty.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public async Task UpdateFromDistinctionAsync_RecognitionStage_BoostsFitness()
    {
        DistinctionState state = DistinctionState.Initial();
        Observation obs = Observation.WithCertainPrior("test content that is long enough to have meaningful fitness");

        Result<DistinctionState, string> resultRecognition = await _sut.UpdateFromDistinctionAsync(state, obs, "Recognition");
        Result<DistinctionState, string> resultDraft = await _sut.UpdateFromDistinctionAsync(state, obs, "Draft");

        resultRecognition.IsSuccess.Should().BeTrue();
        resultDraft.IsSuccess.Should().BeTrue();
        resultRecognition.Value.ActiveDistinctions[0].Fitness
            .Should().BeGreaterThanOrEqualTo(resultDraft.Value.ActiveDistinctions[0].Fitness);
    }

    [Fact]
    public async Task UpdateFromDistinctionAsync_Cancelled_Throws()
    {
        DistinctionState state = DistinctionState.Initial();
        Observation obs = Observation.WithCertainPrior("test");
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Func<Task> act = () => _sut.UpdateFromDistinctionAsync(state, obs, "Draft", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task RecognizeAsync_Boosts_Certainty()
    {
        DistinctionState state = DistinctionState.Initial().WithCertainty(0.5);

        Result<DistinctionState, string> result = await _sut.RecognizeAsync(state, "test circumstance");

        result.IsSuccess.Should().BeTrue();
        result.Value.EpistemicCertainty.Should().BeGreaterThan(0.5);
    }

    [Fact]
    public async Task RecognizeAsync_Increments_CycleCount()
    {
        DistinctionState state = DistinctionState.Initial();

        Result<DistinctionState, string> result = await _sut.RecognizeAsync(state, "test");

        result.IsSuccess.Should().BeTrue();
        result.Value.CycleCount.Should().Be(1);
    }

    [Fact]
    public async Task RecognizeAsync_Certainty_CappedAt_One()
    {
        DistinctionState state = DistinctionState.Initial().WithCertainty(0.99);

        Result<DistinctionState, string> result = await _sut.RecognizeAsync(state, "test");

        result.IsSuccess.Should().BeTrue();
        result.Value.EpistemicCertainty.Should().BeLessThanOrEqualTo(1.0);
    }

    [Fact]
    public async Task RecognizeAsync_Cancelled_Throws()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Func<Task> act = () => _sut.RecognizeAsync(DistinctionState.Initial(), "test", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task DissolveAsync_FitnessThreshold_Dissolves_LowFitness()
    {
        List<DistinctionWeightMetadata> metadata = new()
        {
            new("id1", "/path/1", 0.01, "Draft", DateTime.UtcNow.AddDays(-1), false, 100),
            new("id2", "/path/2", 0.99, "Recognition", DateTime.UtcNow, false, 100),
        };

        _storage.Setup(s => s.ListWeightsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<DistinctionWeightMetadata>, string>.Success(metadata));
        _storage.Setup(s => s.DissolveWeightsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        Result<Unit, string> result = await _sut.DissolveAsync(DistinctionState.Initial(), DissolutionStrategy.FitnessThreshold);

        result.IsSuccess.Should().BeTrue();
        _storage.Verify(s => s.DissolveWeightsAsync("/path/1", It.IsAny<CancellationToken>()), Times.Once);
        _storage.Verify(s => s.DissolveWeightsAsync("/path/2", It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DissolveAsync_All_Dissolves_AllActive()
    {
        List<DistinctionWeightMetadata> metadata = new()
        {
            new("id1", "/path/1", 0.5, "Draft", DateTime.UtcNow, false, 100),
            new("id2", "/path/2", 0.9, "Draft", DateTime.UtcNow, false, 100),
            new("id3", "/path/3", 0.3, "Draft", DateTime.UtcNow, true, 100),
        };

        _storage.Setup(s => s.ListWeightsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<DistinctionWeightMetadata>, string>.Success(metadata));
        _storage.Setup(s => s.DissolveWeightsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        Result<Unit, string> result = await _sut.DissolveAsync(DistinctionState.Initial(), DissolutionStrategy.All);

        result.IsSuccess.Should().BeTrue();
        _storage.Verify(s => s.DissolveWeightsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task DissolveAsync_ListFailure_Returns_Failure()
    {
        _storage.Setup(s => s.ListWeightsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<DistinctionWeightMetadata>, string>.Failure("storage error"));

        Result<Unit, string> result = await _sut.DissolveAsync(DistinctionState.Initial(), DissolutionStrategy.All);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Failed to list weights");
    }

    [Fact]
    public async Task EvaluateDistinctionFitnessAsync_EmptyObservations_Returns_Zero()
    {
        Result<double, string> result = await _sut.EvaluateDistinctionFitnessAsync("test", Array.Empty<Observation>());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0.0);
    }

    [Fact]
    public async Task EvaluateDistinctionFitnessAsync_AllMatch_ReturnsHighFitness()
    {
        Observation[] observations = new[]
        {
            Observation.WithCertainPrior("This contains the test distinction"),
            Observation.WithCertainPrior("Another observation with test inside"),
        };

        Result<double, string> result = await _sut.EvaluateDistinctionFitnessAsync("test", observations);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public async Task EvaluateDistinctionFitnessAsync_NoneMatch_ReturnsZero()
    {
        Observation[] observations = new[]
        {
            Observation.WithCertainPrior("No match here"),
            Observation.WithCertainPrior("Nothing relevant"),
        };

        Result<double, string> result = await _sut.EvaluateDistinctionFitnessAsync("xyz_unique", observations);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0.0);
    }

    [Fact]
    public async Task EvaluateDistinctionFitnessAsync_CaseInsensitive()
    {
        Observation[] observations = new[]
        {
            Observation.WithCertainPrior("CAPITAL TEST content"),
        };

        Result<double, string> result = await _sut.EvaluateDistinctionFitnessAsync("capital test", observations);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public async Task EvaluateDistinctionFitnessAsync_Cancelled_Throws()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Func<Task> act = () => _sut.EvaluateDistinctionFitnessAsync("test", new[] { Observation.WithCertainPrior("x") }, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
