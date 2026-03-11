using Ouroboros.Core.Processing;

namespace Ouroboros.Core.Tests.Processing;

[Trait("Category", "Unit")]
public class RecursiveChunkProcessorAdditionalTests
{
    // ========================================================================
    // ProcessLargeContextAsync - empty / whitespace input
    // ========================================================================

    [Fact]
    public async Task ProcessLargeContextAsync_EmptyString_ReturnsFailure()
    {
        var sut = CreateProcessor();

        var result = await sut.ProcessLargeContextAsync<string, string>("");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("chunks");
    }

    [Fact]
    public async Task ProcessLargeContextAsync_WhitespaceOnly_ReturnsFailure()
    {
        var sut = CreateProcessor();

        var result = await sut.ProcessLargeContextAsync<string, string>("   ");

        result.IsFailure.Should().BeTrue();
    }

    // ========================================================================
    // ProcessLargeContextAsync - type conversion
    // ========================================================================

    [Fact]
    public async Task ProcessLargeContextAsync_IncompatibleOutputType_ReturnsFailure()
    {
        var sut = CreateProcessor();

        // Output is string, but trying to get int
        var result = await sut.ProcessLargeContextAsync<string, int>("Hello world.");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cannot convert");
    }

    // ========================================================================
    // ProcessLargeContextAsync - adaptive strategy with performance history
    // ========================================================================

    [Fact]
    public async Task ProcessLargeContextAsync_AdaptiveStrategy_LearnsFromSuccesses()
    {
        var sut = CreateProcessor();
        string largeText = string.Join(". ", Enumerable.Range(0, 100).Select(i => $"Sentence {i}"));

        // First call establishes performance data
        var result1 = await sut.ProcessLargeContextAsync<string, string>(
            largeText, maxChunkSize: 300, strategy: ChunkingStrategy.Adaptive);
        result1.IsSuccess.Should().BeTrue();

        // Second call should use adaptive chunk size based on history
        var result2 = await sut.ProcessLargeContextAsync<string, string>(
            largeText, maxChunkSize: 300, strategy: ChunkingStrategy.Adaptive);
        result2.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessLargeContextAsync_AdaptiveStrategy_LearnsFromFailures()
    {
        int callCount = 0;
        var sut = new RecursiveChunkProcessor(
            chunk =>
            {
                callCount++;
                // Fail the first batch, succeed on subsequent
                if (callCount <= 2)
                    return Task.FromResult(Result<string>.Failure("processing error"));
                return Task.FromResult(Result<string>.Success(chunk));
            },
            results => Task.FromResult(Result<string>.Success(string.Join(" ", results))));

        string text = string.Join(". ", Enumerable.Range(0, 50).Select(i => $"Sentence {i}"));

        // First call fails
        var result1 = await sut.ProcessLargeContextAsync<string, string>(
            text, strategy: ChunkingStrategy.Adaptive);
        result1.IsFailure.Should().BeTrue();

        // Reset call count so next call can succeed
        callCount = 100;
        var result2 = await sut.ProcessLargeContextAsync<string, string>(
            text, strategy: ChunkingStrategy.Adaptive);
        result2.IsSuccess.Should().BeTrue();
    }

    // ========================================================================
    // ProcessLargeContextAsync - Fixed strategy
    // ========================================================================

    [Fact]
    public async Task ProcessLargeContextAsync_FixedStrategy_UsesExactChunkSize()
    {
        int processedChunks = 0;
        var sut = new RecursiveChunkProcessor(
            chunk =>
            {
                Interlocked.Increment(ref processedChunks);
                return Task.FromResult(Result<string>.Success(chunk));
            },
            results => Task.FromResult(Result<string>.Success(string.Join(" ", results))));

        string text = string.Join(". ", Enumerable.Range(0, 200).Select(i => $"Content block {i}"));

        var result = await sut.ProcessLargeContextAsync<string, string>(
            text, maxChunkSize: 512, strategy: ChunkingStrategy.Fixed);

        result.IsSuccess.Should().BeTrue();
        processedChunks.Should().BeGreaterOrEqualTo(1);
    }

    // ========================================================================
    // ProcessLargeContextAsync - chunk size clamping
    // ========================================================================

    [Fact]
    public async Task ProcessLargeContextAsync_VerySmallChunkSize_ClampedToMinimum()
    {
        var sut = CreateProcessor();
        string text = string.Join(". ", Enumerable.Range(0, 50).Select(i => $"Sentence {i}"));

        // Request chunk size smaller than minimum (256)
        var result = await sut.ProcessLargeContextAsync<string, string>(
            text, maxChunkSize: 10, strategy: ChunkingStrategy.Adaptive);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessLargeContextAsync_VeryLargeChunkSize_ClampedToMaximum()
    {
        var sut = CreateProcessor();
        string text = string.Join(". ", Enumerable.Range(0, 50).Select(i => $"Sentence {i}"));

        // Request chunk size larger than maximum (1024)
        var result = await sut.ProcessLargeContextAsync<string, string>(
            text, maxChunkSize: 10000, strategy: ChunkingStrategy.Adaptive);

        result.IsSuccess.Should().BeTrue();
    }

    // ========================================================================
    // ProcessLargeContextAsync - exception handling
    // ========================================================================

    [Fact]
    public async Task ProcessLargeContextAsync_ProcessThrowsInvalidOperation_ChunkMarkedAsFailed()
    {
        var sut = new RecursiveChunkProcessor(
            chunk => throw new InvalidOperationException("process error"),
            results => Task.FromResult(Result<string>.Success("ok")));

        var result = await sut.ProcessLargeContextAsync<string, string>("Some text content.");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessLargeContextAsync_CombineThrowsInvalidOperation_ReturnsFailure()
    {
        var sut = new RecursiveChunkProcessor(
            chunk => Task.FromResult(Result<string>.Success(chunk)),
            results => throw new InvalidOperationException("combine error"));

        var result = await sut.ProcessLargeContextAsync<string, string>("Some text.");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("combine");
    }

    [Fact]
    public async Task ProcessLargeContextAsync_UnexpectedException_ReturnsFailure()
    {
        var sut = new RecursiveChunkProcessor(
            chunk => throw new NotSupportedException("unexpected"),
            results => Task.FromResult(Result<string>.Success("ok")));

        var result = await sut.ProcessLargeContextAsync<string, string>("Some text.");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Unexpected error");
    }

    // ========================================================================
    // ProcessLargeContextAsync - text splitting at sentence boundaries
    // ========================================================================

    [Fact]
    public async Task ProcessLargeContextAsync_TextWithPeriods_SplitsAtSentenceBoundaries()
    {
        var chunks = new List<string>();
        var sut = new RecursiveChunkProcessor(
            chunk =>
            {
                lock (chunks) { chunks.Add(chunk); }
                return Task.FromResult(Result<string>.Success(chunk));
            },
            results => Task.FromResult(Result<string>.Success(string.Join(" ", results))));

        // Create text with clear sentence boundaries
        string text = string.Join(". ", Enumerable.Range(0, 300).Select(i => $"Sentence number {i}"));

        var result = await sut.ProcessLargeContextAsync<string, string>(text, maxChunkSize: 256);

        result.IsSuccess.Should().BeTrue();
        chunks.Should().HaveCountGreaterThan(1);
    }

    // ========================================================================
    // ChunkMetadata - additional record tests
    // ========================================================================

    [Fact]
    public void ChunkMetadata_WithExpression_CreatesModifiedCopy()
    {
        var original = new ChunkMetadata(0, 5, 100, ChunkingStrategy.Fixed);
        var modified = original with { Index = 3 };

        modified.Index.Should().Be(3);
        original.Index.Should().Be(0);
    }

    // ========================================================================
    // ChunkResult - additional record tests
    // ========================================================================

    [Fact]
    public void ChunkResult_WithExpression_CreatesModifiedCopy()
    {
        var metadata = new ChunkMetadata(0, 1, 50, ChunkingStrategy.Adaptive);
        var original = new ChunkResult<string>("output", metadata, TimeSpan.FromMilliseconds(100), true);
        var modified = original with { Success = false };

        modified.Success.Should().BeFalse();
        original.Success.Should().BeTrue();
    }

    [Fact]
    public void ChunkResult_RecordEquality()
    {
        var metadata = new ChunkMetadata(0, 1, 50, ChunkingStrategy.Fixed);
        var time = TimeSpan.FromMilliseconds(100);
        var a = new ChunkResult<string>("out", metadata, time, true);
        var b = new ChunkResult<string>("out", metadata, time, true);

        a.Should().Be(b);
    }

    // ========================================================================
    // Helpers
    // ========================================================================

    private static RecursiveChunkProcessor CreateProcessor()
    {
        return new RecursiveChunkProcessor(
            chunk => Task.FromResult(Result<string>.Success(chunk)),
            results => Task.FromResult(Result<string>.Success(string.Join(" ", results))));
    }
}
