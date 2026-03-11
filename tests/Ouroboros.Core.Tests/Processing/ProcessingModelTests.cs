using Ouroboros.Core.Processing;

namespace Ouroboros.Core.Tests.Processing;

[Trait("Category", "Unit")]
public class ChunkMetadataTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var metadata = new ChunkMetadata(2, 5, 150, ChunkingStrategy.Fixed);

        metadata.Index.Should().Be(2);
        metadata.TotalChunks.Should().Be(5);
        metadata.TokenCount.Should().Be(150);
        metadata.Strategy.Should().Be(ChunkingStrategy.Fixed);
    }

    [Fact]
    public void RecordEquality_Works()
    {
        var a = new ChunkMetadata(0, 3, 100, ChunkingStrategy.Adaptive);
        var b = new ChunkMetadata(0, 3, 100, ChunkingStrategy.Adaptive);
        a.Should().Be(b);
    }
}

[Trait("Category", "Unit")]
public class ChunkResultTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var metadata = new ChunkMetadata(0, 1, 50, ChunkingStrategy.Fixed);
        var result = new ChunkResult<string>("output text", metadata, TimeSpan.FromMilliseconds(100), true);

        result.Output.Should().Be("output text");
        result.Metadata.Should().Be(metadata);
        result.ProcessingTime.Should().Be(TimeSpan.FromMilliseconds(100));
        result.Success.Should().BeTrue();
    }

    [Fact]
    public void Construction_FailedResult()
    {
        var metadata = new ChunkMetadata(1, 3, 200, ChunkingStrategy.Adaptive);
        var result = new ChunkResult<int>(0, metadata, TimeSpan.FromSeconds(5), false);

        result.Success.Should().BeFalse();
        result.Output.Should().Be(0);
    }
}

[Trait("Category", "Unit")]
public class ChunkingStrategyEnumTests
{
    [Theory]
    [InlineData(ChunkingStrategy.Fixed)]
    [InlineData(ChunkingStrategy.Adaptive)]
    public void AllValues_AreDefined(ChunkingStrategy strategy)
    {
        Enum.IsDefined(typeof(ChunkingStrategy), strategy).Should().BeTrue();
    }
}
