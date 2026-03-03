using Ouroboros.Core.Processing;

namespace Ouroboros.Core.Tests.Processing;

[Trait("Category", "Unit")]
[Trait("Category", "Processing")]
public class RecursiveChunkProcessorTests
{
    [Fact]
    public void Constructor_NullProcessFunc_ThrowsArgumentNullException()
    {
        var act = () => new RecursiveChunkProcessor(
            null!,
            results => Task.FromResult(Result<string>.Success("ok")));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullCombineFunc_ThrowsArgumentNullException()
    {
        var act = () => new RecursiveChunkProcessor(
            chunk => Task.FromResult(Result<string>.Success(chunk)),
            null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ProcessLargeContextAsync_NonStringInput_ReturnsFailure()
    {
        var sut = CreateProcessor();

        var result = await sut.ProcessLargeContextAsync<int, string>(42);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Only string input");
    }

    [Fact]
    public async Task ProcessLargeContextAsync_SmallText_ProcessesSuccessfully()
    {
        var sut = CreateProcessor();

        var result = await sut.ProcessLargeContextAsync<string, string>("Hello world.");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessLargeContextAsync_LargeText_ChunksAndProcesses()
    {
        var chunkCount = 0;
        var sut = new RecursiveChunkProcessor(
            chunk =>
            {
                Interlocked.Increment(ref chunkCount);
                return Task.FromResult(Result<string>.Success($"processed:{chunk.Length}"));
            },
            results => Task.FromResult(Result<string>.Success(string.Join("|", results))));

        // Generate text large enough to require multiple chunks
        string largeText = string.Join(". ", Enumerable.Range(0, 500).Select(i => $"Sentence number {i} with some content"));

        var result = await sut.ProcessLargeContextAsync<string, string>(largeText, maxChunkSize: 256);

        result.IsSuccess.Should().BeTrue();
        chunkCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task ProcessLargeContextAsync_ProcessingFails_ReturnsFailure()
    {
        var sut = new RecursiveChunkProcessor(
            chunk => Task.FromResult(Result<string>.Failure("processing error")),
            results => Task.FromResult(Result<string>.Success("ok")));

        var result = await sut.ProcessLargeContextAsync<string, string>("Some text content.");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessLargeContextAsync_CombineFails_ReturnsFailure()
    {
        var sut = new RecursiveChunkProcessor(
            chunk => Task.FromResult(Result<string>.Success(chunk)),
            results => Task.FromResult(Result<string>.Failure("combine error")));

        var result = await sut.ProcessLargeContextAsync<string, string>("Some text.");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessLargeContextAsync_Cancellation_ReturnsFailure()
    {
        var sut = new RecursiveChunkProcessor(
            async chunk =>
            {
                await Task.Delay(5000);
                return Result<string>.Success(chunk);
            },
            results => Task.FromResult(Result<string>.Success("ok")));

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await sut.ProcessLargeContextAsync<string, string>(
            "Some text.", cancellationToken: cts.Token);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessLargeContextAsync_AdaptiveStrategy_UsesAdaptiveChunking()
    {
        var sut = CreateProcessor();

        var result = await sut.ProcessLargeContextAsync<string, string>(
            "Hello world. This is a test.",
            strategy: ChunkingStrategy.Adaptive);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessLargeContextAsync_FixedStrategy_UsesFixedChunking()
    {
        var sut = CreateProcessor();

        var result = await sut.ProcessLargeContextAsync<string, string>(
            "Hello world. This is a test.",
            strategy: ChunkingStrategy.Fixed);

        result.IsSuccess.Should().BeTrue();
    }

    private static RecursiveChunkProcessor CreateProcessor()
    {
        return new RecursiveChunkProcessor(
            chunk => Task.FromResult(Result<string>.Success(chunk)),
            results => Task.FromResult(Result<string>.Success(string.Join(" ", results))));
    }
}
