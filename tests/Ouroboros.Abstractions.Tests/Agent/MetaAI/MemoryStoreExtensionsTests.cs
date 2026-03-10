using Ouroboros.Abstractions;
using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class MemoryStoreExtensionsTests
{
    private static Experience CreateSampleExperience(string id = "exp-1") =>
        new Experience(
            Id: id,
            Action: "test-action",
            Context: "test-context",
            Outcome: "test-outcome",
            Reward: 0.8,
            Timestamp: DateTime.UtcNow,
            Tags: new List<string> { "tag1" },
            Metadata: new Dictionary<string, object>());

    private static MemoryQuery CreateSampleQuery() =>
        new MemoryQuery(
            Tags: null,
            ContextSimilarity: "test",
            MaxResults: 10);

    [Fact]
    public async Task RetrieveRelevantExperiencesAsync_WhenQuerySucceeds_ReturnsExperiencesList()
    {
        // Arrange
        var experiences = new List<Experience> { CreateSampleExperience() };
        var mockStore = new Mock<IMemoryStore>();
        mockStore
            .Setup(s => s.QueryExperiencesAsync(It.IsAny<MemoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<Experience>, string>.Success(experiences));

        // Act
        var result = await mockStore.Object.RetrieveRelevantExperiencesAsync(CreateSampleQuery());

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("exp-1");
    }

    [Fact]
    public async Task RetrieveRelevantExperiencesAsync_WhenQueryFails_ReturnsEmptyList()
    {
        // Arrange
        var mockStore = new Mock<IMemoryStore>();
        mockStore
            .Setup(s => s.QueryExperiencesAsync(It.IsAny<MemoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<Experience>, string>.Failure("error"));

        // Act
        var result = await mockStore.Object.RetrieveRelevantExperiencesAsync(CreateSampleQuery());

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task RetrieveRelevantExperiencesAsync_NullStore_ThrowsArgumentNullException()
    {
        // Arrange
        IMemoryStore? store = null;

        // Act
        var act = () => store!.RetrieveRelevantExperiencesAsync(CreateSampleQuery());

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RetrieveRelevantExperiencesAsync_NullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        var mockStore = new Mock<IMemoryStore>();

        // Act
        var act = () => mockStore.Object.RetrieveRelevantExperiencesAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetStatsAsync_WhenSucceeds_ReturnsStatistics()
    {
        // Arrange
        var stats = new MemoryStatistics(100, 50, 25, 10, 5);
        var mockStore = new Mock<IMemoryStore>();
        mockStore
            .Setup(s => s.GetStatisticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<MemoryStatistics, string>.Success(stats));

        // Act
        var result = await mockStore.Object.GetStatsAsync();

        // Assert
        result.Should().Be(stats);
    }

    [Fact]
    public async Task GetStatsAsync_WhenFails_ReturnsDefaultStatistics()
    {
        // Arrange
        var mockStore = new Mock<IMemoryStore>();
        mockStore
            .Setup(s => s.GetStatisticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<MemoryStatistics, string>.Failure("error"));

        // Act
        var result = await mockStore.Object.GetStatsAsync();

        // Assert
        result.Should().Be(new MemoryStatistics(0, 0, 0, 0, 0));
    }

    [Fact]
    public async Task GetStatsAsync_NullStore_ThrowsArgumentNullException()
    {
        // Arrange
        IMemoryStore? store = null;

        // Act
        var act = () => store!.GetStatsAsync();

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RetrieveRelevantExperiencesAsync_WithCancellationToken_PassesTokenToStore()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var mockStore = new Mock<IMemoryStore>();
        mockStore
            .Setup(s => s.QueryExperiencesAsync(It.IsAny<MemoryQuery>(), cts.Token))
            .ReturnsAsync(Result<IReadOnlyList<Experience>, string>.Success(
                new List<Experience>()));

        // Act
        await mockStore.Object.RetrieveRelevantExperiencesAsync(CreateSampleQuery(), cts.Token);

        // Assert
        mockStore.Verify(s => s.QueryExperiencesAsync(It.IsAny<MemoryQuery>(), cts.Token), Times.Once);
    }

    [Fact]
    public async Task RetrieveRelevantExperiencesAsync_WithMultipleExperiences_ReturnsAll()
    {
        // Arrange
        var experiences = new List<Experience>
        {
            CreateSampleExperience("exp-1"),
            CreateSampleExperience("exp-2"),
            CreateSampleExperience("exp-3")
        };
        var mockStore = new Mock<IMemoryStore>();
        mockStore
            .Setup(s => s.QueryExperiencesAsync(It.IsAny<MemoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<Experience>, string>.Success(experiences));

        // Act
        var result = await mockStore.Object.RetrieveRelevantExperiencesAsync(CreateSampleQuery());

        // Assert
        result.Should().HaveCount(3);
    }
}
