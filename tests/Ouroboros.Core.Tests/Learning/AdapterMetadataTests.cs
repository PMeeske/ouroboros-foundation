using Ouroboros.Core.Learning;

namespace Ouroboros.Tests.Learning;

[Trait("Category", "Unit")]
public sealed class AdapterMetadataTests
{
    [Fact]
    public void Create_SetsInitialProperties()
    {
        var id = AdapterId.NewId();
        var config = AdapterConfig.Default();

        var sut = AdapterMetadata.Create(id, "classification", config, "/blobs/adapter1");

        sut.Id.Should().Be(id);
        sut.TaskName.Should().Be("classification");
        sut.Config.Should().Be(config);
        sut.BlobStoragePath.Should().Be("/blobs/adapter1");
        sut.TrainingExampleCount.Should().Be(0);
        sut.PerformanceScore.Should().BeNull();
    }

    [Fact]
    public void Create_SetsTimestamps()
    {
        var before = DateTime.UtcNow;
        var sut = AdapterMetadata.Create(AdapterId.NewId(), "task", AdapterConfig.Default(), "/path");
        var after = DateTime.UtcNow;

        sut.CreatedAt.Should().BeOnOrAfter(before);
        sut.CreatedAt.Should().BeOnOrBefore(after);
        sut.LastTrainedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void WithTraining_AccumulatesExampleCount()
    {
        var sut = AdapterMetadata.Create(AdapterId.NewId(), "task", AdapterConfig.Default(), "/path");

        var updated = sut.WithTraining(50);

        updated.TrainingExampleCount.Should().Be(50);
    }

    [Fact]
    public void WithTraining_MultipleSessions_AccumulatesCount()
    {
        var sut = AdapterMetadata.Create(AdapterId.NewId(), "task", AdapterConfig.Default(), "/path");

        var updated = sut.WithTraining(50).WithTraining(30);

        updated.TrainingExampleCount.Should().Be(80);
    }

    [Fact]
    public void WithTraining_SetsPerformanceScore()
    {
        var sut = AdapterMetadata.Create(AdapterId.NewId(), "task", AdapterConfig.Default(), "/path");

        var updated = sut.WithTraining(100, performanceScore: 0.95);

        updated.PerformanceScore.Should().Be(0.95);
    }

    [Fact]
    public void WithTraining_NoPerformanceScore_KeepsExisting()
    {
        var sut = AdapterMetadata.Create(AdapterId.NewId(), "task", AdapterConfig.Default(), "/path")
            .WithTraining(50, performanceScore: 0.8);

        var updated = sut.WithTraining(50);

        updated.PerformanceScore.Should().Be(0.8);
    }

    [Fact]
    public void WithTraining_UpdatesLastTrainedAt()
    {
        var sut = AdapterMetadata.Create(AdapterId.NewId(), "task", AdapterConfig.Default(), "/path");
        var originalTime = sut.LastTrainedAt;

        var updated = sut.WithTraining(10);

        updated.LastTrainedAt.Should().BeOnOrAfter(originalTime);
    }

    [Fact]
    public void WithTraining_PreservesImmutableFields()
    {
        var id = AdapterId.NewId();
        var sut = AdapterMetadata.Create(id, "task", AdapterConfig.Default(), "/path");

        var updated = sut.WithTraining(10);

        updated.Id.Should().Be(id);
        updated.TaskName.Should().Be("task");
        updated.BlobStoragePath.Should().Be("/path");
    }
}
