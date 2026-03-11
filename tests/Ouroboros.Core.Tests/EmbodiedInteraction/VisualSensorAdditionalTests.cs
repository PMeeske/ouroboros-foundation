using System.Reactive.Linq;
using Ouroboros.Core.EmbodiedInteraction;
using Ouroboros.Core.Monads;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class VisualSensorAdditionalTests : IDisposable
{
    private readonly Mock<IVisionModel> _mockVisionModel;
    private readonly VirtualSelf _virtualSelf;
    private readonly VisualSensor _sut;

    public VisualSensorAdditionalTests()
    {
        _mockVisionModel = new Mock<IVisionModel>();
        _mockVisionModel.Setup(m => m.ModelName).Returns("test-vision");
        _virtualSelf = new VirtualSelf("TestAgent", fusionWindowMs: 60000);
        _sut = new VisualSensor(_mockVisionModel.Object, _virtualSelf);
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
    public void Constructor_NullVisionModel_ThrowsArgumentNullException()
    {
        Action act = () => new VisualSensor(null!, _virtualSelf);

        act.Should().Throw<ArgumentNullException>().WithParameterName("visionModel");
    }

    [Fact]
    public void Constructor_NullVirtualSelf_ThrowsArgumentNullException()
    {
        Action act = () => new VisualSensor(_mockVisionModel.Object, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("virtualSelf");
    }

    [Fact]
    public void Constructor_NullConfig_UsesDefaultConfig()
    {
        var sensor = new VisualSensor(_mockVisionModel.Object, _virtualSelf);
        sensor.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_CustomConfig_UsesProvidedConfig()
    {
        var config = new VisualSensorConfig(Width: 1920, Height: 1080, ProcessEveryNthFrame: 10);
        var sensor = new VisualSensor(_mockVisionModel.Object, _virtualSelf, config);
        sensor.Should().NotBeNull();
    }

    // ========================================================================
    // Properties
    // ========================================================================

    [Fact]
    public void ModelName_ReturnsVisionModelName()
    {
        _sut.ModelName.Should().Be("test-vision");
    }

    [Fact]
    public void IsObserving_InitiallyFalse()
    {
        _sut.IsObserving.Should().BeFalse();
    }

    [Fact]
    public void FrameCount_InitiallyZero()
    {
        _sut.FrameCount.Should().Be(0);
    }

    [Fact]
    public void Frames_IsObservable()
    {
        _sut.Frames.Should().NotBeNull();
    }

    [Fact]
    public void AnalysisResults_IsObservable()
    {
        _sut.AnalysisResults.Should().NotBeNull();
    }

    // ========================================================================
    // StartObserving / StopObserving
    // ========================================================================

    [Fact]
    public void StartObserving_SetsIsObservingTrue()
    {
        var result = _sut.StartObserving();

        result.IsSuccess.Should().BeTrue();
        _sut.IsObserving.Should().BeTrue();
    }

    [Fact]
    public void StartObserving_ActivatesVisualSensorOnVirtualSelf()
    {
        _sut.StartObserving();

        _virtualSelf.CurrentState.ActiveSensors.Should().Contain(SensorModality.Visual);
    }

    [Fact]
    public void StartObserving_SetsStateToObserving()
    {
        _sut.StartObserving();

        _virtualSelf.CurrentState.State.Should().Be(EmbodimentState.Observing);
    }

    [Fact]
    public void StartObserving_WhenAlreadyObserving_ReturnsFailure()
    {
        _sut.StartObserving();

        var result = _sut.StartObserving();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Already observing");
    }

    [Fact]
    public void StartObserving_WhenDisposed_ReturnsFailure()
    {
        _sut.Dispose();

        var result = _sut.StartObserving();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public void StopObserving_WhenNotObserving_ReturnsSuccess()
    {
        var result = _sut.StopObserving();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void StopObserving_WhenObserving_SetsIsObservingFalse()
    {
        _sut.StartObserving();

        var result = _sut.StopObserving();

        result.IsSuccess.Should().BeTrue();
        _sut.IsObserving.Should().BeFalse();
    }

    [Fact]
    public void StopObserving_DeactivatesVisualSensorOnVirtualSelf()
    {
        _sut.StartObserving();

        _sut.StopObserving();

        _virtualSelf.CurrentState.ActiveSensors.Should().NotContain(SensorModality.Visual);
    }

    // ========================================================================
    // ProcessFrameAsync
    // ========================================================================

    [Fact]
    public async Task ProcessFrameAsync_WhenDisposed_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.ProcessFrameAsync(new byte[] { 1 }, 640, 480);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public async Task ProcessFrameAsync_WhenNotObserving_ReturnsFailure()
    {
        var result = await _sut.ProcessFrameAsync(new byte[] { 1 }, 640, 480);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Not observing");
    }

    [Fact]
    public async Task ProcessFrameAsync_IncrementsFrameCount()
    {
        _sut.StartObserving();

        await _sut.ProcessFrameAsync(new byte[] { 1 }, 640, 480);

        _sut.FrameCount.Should().Be(1);
    }

    [Fact]
    public async Task ProcessFrameAsync_EmitsVideoFrame()
    {
        _sut.StartObserving();
        VideoFrame? received = null;
        _sut.Frames.Subscribe(f => received = f);

        await _sut.ProcessFrameAsync(new byte[] { 1 }, 640, 480, "rgb24");

        received.Should().NotBeNull();
        received!.Width.Should().Be(640);
        received.Height.Should().Be(480);
        received.Format.Should().Be("rgb24");
    }

    [Fact]
    public async Task ProcessFrameAsync_NonNthFrame_ReturnsNone()
    {
        // Default config processes every 5th frame
        _sut.StartObserving();

        var result = await _sut.ProcessFrameAsync(new byte[] { 1 }, 640, 480);

        // Frame 1 is not the 5th frame, so should skip analysis
        result.IsSuccess.Should().BeTrue();
        result.Value.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessFrameAsync_NthFrame_AnalyzesAndReturnsResult()
    {
        // Use config that processes every frame
        var config = new VisualSensorConfig(ProcessEveryNthFrame: 1);
        var sensor = new VisualSensor(_mockVisionModel.Object, _virtualSelf, config);

        var analysisResult = new VisionAnalysisResult(
            "a room", new List<DetectedObject>(), new List<DetectedFace>(),
            "indoor", null, null, 0.9, 100);
        _mockVisionModel.Setup(m => m.AnalyzeImageAsync(
                It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<VisionAnalysisOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<VisionAnalysisResult, string>.Success(analysisResult));

        sensor.StartObserving();

        var result = await sensor.ProcessFrameAsync(new byte[] { 1 }, 640, 480);

        result.IsSuccess.Should().BeTrue();
        result.Value.HasValue.Should().BeTrue();
        result.Value.Value.Description.Should().Be("a room");
    }

    [Fact]
    public async Task ProcessFrameAsync_NthFrame_EmitsAnalysisResult()
    {
        var config = new VisualSensorConfig(ProcessEveryNthFrame: 1);
        var sensor = new VisualSensor(_mockVisionModel.Object, _virtualSelf, config);

        var analysisResult = new VisionAnalysisResult(
            "a room", new List<DetectedObject>(), new List<DetectedFace>(),
            "indoor", null, null, 0.9, 100);
        _mockVisionModel.Setup(m => m.AnalyzeImageAsync(
                It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<VisionAnalysisOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<VisionAnalysisResult, string>.Success(analysisResult));

        VisionAnalysisResult? received = null;
        sensor.AnalysisResults.Subscribe(r => received = r);

        sensor.StartObserving();
        await sensor.ProcessFrameAsync(new byte[] { 1 }, 640, 480);

        received.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessFrameAsync_NthFrame_PublishesVisualPerceptionToVirtualSelf()
    {
        var config = new VisualSensorConfig(ProcessEveryNthFrame: 1);
        var sensor = new VisualSensor(_mockVisionModel.Object, _virtualSelf, config);

        var face = new DetectedFace("face1", 0.9, (10, 20, 30, 40), "happy", 25, null);
        var analysisResult = new VisionAnalysisResult(
            "a person", new List<DetectedObject>(), new List<DetectedFace> { face },
            "indoor", null, null, 0.9, 100);
        _mockVisionModel.Setup(m => m.AnalyzeImageAsync(
                It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<VisionAnalysisOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<VisionAnalysisResult, string>.Success(analysisResult));

        PerceptionEvent? received = null;
        _virtualSelf.Perceptions.Subscribe(p => received = p);

        sensor.StartObserving();
        await sensor.ProcessFrameAsync(new byte[] { 1 }, 640, 480);

        received.Should().NotBeNull();
        received.Should().BeOfType<VisualPerception>();
    }

    [Fact]
    public async Task ProcessFrameAsync_AnalysisFails_ReturnsFailure()
    {
        var config = new VisualSensorConfig(ProcessEveryNthFrame: 1);
        var sensor = new VisualSensor(_mockVisionModel.Object, _virtualSelf, config);

        _mockVisionModel.Setup(m => m.AnalyzeImageAsync(
                It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<VisionAnalysisOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<VisionAnalysisResult, string>.Failure("analysis failed"));

        sensor.StartObserving();
        var result = await sensor.ProcessFrameAsync(new byte[] { 1 }, 640, 480);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("analysis failed");
    }

    // ========================================================================
    // AnalyzeImageAsync
    // ========================================================================

    [Fact]
    public async Task AnalyzeImageAsync_WhenDisposed_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.AnalyzeImageAsync("test.jpg");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public async Task AnalyzeImageAsync_Success_EmitsAnalysisResult()
    {
        var analysisResult = new VisionAnalysisResult(
            "a cat", new List<DetectedObject>(), new List<DetectedFace>(),
            "indoor", null, null, 0.95, 150);
        _mockVisionModel.Setup(m => m.AnalyzeImageFileAsync(
                It.IsAny<string>(), It.IsAny<VisionAnalysisOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<VisionAnalysisResult, string>.Success(analysisResult));

        VisionAnalysisResult? received = null;
        _sut.AnalysisResults.Subscribe(r => received = r);

        var result = await _sut.AnalyzeImageAsync("cat.jpg");

        result.IsSuccess.Should().BeTrue();
        received.Should().NotBeNull();
        received!.Description.Should().Be("a cat");
    }

    [Fact]
    public async Task AnalyzeImageAsync_Success_PublishesToVirtualSelf()
    {
        var analysisResult = new VisionAnalysisResult(
            "a dog", new List<DetectedObject>(), new List<DetectedFace>(),
            "outdoor", null, null, 0.9, 100);
        _mockVisionModel.Setup(m => m.AnalyzeImageFileAsync(
                It.IsAny<string>(), It.IsAny<VisionAnalysisOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<VisionAnalysisResult, string>.Success(analysisResult));

        PerceptionEvent? received = null;
        _virtualSelf.Perceptions.Subscribe(p => received = p);

        await _sut.AnalyzeImageAsync("dog.jpg");

        received.Should().NotBeNull();
        received.Should().BeOfType<VisualPerception>();
    }

    [Fact]
    public async Task AnalyzeImageAsync_Failure_DoesNotEmitOrPublish()
    {
        _mockVisionModel.Setup(m => m.AnalyzeImageFileAsync(
                It.IsAny<string>(), It.IsAny<VisionAnalysisOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<VisionAnalysisResult, string>.Failure("file not found"));

        VisionAnalysisResult? received = null;
        _sut.AnalysisResults.Subscribe(r => received = r);

        var result = await _sut.AnalyzeImageAsync("missing.jpg");

        result.IsSuccess.Should().BeFalse();
        received.Should().BeNull();
    }

    // ========================================================================
    // AskAboutImageAsync
    // ========================================================================

    [Fact]
    public async Task AskAboutImageAsync_WhenDisposed_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.AskAboutImageAsync(new byte[] { 1 }, "jpg", "What is this?");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public async Task AskAboutImageAsync_DelegatesToVisionModel()
    {
        _mockVisionModel.Setup(m => m.AnswerQuestionAsync(
                It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("It's a cat"));

        var result = await _sut.AskAboutImageAsync(new byte[] { 1 }, "jpg", "What is this?");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("It's a cat");
    }

    // ========================================================================
    // FocusOn
    // ========================================================================

    [Fact]
    public void FocusOn_SetsVisualAttentionOnVirtualSelf()
    {
        _sut.FocusOn("person in frame");

        _virtualSelf.CurrentState.AttentionFocus.Should().NotBeNull();
        _virtualSelf.CurrentState.AttentionFocus!.Target.Should().Be("person in frame");
        _virtualSelf.CurrentState.AttentionFocus.Modality.Should().Be(SensorModality.Visual);
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
        bool framesCompleted = false;
        bool analysisCompleted = false;

        _sut.Frames.Subscribe(_ => { }, () => framesCompleted = true);
        _sut.AnalysisResults.Subscribe(_ => { }, () => analysisCompleted = true);

        _sut.Dispose();

        framesCompleted.Should().BeTrue();
        analysisCompleted.Should().BeTrue();
    }
}
