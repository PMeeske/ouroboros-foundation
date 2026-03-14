using System.Reactive.Linq;
using Ouroboros.Core.EmbodiedInteraction;
using Ouroboros.Core.Monads;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class VoiceActuatorAdditionalTests : IDisposable
{
    private readonly Mock<ITtsModel> _mockTtsModel;
    private readonly VirtualSelf _virtualSelf;
    private readonly VoiceActuator _sut;

    public VoiceActuatorAdditionalTests()
    {
        _mockTtsModel = new Mock<ITtsModel>();
        _mockTtsModel.Setup(m => m.ModelName).Returns("test-tts");
        _mockTtsModel.Setup(m => m.SupportsStreaming).Returns(false);
        _virtualSelf = new VirtualSelf("TestAgent", fusionWindowMs: 60000);
        _sut = new VoiceActuator(_mockTtsModel.Object, _virtualSelf);
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
    public void Constructor_NullTtsModel_ThrowsArgumentNullException()
    {
        Action act = () => new VoiceActuator(null!, _virtualSelf);

        act.Should().Throw<ArgumentNullException>().WithParameterName("ttsModel");
    }

    [Fact]
    public void Constructor_NullVirtualSelf_ThrowsArgumentNullException()
    {
        Action act = () => new VoiceActuator(_mockTtsModel.Object, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("virtualSelf");
    }

    [Fact]
    public void Constructor_NullConfig_UsesDefaultConfig()
    {
        using var actuator = new VoiceActuator(_mockTtsModel.Object, _virtualSelf);
        actuator.Config.Should().NotBeNull();
        actuator.Config.Voice.Should().Be("default");
    }

    [Fact]
    public void Constructor_CustomConfig_UsesProvidedConfig()
    {
        var config = new VoiceConfig(Voice: "jenny", Speed: 1.2);
        using var actuator = new VoiceActuator(_mockTtsModel.Object, _virtualSelf, config);
        actuator.Config.Voice.Should().Be("jenny");
        actuator.Config.Speed.Should().Be(1.2);
    }

    // ========================================================================
    // Properties
    // ========================================================================

    [Fact]
    public void ModelName_ReturnsTtsModelName()
    {
        _sut.ModelName.Should().Be("test-tts");
    }

    [Fact]
    public void IsSpeaking_InitiallyFalse()
    {
        _sut.IsSpeaking.Should().BeFalse();
    }

    [Fact]
    public void SpeechOutput_IsObservable()
    {
        _sut.SpeechOutput.Should().NotBeNull();
    }

    // ========================================================================
    // Configure
    // ========================================================================

    [Fact]
    public void Configure_UpdatesConfig()
    {
        var newConfig = new VoiceConfig(Voice: "bob", Speed: 0.8);
        _sut.Configure(newConfig);

        _sut.Config.Voice.Should().Be("bob");
        _sut.Config.Speed.Should().Be(0.8);
    }

    [Fact]
    public void Configure_NullConfig_ThrowsArgumentNullException()
    {
        Action act = () => _sut.Configure(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ========================================================================
    // SetSpeed
    // ========================================================================

    [Theory]
    [InlineData(1.5, 1.5)]
    [InlineData(0.1, 0.5)]   // Clamped to min
    [InlineData(3.0, 2.0)]   // Clamped to max
    [InlineData(0.5, 0.5)]   // Exact min
    [InlineData(2.0, 2.0)]   // Exact max
    public void SetSpeed_ClampsToValidRange(double input, double expected)
    {
        _sut.SetSpeed(input);

        _sut.Config.Speed.Should().Be(expected);
    }

    // ========================================================================
    // SetStyle
    // ========================================================================

    [Fact]
    public void SetStyle_UpdatesConfigStyle()
    {
        _sut.SetStyle("cheerful");

        _sut.Config.Style.Should().Be("cheerful");
    }

    // ========================================================================
    // SetVoice
    // ========================================================================

    [Fact]
    public void SetVoice_UpdatesConfigVoice()
    {
        _sut.SetVoice("voice-123");

        _sut.Config.Voice.Should().Be("voice-123");
    }

    // ========================================================================
    // SpeakAsync
    // ========================================================================

    [Fact]
    public async Task SpeakAsync_WhenDisposed_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.SpeakAsync("Hello");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public async Task SpeakAsync_EmptyText_ReturnsFailure()
    {
        var result = await _sut.SpeakAsync("");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task SpeakAsync_WhitespaceText_ReturnsFailure()
    {
        var result = await _sut.SpeakAsync("   ");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task SpeakAsync_Success_EmitsSpeechOutput()
    {
        var speech = new SynthesizedSpeech("Hello", new byte[] { 1 }, "wav", 16000,
            TimeSpan.FromSeconds(0.5), DateTime.UtcNow);
        _mockTtsModel.Setup(m => m.SynthesizeAsync(
                It.IsAny<string>(), It.IsAny<VoiceConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SynthesizedSpeech, string>.Success(speech));

        SynthesizedSpeech? received = null;
        _sut.SpeechOutput.Subscribe(s => received = s);

        var result = await _sut.SpeakAsync("Hello");

        result.IsSuccess.Should().BeTrue();
        received.Should().NotBeNull();
        received!.Text.Should().Be("Hello");
    }

    [Fact]
    public async Task SpeakAsync_Failure_DoesNotEmitSpeechOutput()
    {
        _mockTtsModel.Setup(m => m.SynthesizeAsync(
                It.IsAny<string>(), It.IsAny<VoiceConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SynthesizedSpeech, string>.Failure("TTS error"));

        SynthesizedSpeech? received = null;
        _sut.SpeechOutput.Subscribe(s => received = s);

        var result = await _sut.SpeakAsync("Hello");

        result.IsSuccess.Should().BeFalse();
        received.Should().BeNull();
    }

    [Fact]
    public async Task SpeakAsync_SetsStateToSpeakingThenAwake()
    {
        var speech = new SynthesizedSpeech("Hi", new byte[] { 1 }, "wav", 16000,
            TimeSpan.FromSeconds(0.5), DateTime.UtcNow);
        _mockTtsModel.Setup(m => m.SynthesizeAsync(
                It.IsAny<string>(), It.IsAny<VoiceConfig>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SynthesizedSpeech, string>.Success(speech));

        await _sut.SpeakAsync("Hi");

        // After speaking finishes, state should be Awake
        _sut.IsSpeaking.Should().BeFalse();
        _virtualSelf.CurrentState.State.Should().Be(EmbodimentState.Awake);
    }

    [Fact]
    public async Task SpeakAsync_WithEmotion_UsesEmotionAsStyle()
    {
        var speech = new SynthesizedSpeech("Hi", new byte[] { 1 }, "wav", 16000,
            TimeSpan.FromSeconds(0.5), DateTime.UtcNow);
        VoiceConfig? capturedConfig = null;
        _mockTtsModel.Setup(m => m.SynthesizeAsync(
                It.IsAny<string>(), It.IsAny<VoiceConfig>(), It.IsAny<CancellationToken>()))
            .Callback<string, VoiceConfig, CancellationToken>((_, config, _) => capturedConfig = config)
            .ReturnsAsync(Result<SynthesizedSpeech, string>.Success(speech));

        await _sut.SpeakAsync("Hi", emotion: "happy");

        capturedConfig.Should().NotBeNull();
        capturedConfig!.Style.Should().Be("happy");
    }

    [Fact]
    public async Task SpeakAsync_WithoutEmotion_UsesCurrentConfig()
    {
        var speech = new SynthesizedSpeech("Hi", new byte[] { 1 }, "wav", 16000,
            TimeSpan.FromSeconds(0.5), DateTime.UtcNow);
        VoiceConfig? capturedConfig = null;
        _mockTtsModel.Setup(m => m.SynthesizeAsync(
                It.IsAny<string>(), It.IsAny<VoiceConfig>(), It.IsAny<CancellationToken>()))
            .Callback<string, VoiceConfig, CancellationToken>((_, config, _) => capturedConfig = config)
            .ReturnsAsync(Result<SynthesizedSpeech, string>.Success(speech));

        _sut.SetStyle("sad");
        await _sut.SpeakAsync("Hi");

        capturedConfig.Should().NotBeNull();
        capturedConfig!.Style.Should().Be("sad");
    }

    // ========================================================================
    // SpeakStreaming
    // ========================================================================

    [Fact]
    public void SpeakStreaming_WhenDisposed_ReturnsEmpty()
    {
        _sut.Dispose();

        var observable = _sut.SpeakStreaming("Hello");

        var items = new List<byte[]>();
        observable.Subscribe(b => items.Add(b));

        items.Should().BeEmpty();
    }

    [Fact]
    public void SpeakStreaming_ModelDoesNotSupportStreaming_ReturnsEmpty()
    {
        _mockTtsModel.Setup(m => m.SupportsStreaming).Returns(false);

        var observable = _sut.SpeakStreaming("Hello");

        var items = new List<byte[]>();
        observable.Subscribe(b => items.Add(b));

        items.Should().BeEmpty();
    }

    [Fact]
    public void SpeakStreaming_SupportsStreaming_CallsTtsModel()
    {
        _mockTtsModel.Setup(m => m.SupportsStreaming).Returns(true);
        _mockTtsModel.Setup(m => m.SynthesizeStreaming(
                It.IsAny<string>(), It.IsAny<VoiceConfig>(), It.IsAny<CancellationToken>()))
            .Returns(Observable.Empty<byte[]>());

        using var actuator = new VoiceActuator(_mockTtsModel.Object, _virtualSelf);

        actuator.SpeakStreaming("Hello");

        _mockTtsModel.Verify(m => m.SynthesizeStreaming(
            "Hello", It.IsAny<VoiceConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void SpeakStreaming_WithEmotion_UsesEmotionStyle()
    {
        _mockTtsModel.Setup(m => m.SupportsStreaming).Returns(true);
        VoiceConfig? capturedConfig = null;
        _mockTtsModel.Setup(m => m.SynthesizeStreaming(
                It.IsAny<string>(), It.IsAny<VoiceConfig>(), It.IsAny<CancellationToken>()))
            .Callback<string, VoiceConfig, CancellationToken>((_, config, _) => capturedConfig = config)
            .Returns(Observable.Empty<byte[]>());

        using var actuator = new VoiceActuator(_mockTtsModel.Object, _virtualSelf);
        actuator.SpeakStreaming("Hello", emotion: "excited");

        capturedConfig.Should().NotBeNull();
        capturedConfig!.Style.Should().Be("excited");
    }

    // ========================================================================
    // Interrupt
    // ========================================================================

    [Fact]
    public void Interrupt_WhenNotSpeaking_DoesNothing()
    {
        var act = () => _sut.Interrupt();

        act.Should().NotThrow();
        _sut.IsSpeaking.Should().BeFalse();
    }

    // ========================================================================
    // GetVoicesAsync
    // ========================================================================

    [Fact]
    public async Task GetVoicesAsync_DelegatesToTtsModel()
    {
        var voices = new List<VoiceInfo>
        {
            new("v1", "Voice1", "en-US", "Female", null)
        };
        _mockTtsModel.Setup(m => m.GetVoicesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<VoiceInfo>, string>.Success(voices));

        var result = await _sut.GetVoicesAsync("en-US");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
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
        bool speechCompleted = false;

        _sut.SpeechOutput.Subscribe(_ => { }, () => speechCompleted = true);

        _sut.Dispose();

        speechCompleted.Should().BeTrue();
    }
}
