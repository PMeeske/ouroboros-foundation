using System.Reactive.Linq;
using System.Reactive.Subjects;
using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.EmbodiedInteraction;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class AudioSensorTests : IDisposable
{
    private readonly Mock<ISttModel> _mockSttModel;
    private readonly VirtualSelf _virtualSelf;
    private readonly AudioSensor _sut;

    public AudioSensorTests()
    {
        _mockSttModel = new Mock<ISttModel>();
        _mockSttModel.Setup(m => m.ModelName).Returns("test-stt");
        _mockSttModel.Setup(m => m.SupportsStreaming).Returns(false);

        _virtualSelf = new VirtualSelf("TestSelf", fusionWindowMs: 60000);
        _sut = new AudioSensor(_mockSttModel.Object, _virtualSelf);
    }

    public void Dispose()
    {
        _sut.Dispose();
        _virtualSelf.Dispose();
    }

    [Fact]
    public void Constructor_NullSttModel_ThrowsArgumentNullException()
    {
        Action act = () => new AudioSensor(null!, _virtualSelf);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullVirtualSelf_ThrowsArgumentNullException()
    {
        Action act = () => new AudioSensor(_mockSttModel.Object, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ModelName_ReturnsModelName()
    {
        _sut.ModelName.Should().Be("test-stt");
    }

    [Fact]
    public void IsListening_Initially_IsFalse()
    {
        _sut.IsListening.Should().BeFalse();
    }

    [Fact]
    public async Task StartListeningAsync_WhenNotListening_ReturnsSuccess()
    {
        var result = await _sut.StartListeningAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.IsListening.Should().BeTrue();
    }

    [Fact]
    public async Task StartListeningAsync_WhenAlreadyListening_ReturnsFailure()
    {
        await _sut.StartListeningAsync();

        var result = await _sut.StartListeningAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Already listening");
    }

    [Fact]
    public async Task StartListeningAsync_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.StartListeningAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public async Task StopListeningAsync_WhenNotListening_ReturnsSuccess()
    {
        var result = await _sut.StopListeningAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task StopListeningAsync_WhenListening_ReturnsSuccess()
    {
        await _sut.StartListeningAsync();

        var result = await _sut.StopListeningAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.IsListening.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessAudioChunkAsync_WhenNotListening_ReturnsFailure()
    {
        var result = await _sut.ProcessAudioChunkAsync(new byte[] { 1, 2, 3 });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Not listening");
    }

    [Fact]
    public async Task ProcessAudioChunkAsync_WhenListening_ReturnsSuccess()
    {
        await _sut.StartListeningAsync();

        var result = await _sut.ProcessAudioChunkAsync(new byte[] { 1, 2, 3 });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessAudioChunkAsync_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.ProcessAudioChunkAsync(new byte[] { 1, 2, 3 });

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessAudioChunkAsync_EmitsAudioChunk()
    {
        await _sut.StartListeningAsync();
        AudioChunk? received = null;
        _sut.AudioChunks.Subscribe(c => received = c);

        await _sut.ProcessAudioChunkAsync(new byte[] { 1, 2, 3 });

        received.Should().NotBeNull();
        received!.Data.Should().BeEquivalentTo(new byte[] { 1, 2, 3 });
    }

    [Fact]
    public async Task TranscribeFileAsync_Success_EmitsTranscription()
    {
        var transcription = new TranscriptionResult(
            "hello world", 0.95, "en", true,
            TimeSpan.Zero, TimeSpan.FromSeconds(1), null);
        _mockSttModel
            .Setup(m => m.TranscribeAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<TranscriptionResult, string>.Success(transcription));

        TranscriptionResult? received = null;
        _sut.Transcriptions.Subscribe(t => received = t);

        var result = await _sut.TranscribeFileAsync("test.wav");

        result.IsSuccess.Should().BeTrue();
        result.Value.Text.Should().Be("hello world");
        received.Should().NotBeNull();
    }

    [Fact]
    public async Task TranscribeFileAsync_Failure_ReturnsFailure()
    {
        _mockSttModel
            .Setup(m => m.TranscribeAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<TranscriptionResult, string>.Failure("file not found"));

        var result = await _sut.TranscribeFileAsync("missing.wav");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TranscribeFileAsync_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.TranscribeFileAsync("test.wav");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task StartListeningAsync_WithStreaming_CreatesSession()
    {
        var mockSession = new Mock<IStreamingTranscription>();
        using var resultsSubject = new Subject<TranscriptionResult>();
        using var voiceSubject = new Subject<VoiceActivity>();
        mockSession.Setup(s => s.Results).Returns(resultsSubject.AsObservable());
        mockSession.Setup(s => s.VoiceActivity).Returns(voiceSubject.AsObservable());

        _mockSttModel.Setup(m => m.SupportsStreaming).Returns(true);
        _mockSttModel.Setup(m => m.CreateStreamingSession(It.IsAny<string?>()))
            .Returns(mockSession.Object);

        var result = await _sut.StartListeningAsync();

        result.IsSuccess.Should().BeTrue();
        _mockSttModel.Verify(m => m.CreateStreamingSession(It.IsAny<string?>()), Times.Once);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        _sut.Dispose();

        var act = () => _sut.Dispose();

        act.Should().NotThrow();
    }
}
