using System.Reactive.Linq;
using System.Reactive.Subjects;
using Ouroboros.Core.EmbodiedInteraction;
using Ouroboros.Core.Monads;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class AudioSensorAdditionalTests : IDisposable
{
    private readonly Mock<ISttModel> _mockSttModel;
    private readonly VirtualSelf _virtualSelf;
    private readonly AudioSensor _sut;

    public AudioSensorAdditionalTests()
    {
        _mockSttModel = new Mock<ISttModel>();
        _mockSttModel.Setup(m => m.ModelName).Returns("test-stt");
        _mockSttModel.Setup(m => m.SupportsStreaming).Returns(false);
        _virtualSelf = new VirtualSelf("TestAgent", fusionWindowMs: 60000);
        _sut = new AudioSensor(_mockSttModel.Object, _virtualSelf);
    }

    public void Dispose()
    {
        _sut.Dispose();
        _virtualSelf.Dispose();
    }

    // ========================================================================
    // Constructor
    // ========================================================================

    [Fact]
    public void Constructor_NullSttModel_ThrowsArgumentNullException()
    {
        Action act = () => new AudioSensor(null!, _virtualSelf);

        act.Should().Throw<ArgumentNullException>().WithParameterName("sttModel");
    }

    [Fact]
    public void Constructor_NullVirtualSelf_ThrowsArgumentNullException()
    {
        Action act = () => new AudioSensor(_mockSttModel.Object, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("virtualSelf");
    }

    [Fact]
    public void Constructor_NullConfig_UsesDefaultConfig()
    {
        var sensor = new AudioSensor(_mockSttModel.Object, _virtualSelf);
        sensor.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_CustomConfig_UsesProvidedConfig()
    {
        var config = new AudioSensorConfig(SampleRate: 44100, Channels: 2, Language: "fr");
        var sensor = new AudioSensor(_mockSttModel.Object, _virtualSelf, config);
        sensor.Should().NotBeNull();
    }

    // ========================================================================
    // Properties
    // ========================================================================

    [Fact]
    public void ModelName_ReturnsSttModelName()
    {
        _sut.ModelName.Should().Be("test-stt");
    }

    [Fact]
    public void IsListening_InitiallyFalse()
    {
        _sut.IsListening.Should().BeFalse();
    }

    [Fact]
    public void AudioChunks_IsObservable()
    {
        _sut.AudioChunks.Should().NotBeNull();
    }

    [Fact]
    public void Transcriptions_IsObservable()
    {
        _sut.Transcriptions.Should().NotBeNull();
    }

    [Fact]
    public void VoiceActivityEvents_IsObservable()
    {
        _sut.VoiceActivityEvents.Should().NotBeNull();
    }

    // ========================================================================
    // StartListeningAsync
    // ========================================================================

    [Fact]
    public async Task StartListeningAsync_NonStreamingModel_SetsListeningTrue()
    {
        var result = await _sut.StartListeningAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.IsListening.Should().BeTrue();
    }

    [Fact]
    public async Task StartListeningAsync_ActivatesAudioSensor()
    {
        await _sut.StartListeningAsync();

        _virtualSelf.CurrentState.ActiveSensors.Should().Contain(SensorModality.Audio);
    }

    [Fact]
    public async Task StartListeningAsync_SetsStateToListening()
    {
        await _sut.StartListeningAsync();

        _virtualSelf.CurrentState.State.Should().Be(EmbodimentState.Listening);
    }

    [Fact]
    public async Task StartListeningAsync_WhenAlreadyListening_ReturnsFailure()
    {
        await _sut.StartListeningAsync();

        var result = await _sut.StartListeningAsync();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Already listening");
    }

    [Fact]
    public async Task StartListeningAsync_WhenDisposed_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.StartListeningAsync();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public async Task StartListeningAsync_StreamingModel_CreatesSession()
    {
        var mockSession = new Mock<IStreamingTranscription>();
        mockSession.Setup(s => s.Results).Returns(new Subject<TranscriptionResult>().AsObservable());
        mockSession.Setup(s => s.VoiceActivity).Returns(new Subject<VoiceActivity>().AsObservable());

        _mockSttModel.Setup(m => m.SupportsStreaming).Returns(true);
        _mockSttModel.Setup(m => m.CreateStreamingSession(It.IsAny<string>()))
            .Returns(mockSession.Object);

        var sensor = new AudioSensor(_mockSttModel.Object, _virtualSelf);

        var result = await sensor.StartListeningAsync();

        result.IsSuccess.Should().BeTrue();
        _mockSttModel.Verify(m => m.CreateStreamingSession(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task StartListeningAsync_StreamingModel_ForwardsFinalTranscriptionsToVirtualSelf()
    {
        var resultsSubject = new Subject<TranscriptionResult>();
        var mockSession = new Mock<IStreamingTranscription>();
        mockSession.Setup(s => s.Results).Returns(resultsSubject.AsObservable());
        mockSession.Setup(s => s.VoiceActivity).Returns(new Subject<VoiceActivity>().AsObservable());

        _mockSttModel.Setup(m => m.SupportsStreaming).Returns(true);
        _mockSttModel.Setup(m => m.CreateStreamingSession(It.IsAny<string>()))
            .Returns(mockSession.Object);

        var sensor = new AudioSensor(_mockSttModel.Object, _virtualSelf);

        TranscriptionResult? capturedTranscription = null;
        sensor.Transcriptions.Subscribe(t => capturedTranscription = t);

        await sensor.StartListeningAsync();

        var transcription = new TranscriptionResult(
            "Hello", 0.95, "en", true, TimeSpan.Zero, TimeSpan.FromSeconds(1), null);
        resultsSubject.OnNext(transcription);

        capturedTranscription.Should().NotBeNull();
        capturedTranscription!.Text.Should().Be("Hello");
    }

    [Fact]
    public async Task StartListeningAsync_StreamingModel_ForwardsVoiceActivity()
    {
        var voiceActivitySubject = new Subject<VoiceActivity>();
        var mockSession = new Mock<IStreamingTranscription>();
        mockSession.Setup(s => s.Results).Returns(new Subject<TranscriptionResult>().AsObservable());
        mockSession.Setup(s => s.VoiceActivity).Returns(voiceActivitySubject.AsObservable());

        _mockSttModel.Setup(m => m.SupportsStreaming).Returns(true);
        _mockSttModel.Setup(m => m.CreateStreamingSession(It.IsAny<string>()))
            .Returns(mockSession.Object);

        var sensor = new AudioSensor(_mockSttModel.Object, _virtualSelf);

        VoiceActivity? capturedActivity = null;
        sensor.VoiceActivityEvents.Subscribe(a => capturedActivity = a);

        await sensor.StartListeningAsync();

        voiceActivitySubject.OnNext(VoiceActivity.SpeechDetected);

        capturedActivity.Should().Be(VoiceActivity.SpeechDetected);
    }

    // ========================================================================
    // StopListeningAsync
    // ========================================================================

    [Fact]
    public async Task StopListeningAsync_WhenNotListening_ReturnsSuccess()
    {
        var result = await _sut.StopListeningAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task StopListeningAsync_WhenListening_StopsAndDeactivatesSensor()
    {
        await _sut.StartListeningAsync();

        var result = await _sut.StopListeningAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.IsListening.Should().BeFalse();
    }

    [Fact]
    public async Task StopListeningAsync_WithStreamingSession_EndsAndDisposesSession()
    {
        var mockSession = new Mock<IStreamingTranscription>();
        mockSession.Setup(s => s.Results).Returns(new Subject<TranscriptionResult>().AsObservable());
        mockSession.Setup(s => s.VoiceActivity).Returns(new Subject<VoiceActivity>().AsObservable());
        mockSession.Setup(s => s.EndAudioAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mockSession.Setup(s => s.DisposeAsync()).Returns(ValueTask.CompletedTask);

        _mockSttModel.Setup(m => m.SupportsStreaming).Returns(true);
        _mockSttModel.Setup(m => m.CreateStreamingSession(It.IsAny<string>()))
            .Returns(mockSession.Object);

        var sensor = new AudioSensor(_mockSttModel.Object, _virtualSelf);
        await sensor.StartListeningAsync();

        var result = await sensor.StopListeningAsync();

        result.IsSuccess.Should().BeTrue();
        mockSession.Verify(s => s.EndAudioAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockSession.Verify(s => s.DisposeAsync(), Times.Once);
    }

    // ========================================================================
    // ProcessAudioChunkAsync
    // ========================================================================

    [Fact]
    public async Task ProcessAudioChunkAsync_WhenDisposed_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.ProcessAudioChunkAsync(new byte[] { 1, 2, 3 });

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public async Task ProcessAudioChunkAsync_WhenNotListening_ReturnsFailure()
    {
        var result = await _sut.ProcessAudioChunkAsync(new byte[] { 1, 2, 3 });

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Not listening");
    }

    [Fact]
    public async Task ProcessAudioChunkAsync_EmitsAudioChunk()
    {
        await _sut.StartListeningAsync();

        AudioChunk? received = null;
        _sut.AudioChunks.Subscribe(c => received = c);

        var result = await _sut.ProcessAudioChunkAsync(new byte[] { 1, 2, 3 });

        result.IsSuccess.Should().BeTrue();
        received.Should().NotBeNull();
        received!.Data.Should().BeEquivalentTo(new byte[] { 1, 2, 3 });
    }

    [Fact]
    public async Task ProcessAudioChunkAsync_WithStreamingSession_PushesAudio()
    {
        var mockSession = new Mock<IStreamingTranscription>();
        mockSession.Setup(s => s.Results).Returns(new Subject<TranscriptionResult>().AsObservable());
        mockSession.Setup(s => s.VoiceActivity).Returns(new Subject<VoiceActivity>().AsObservable());
        mockSession.Setup(s => s.PushAudioAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockSttModel.Setup(m => m.SupportsStreaming).Returns(true);
        _mockSttModel.Setup(m => m.CreateStreamingSession(It.IsAny<string>()))
            .Returns(mockSession.Object);

        var sensor = new AudioSensor(_mockSttModel.Object, _virtualSelf);
        await sensor.StartListeningAsync();

        var result = await sensor.ProcessAudioChunkAsync(new byte[] { 1, 2 });

        result.IsSuccess.Should().BeTrue();
        mockSession.Verify(s => s.PushAudioAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessAudioChunkAsync_FinalChunk_EndsAudioOnSession()
    {
        var mockSession = new Mock<IStreamingTranscription>();
        mockSession.Setup(s => s.Results).Returns(new Subject<TranscriptionResult>().AsObservable());
        mockSession.Setup(s => s.VoiceActivity).Returns(new Subject<VoiceActivity>().AsObservable());
        mockSession.Setup(s => s.PushAudioAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mockSession.Setup(s => s.EndAudioAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockSttModel.Setup(m => m.SupportsStreaming).Returns(true);
        _mockSttModel.Setup(m => m.CreateStreamingSession(It.IsAny<string>()))
            .Returns(mockSession.Object);

        var sensor = new AudioSensor(_mockSttModel.Object, _virtualSelf);
        await sensor.StartListeningAsync();

        var result = await sensor.ProcessAudioChunkAsync(new byte[] { 1, 2 }, isFinal: true);

        result.IsSuccess.Should().BeTrue();
        mockSession.Verify(s => s.EndAudioAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ========================================================================
    // TranscribeFileAsync
    // ========================================================================

    [Fact]
    public async Task TranscribeFileAsync_WhenDisposed_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.TranscribeFileAsync("test.wav");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public async Task TranscribeFileAsync_Success_EmitsTranscription()
    {
        var transcription = new TranscriptionResult(
            "Hello world", 0.92, "en", true,
            TimeSpan.Zero, TimeSpan.FromSeconds(2), null);
        _mockSttModel.Setup(m => m.TranscribeAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<TranscriptionResult, string>.Success(transcription));

        TranscriptionResult? received = null;
        _sut.Transcriptions.Subscribe(t => received = t);

        var result = await _sut.TranscribeFileAsync("test.wav");

        result.IsSuccess.Should().BeTrue();
        result.Value.Text.Should().Be("Hello world");
        received.Should().NotBeNull();
        received!.Text.Should().Be("Hello world");
    }

    [Fact]
    public async Task TranscribeFileAsync_Success_PublishesToVirtualSelf()
    {
        var transcription = new TranscriptionResult(
            "Hi there", 0.9, "en", true,
            TimeSpan.Zero, TimeSpan.FromSeconds(1), null);
        _mockSttModel.Setup(m => m.TranscribeAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<TranscriptionResult, string>.Success(transcription));

        PerceptionEvent? received = null;
        _virtualSelf.Perceptions.Subscribe(p => received = p);

        await _sut.TranscribeFileAsync("test.wav");

        received.Should().NotBeNull();
        received.Should().BeOfType<AudioPerception>();
    }

    [Fact]
    public async Task TranscribeFileAsync_Failure_DoesNotEmitTranscription()
    {
        _mockSttModel.Setup(m => m.TranscribeAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<TranscriptionResult, string>.Failure("file not found"));

        TranscriptionResult? received = null;
        _sut.Transcriptions.Subscribe(t => received = t);

        var result = await _sut.TranscribeFileAsync("missing.wav");

        result.IsSuccess.Should().BeFalse();
        received.Should().BeNull();
    }

    // ========================================================================
    // Dispose
    // ========================================================================

    [Fact]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        _sut.Dispose();

        var act = () => _sut.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CompletesSubjects()
    {
        bool chunksCompleted = false;
        bool transcriptionsCompleted = false;
        bool voiceActivityCompleted = false;

        _sut.AudioChunks.Subscribe(_ => { }, () => chunksCompleted = true);
        _sut.Transcriptions.Subscribe(_ => { }, () => transcriptionsCompleted = true);
        _sut.VoiceActivityEvents.Subscribe(_ => { }, () => voiceActivityCompleted = true);

        _sut.Dispose();

        chunksCompleted.Should().BeTrue();
        transcriptionsCompleted.Should().BeTrue();
        voiceActivityCompleted.Should().BeTrue();
    }
}
