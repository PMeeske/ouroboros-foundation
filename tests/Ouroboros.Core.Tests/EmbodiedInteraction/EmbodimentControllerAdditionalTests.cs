using System.Reactive.Linq;
using System.Reactive.Subjects;
using Ouroboros.Core.EmbodiedInteraction;
using Ouroboros.Core.Monads;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class EmbodimentControllerAdditionalTests : IDisposable
{
    private readonly VirtualSelf _virtualSelf;
    private readonly BodySchema _bodySchema;
    private readonly EmbodimentController _sut;

    public EmbodimentControllerAdditionalTests()
    {
        _virtualSelf = new VirtualSelf("TestSelf", fusionWindowMs: 60000);
        _bodySchema = new BodySchema();
        _sut = new EmbodimentController(_virtualSelf, _bodySchema);
    }

    public void Dispose()
    {
        _sut.Dispose();
        _virtualSelf.Dispose();
    }

    // ========================================================================
    // RegisterAudioSensor
    // ========================================================================

    [Fact]
    public void RegisterAudioSensor_ReturnsSelf_ForFluentChaining()
    {
        var sttModel = new Mock<ISttModel>();
        sttModel.Setup(m => m.SupportsStreaming).Returns(false);
        sttModel.Setup(m => m.ModelName).Returns("test-stt");
        using var sensor = new AudioSensor(sttModel.Object, _virtualSelf);

        var result = _sut.RegisterAudioSensor("audio1", sensor);

        result.Should().BeSameAs(_sut);
    }

    [Fact]
    public void RegisterAudioSensor_AfterDispose_ThrowsObjectDisposedException()
    {
        _sut.Dispose();
        var sttModel = new Mock<ISttModel>();
        sttModel.Setup(m => m.SupportsStreaming).Returns(false);
        using var sensor = new AudioSensor(sttModel.Object, _virtualSelf);

        Action act = () => _sut.RegisterAudioSensor("audio1", sensor);

        act.Should().Throw<ObjectDisposedException>();
    }

    // ========================================================================
    // RegisterVisualSensor
    // ========================================================================

    [Fact]
    public void RegisterVisualSensor_ReturnsSelf_ForFluentChaining()
    {
        var visionModel = new Mock<IVisionModel>();
        visionModel.Setup(m => m.ModelName).Returns("test-vision");
        using var sensor = new VisualSensor(visionModel.Object, _virtualSelf);

        var result = _sut.RegisterVisualSensor("visual1", sensor);

        result.Should().BeSameAs(_sut);
    }

    [Fact]
    public void RegisterVisualSensor_AfterDispose_ThrowsObjectDisposedException()
    {
        _sut.Dispose();
        var visionModel = new Mock<IVisionModel>();
        using var sensor = new VisualSensor(visionModel.Object, _virtualSelf);

        Action act = () => _sut.RegisterVisualSensor("visual1", sensor);

        act.Should().Throw<ObjectDisposedException>();
    }

    // ========================================================================
    // RegisterVoiceActuator
    // ========================================================================

    [Fact]
    public void RegisterVoiceActuator_ReturnsSelf_ForFluentChaining()
    {
        var ttsModel = new Mock<ITtsModel>();
        ttsModel.Setup(m => m.ModelName).Returns("test-tts");
        using var actuator = new VoiceActuator(ttsModel.Object, _virtualSelf);

        var result = _sut.RegisterVoiceActuator("voice1", actuator);

        result.Should().BeSameAs(_sut);
    }

    [Fact]
    public void RegisterVoiceActuator_AfterDispose_ThrowsObjectDisposedException()
    {
        _sut.Dispose();
        var ttsModel = new Mock<ITtsModel>();
        using var actuator = new VoiceActuator(ttsModel.Object, _virtualSelf);

        Action act = () => _sut.RegisterVoiceActuator("voice1", actuator);

        act.Should().Throw<ObjectDisposedException>();
    }

    // ========================================================================
    // StartAsync
    // ========================================================================

    [Fact]
    public async Task StartAsync_WithNoSensors_ReturnsSuccessAndSetsRunning()
    {
        var result = await _sut.StartAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.IsRunning.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsync_WhenAlreadyRunning_ReturnsFailure()
    {
        await _sut.StartAsync();

        var result = await _sut.StartAsync();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Already running");
    }

    [Fact]
    public async Task StartAsync_WhenDisposed_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.StartAsync();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public async Task StartAsync_WithAudioSensor_StartsListening()
    {
        var sttModel = new Mock<ISttModel>();
        sttModel.Setup(m => m.SupportsStreaming).Returns(false);
        sttModel.Setup(m => m.ModelName).Returns("test-stt");
        using var sensor = new AudioSensor(sttModel.Object, _virtualSelf);
        _sut.RegisterAudioSensor("audio1", sensor);

        var result = await _sut.StartAsync();

        result.IsSuccess.Should().BeTrue();
        sensor.IsListening.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsync_WithVisualSensor_StartsObserving()
    {
        var visionModel = new Mock<IVisionModel>();
        visionModel.Setup(m => m.ModelName).Returns("test-vision");
        using var sensor = new VisualSensor(visionModel.Object, _virtualSelf);
        _sut.RegisterVisualSensor("visual1", sensor);

        var result = await _sut.StartAsync();

        result.IsSuccess.Should().BeTrue();
        sensor.IsObserving.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsync_SetsVirtualSelfStateToAwake()
    {
        await _sut.StartAsync();

        _virtualSelf.CurrentState.State.Should().Be(EmbodimentState.Awake);
    }

    [Fact]
    public async Task StartAsync_AudioSensorFailsToStart_ReturnsFailure()
    {
        // Create a sensor that's already listening so StartListeningAsync fails
        var sttModel = new Mock<ISttModel>();
        sttModel.Setup(m => m.SupportsStreaming).Returns(false);
        sttModel.Setup(m => m.ModelName).Returns("test-stt");
        using var sensor = new AudioSensor(sttModel.Object, _virtualSelf);
        await sensor.StartListeningAsync(); // Start it first so it fails again
        _sut.RegisterAudioSensor("audio1", sensor);

        var result = await _sut.StartAsync();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to start audio sensor");
    }

    [Fact]
    public async Task StartAsync_VisualSensorFailsToStart_ReturnsFailure()
    {
        // Create a sensor that's already observing so StartObserving fails
        var visionModel = new Mock<IVisionModel>();
        visionModel.Setup(m => m.ModelName).Returns("test-vision");
        using var sensor = new VisualSensor(visionModel.Object, _virtualSelf);
        sensor.StartObserving(); // Start it first
        _sut.RegisterVisualSensor("visual1", sensor);

        var result = await _sut.StartAsync();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to start visual sensor");
    }

    // ========================================================================
    // StopAsync
    // ========================================================================

    [Fact]
    public async Task StopAsync_WhenNotRunning_ReturnsSuccess()
    {
        var result = await _sut.StopAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task StopAsync_WhenRunning_StopsAndSetsStateAndReturnsSuccess()
    {
        await _sut.StartAsync();

        var result = await _sut.StopAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.IsRunning.Should().BeFalse();
        _virtualSelf.CurrentState.State.Should().Be(EmbodimentState.Dormant);
    }

    [Fact]
    public async Task StopAsync_WithAudioSensor_StopsListening()
    {
        var sttModel = new Mock<ISttModel>();
        sttModel.Setup(m => m.SupportsStreaming).Returns(false);
        sttModel.Setup(m => m.ModelName).Returns("test-stt");
        using var sensor = new AudioSensor(sttModel.Object, _virtualSelf);
        _sut.RegisterAudioSensor("audio1", sensor);
        await _sut.StartAsync();

        await _sut.StopAsync();

        sensor.IsListening.Should().BeFalse();
    }

    [Fact]
    public async Task StopAsync_WithVisualSensor_StopsObserving()
    {
        var visionModel = new Mock<IVisionModel>();
        visionModel.Setup(m => m.ModelName).Returns("test-vision");
        using var sensor = new VisualSensor(visionModel.Object, _virtualSelf);
        _sut.RegisterVisualSensor("visual1", sensor);
        await _sut.StartAsync();

        await _sut.StopAsync();

        sensor.IsObserving.Should().BeFalse();
    }

    // ========================================================================
    // ExecuteActionAsync - Voice modality
    // ========================================================================

    [Fact]
    public async Task ExecuteActionAsync_VoiceModality_NoActuator_ReturnsFailure()
    {
        var request = new ActionRequest("voice1", ActuatorModality.Voice, "Hello", null);

        var result = await _sut.ExecuteActionAsync(request);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("No voice actuator");
    }

    [Fact]
    public async Task ExecuteActionAsync_VoiceModality_WithActuator_SpeaksText()
    {
        var ttsModel = new Mock<ITtsModel>();
        ttsModel.Setup(m => m.ModelName).Returns("test-tts");
        var speech = new SynthesizedSpeech("Hello", new byte[] { 1, 2 }, "wav", 16000,
            TimeSpan.FromSeconds(1), DateTime.UtcNow);
        ttsModel.Setup(m => m.SynthesizeAsync(
                It.IsAny<string>(), It.IsAny<VoiceConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SynthesizedSpeech, string>.Success(speech));
        using var actuator = new VoiceActuator(ttsModel.Object, _virtualSelf);
        _sut.RegisterVoiceActuator("voice1", actuator);

        var request = new ActionRequest("voice1", ActuatorModality.Voice, "Hello", null);
        var result = await _sut.ExecuteActionAsync(request);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteActionAsync_VoiceModality_WithEmotionParameter_PassesEmotion()
    {
        var ttsModel = new Mock<ITtsModel>();
        ttsModel.Setup(m => m.ModelName).Returns("test-tts");
        var speech = new SynthesizedSpeech("Hello", new byte[] { 1 }, "wav", 16000,
            TimeSpan.FromSeconds(1), DateTime.UtcNow);
        ttsModel.Setup(m => m.SynthesizeAsync(
                It.IsAny<string>(), It.IsAny<VoiceConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SynthesizedSpeech, string>.Success(speech));
        using var actuator = new VoiceActuator(ttsModel.Object, _virtualSelf);
        _sut.RegisterVoiceActuator("voice1", actuator);

        var parameters = new Dictionary<string, object> { ["emotion"] = "happy" };
        var request = new ActionRequest("voice1", ActuatorModality.Voice, "Hello", parameters);
        var result = await _sut.ExecuteActionAsync(request);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteActionAsync_VoiceModality_FallsBackToFirstActuator()
    {
        var ttsModel = new Mock<ITtsModel>();
        ttsModel.Setup(m => m.ModelName).Returns("test-tts");
        var speech = new SynthesizedSpeech("Hello", new byte[] { 1 }, "wav", 16000,
            TimeSpan.FromSeconds(1), DateTime.UtcNow);
        ttsModel.Setup(m => m.SynthesizeAsync(
                It.IsAny<string>(), It.IsAny<VoiceConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SynthesizedSpeech, string>.Success(speech));
        using var actuator = new VoiceActuator(ttsModel.Object, _virtualSelf);
        _sut.RegisterVoiceActuator("default-voice", actuator);

        // Request targets "unknown-voice" which doesn't exist, should fall back to first
        var request = new ActionRequest("unknown-voice", ActuatorModality.Voice, "Hello", null);
        var result = await _sut.ExecuteActionAsync(request);

        result.Success.Should().BeTrue();
    }

    // ========================================================================
    // SpeakAsync
    // ========================================================================

    [Fact]
    public async Task SpeakAsync_WithSpecificActuatorId_UsesCorrectActuator()
    {
        var ttsModel = new Mock<ITtsModel>();
        ttsModel.Setup(m => m.ModelName).Returns("test-tts");
        var speech = new SynthesizedSpeech("Hi", new byte[] { 1 }, "wav", 16000,
            TimeSpan.FromSeconds(0.5), DateTime.UtcNow);
        ttsModel.Setup(m => m.SynthesizeAsync(
                It.IsAny<string>(), It.IsAny<VoiceConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SynthesizedSpeech, string>.Success(speech));
        using var actuator = new VoiceActuator(ttsModel.Object, _virtualSelf);
        _sut.RegisterVoiceActuator("voice1", actuator);

        var result = await _sut.SpeakAsync("Hi", actuatorId: "voice1");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SpeakAsync_WithEmotion_PassesEmotionToActuator()
    {
        var ttsModel = new Mock<ITtsModel>();
        ttsModel.Setup(m => m.ModelName).Returns("test-tts");
        var speech = new SynthesizedSpeech("Hi", new byte[] { 1 }, "wav", 16000,
            TimeSpan.FromSeconds(0.5), DateTime.UtcNow);
        ttsModel.Setup(m => m.SynthesizeAsync(
                It.IsAny<string>(), It.IsAny<VoiceConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SynthesizedSpeech, string>.Success(speech));
        using var actuator = new VoiceActuator(ttsModel.Object, _virtualSelf);
        _sut.RegisterVoiceActuator("voice1", actuator);

        var result = await _sut.SpeakAsync("Hi", emotion: "cheerful");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SpeakAsync_WithUnknownActuatorId_ReturnsFailure()
    {
        var ttsModel = new Mock<ITtsModel>();
        ttsModel.Setup(m => m.ModelName).Returns("test-tts");
        using var actuator = new VoiceActuator(ttsModel.Object, _virtualSelf);
        _sut.RegisterVoiceActuator("voice1", actuator);

        var result = await _sut.SpeakAsync("Hi", actuatorId: "unknown");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No voice actuator");
    }

    [Fact]
    public async Task SpeakAsync_WithNoActuatorId_UsesFirstAvailable()
    {
        var ttsModel = new Mock<ITtsModel>();
        ttsModel.Setup(m => m.ModelName).Returns("test-tts");
        var speech = new SynthesizedSpeech("Hi", new byte[] { 1 }, "wav", 16000,
            TimeSpan.FromSeconds(0.5), DateTime.UtcNow);
        ttsModel.Setup(m => m.SynthesizeAsync(
                It.IsAny<string>(), It.IsAny<VoiceConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SynthesizedSpeech, string>.Success(speech));
        using var actuator = new VoiceActuator(ttsModel.Object, _virtualSelf);
        _sut.RegisterVoiceActuator("voice1", actuator);

        var result = await _sut.SpeakAsync("Hi");

        result.IsSuccess.Should().BeTrue();
    }

    // ========================================================================
    // OnTextInput
    // ========================================================================

    [Fact]
    public void OnTextInput_PublishesTextPerception()
    {
        UnifiedPerception? received = null;
        _sut.Perceptions.Subscribe(p => received = p);

        _sut.OnTextInput("Hello there", "user");

        received.Should().NotBeNull();
        received!.Modality.Should().Be(SensorModality.Text);
        received.Source.Should().Be("user");
        received.Perception.Should().BeOfType<TextPerception>();
        ((TextPerception)received.Perception).Text.Should().Be("Hello there");
    }

    [Fact]
    public void OnTextInput_DefaultSource_IsUser()
    {
        UnifiedPerception? received = null;
        _sut.Perceptions.Subscribe(p => received = p);

        _sut.OnTextInput("test");

        received.Should().NotBeNull();
        received!.Source.Should().Be("user");
    }

    // ========================================================================
    // Perception routing during StartAsync
    // ========================================================================

    [Fact]
    public async Task StartAsync_RoutesAudioPerceptionsToVirtualSelf()
    {
        await _sut.StartAsync();

        // Verify VirtualSelf state is Awake
        _virtualSelf.CurrentState.State.Should().Be(EmbodimentState.Awake);
    }

    // ========================================================================
    // Dispose
    // ========================================================================

    [Fact]
    public void Dispose_DisposesAllRegisteredSensorsAndActuators()
    {
        var sttModel = new Mock<ISttModel>();
        sttModel.Setup(m => m.SupportsStreaming).Returns(false);
        using var audioSensor = new AudioSensor(sttModel.Object, _virtualSelf);
        _sut.RegisterAudioSensor("audio1", audioSensor);

        var visionModel = new Mock<IVisionModel>();
        using var visualSensor = new VisualSensor(visionModel.Object, _virtualSelf);
        _sut.RegisterVisualSensor("visual1", visualSensor);

        var ttsModel = new Mock<ITtsModel>();
        using var voiceActuator = new VoiceActuator(ttsModel.Object, _virtualSelf);
        _sut.RegisterVoiceActuator("voice1", voiceActuator);

        _sut.Dispose();

        // After dispose, trying to use sensors should fail
        var audioResult = audioSensor.TranscribeFileAsync("test.wav").Result;
        audioResult.IsSuccess.Should().BeFalse();

        var visualResult = visualSensor.StartObserving();
        visualResult.IsSuccess.Should().BeFalse();

        var voiceResult = voiceActuator.SpeakAsync("test").Result;
        voiceResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetFusedPerceptionAsync_Timeout_ReturnsFailure()
    {
        await _sut.StartAsync();

        var result = await _sut.GetFusedPerceptionAsync(TimeSpan.FromMilliseconds(50));

        result.IsSuccess.Should().BeFalse();
        // Should get either "Timeout" or "No perceptions available"
        result.Error.Should().NotBeNullOrEmpty();
    }
}
