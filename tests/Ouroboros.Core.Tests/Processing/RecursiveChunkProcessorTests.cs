using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.Processing;

namespace Ouroboros.Core.Tests.Processing;

[Trait("Category", "Unit")]
public class RecursiveChunkProcessorTests
{
    [Fact]
    public void Constructor_NullProcessFunc_ThrowsArgumentNullException()
    {
        Action act = () => new RecursiveChunkProcessor(
            null!,
            results => Task.FromResult(Result<string>.Success(string.Join("", results))));

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullCombineFunc_ThrowsArgumentNullException()
    {
        Action act = () => new RecursiveChunkProcessor(
            chunk => Task.FromResult(Result<string>.Success(chunk)),
            null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ProcessLargeContextAsync_NonStringInput_ReturnsFailure()
    {
        var processor = CreateProcessor();

        var result = await processor.ProcessLargeContextAsync<int, string>(42);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("string input");
    }

    [Fact]
    public async Task ProcessLargeContextAsync_EmptyString_ReturnsFailure()
    {
        var processor = CreateProcessor();

        var result = await processor.ProcessLargeContextAsync<string, string>("");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessLargeContextAsync_WhitespaceString_ReturnsFailure()
    {
        var processor = CreateProcessor();

        var result = await processor.ProcessLargeContextAsync<string, string>("   ");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessLargeContextAsync_SmallInput_ProcessesSuccessfully()
    {
        var processor = CreateProcessor();
        var input = "This is a small test input.";

        var result = await processor.ProcessLargeContextAsync<string, string>(input);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ProcessLargeContextAsync_FixedStrategy_UsesFixedSize()
    {
        var processedChunks = new List<string>();
        var processor = new RecursiveChunkProcessor(
            chunk =>
            {
                processedChunks.Add(chunk);
                return Task.FromResult(Result<string>.Success(chunk));
            },
            results => Task.FromResult(Result<string>.Success(string.Join(" ", results))));

        var input = new string('x', 5000);

        var result = await processor.ProcessLargeContextAsync<string, string>(
            input, maxChunkSize: 512, strategy: ChunkingStrategy.Fixed);

        result.IsSuccess.Should().BeTrue();
        processedChunks.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task ProcessLargeContextAsync_AdaptiveStrategy_Works()
    {
        var processor = CreateProcessor();
        var input = new string('y', 5000);

        var result = await processor.ProcessLargeContextAsync<string, string>(
            input, maxChunkSize: 512, strategy: ChunkingStrategy.Adaptive);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessLargeContextAsync_ChunkProcessingFails_ReturnsFailure()
    {
        var processor = new RecursiveChunkProcessor(
            _ => Task.FromResult(Result<string>.Failure("processing error")),
            results => Task.FromResult(Result<string>.Success(string.Join("", results))));

        var input = "Some text to process.";

        var result = await processor.ProcessLargeContextAsync<string, string>(input);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessLargeContextAsync_CombineFails_ReturnsFailure()
    {
        var processor = new RecursiveChunkProcessor(
            chunk => Task.FromResult(Result<string>.Success(chunk)),
            _ => Task.FromResult(Result<string>.Failure("combine error")));

        var input = "Some text to process that is long enough.";

        var result = await processor.ProcessLargeContextAsync<string, string>(input);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("combine");
    }

    [Fact]
    public async Task ProcessLargeContextAsync_Cancellation_ReturnsFailure()
    {
        var processor = new RecursiveChunkProcessor(
            async chunk =>
            {
                await Task.Delay(1000);
                return Result<string>.Success(chunk);
            },
            results => Task.FromResult(Result<string>.Success(string.Join("", results))));

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await processor.ProcessLargeContextAsync<string, string>(
            "test input", cancellationToken: cts.Token);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessLargeContextAsync_TypeMismatch_ReturnsFailure()
    {
        var processor = CreateProcessor();

        var result = await processor.ProcessLargeContextAsync<string, int>("test");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cannot convert");
    }

    [Fact]
    public async Task ProcessLargeContextAsync_LargeInput_SplitsIntoMultipleChunks()
    {
        var chunkCount = 0;
        var processor = new RecursiveChunkProcessor(
            chunk =>
            {
                Interlocked.Increment(ref chunkCount);
                return Task.FromResult(Result<string>.Success(chunk));
            },
            results => Task.FromResult(Result<string>.Success(string.Join("", results))));

        var input = string.Join(". ", Enumerable.Range(0, 100).Select(i => $"Sentence number {i}"));

        var result = await processor.ProcessLargeContextAsync<string, string>(
            input, maxChunkSize: 256);

        result.IsSuccess.Should().BeTrue();
        chunkCount.Should().BeGreaterThan(1);
    }

    private static RecursiveChunkProcessor CreateProcessor()
    {
        return new RecursiveChunkProcessor(
            chunk => Task.FromResult(Result<string>.Success(chunk)),
            results => Task.FromResult(Result<string>.Success(string.Join(" ", results))));
    }
}
