namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

using Ouroboros.Abstractions.Monads;
using Ouroboros.Agent.MetaAI;

[Trait("Category", "Unit")]
public class MemoryStoreExtensionsTests
{
    private readonly Mock<IMemoryStore> _mockStore = new();

    [Fact]
    public async Task RetrieveRelevantExperiencesAsync_NullStore_ThrowsArgumentNullException()
    {
        IMemoryStore store = null!;
        var query = new MemoryQuery("test");

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => store.RetrieveRelevantExperiencesAsync(query));
    }

    [Fact]
    public async Task RetrieveRelevantExperiencesAsync_NullQuery_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _mockStore.Object.RetrieveRelevantExperiencesAsync(null!));
    }

    [Fact]
    public async Task RetrieveRelevantExperiencesAsync_Success_ReturnsList()
    {
        var experiences = new List<Experience>
        {
            new("ctx", "action", "outcome", true, DateTime.UtcNow)
        };

        _mockStore.Setup(s => s.QueryExperiencesAsync(It.IsAny<MemoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<Experience>, string>.Success(experiences));

        var result = await _mockStore.Object.RetrieveRelevantExperiencesAsync(new MemoryQuery("test"));
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task RetrieveRelevantExperiencesAsync_Failure_ReturnsEmptyList()
    {
        _mockStore.Setup(s => s.QueryExperiencesAsync(It.IsAny<MemoryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<Experience>, string>.Failure("error"));

        var result = await _mockStore.Object.RetrieveRelevantExperiencesAsync(new MemoryQuery("test"));
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStatsAsync_NullStore_ThrowsArgumentNullException()
    {
        IMemoryStore store = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() => store.GetStatsAsync());
    }

    [Fact]
    public async Task GetStatsAsync_Success_ReturnsStatistics()
    {
        var stats = new MemoryStatistics(10, 8, 2, 3, 5);
        _mockStore.Setup(s => s.GetStatisticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<MemoryStatistics, string>.Success(stats));

        var result = await _mockStore.Object.GetStatsAsync();
        result.TotalExperiences.Should().Be(10);
    }

    [Fact]
    public async Task GetStatsAsync_Failure_ReturnsDefaultStatistics()
    {
        _mockStore.Setup(s => s.GetStatisticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<MemoryStatistics, string>.Failure("error"));

        var result = await _mockStore.Object.GetStatsAsync();
        result.TotalExperiences.Should().Be(0);
    }
}
