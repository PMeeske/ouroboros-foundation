using System.Reactive.Linq;
using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.EmbodiedInteraction;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class VisualSensorTests : IDisposable
{
    private readonly Mock<IVisionModel> _mockVisionModel;
    private readonly VirtualSelf _virtualSelf;
    private readonly VisualSensor _sut;

    public VisualSensorTests()
    {
        _mockVisionModel = new Mock<IVisionModel>();
        _mockVisionModel.Setup(m => m.ModelName).Returns("test-vision");

        _virtualSelf = new VirtualSelf("TestSelf", fusionWindowMs: 60000);
        _sut = new VisualSensor(_mockVisionModel.Object, _virtualSelf);
    }

    public void Dispose()
    {
        _sut.Dispose();
        _virtualSelf.Dispose();
    }

    [Fact]
    public void Constructor_NullVisionModel_ThrowsArgumentNullException()
    {
        Action act = () => new VisualSensor(null!, _virtualSelf);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullVirtualSelf_ThrowsArgumentNullException()
    {
        Action act = () => new VisualSensor(_mockVisionModel.Object, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ModelName_ReturnsModelName()
    {
        _sut.ModelName.Should().Be("test-vision");
    }

    [Fact]
    public void IsObserving_Initially_IsFalse()
    {
        _sut.IsObserving.Should().BeFalse();
    }

    [Fact]
    public void FrameCount_Initially_IsZero()
    {
        _sut.FrameCount.Should().Be(0);
    }

    [Fact]
    public void StartObserving_WhenNotObserving_ReturnsSuccess()
    {
        var result = _sut.StartObserving();

        result.IsSuccess.Should().BeTrue();
        _sut.IsObserving.Should().BeTrue();
    }

    [Fact]
    public void StartObserving_WhenAlreadyObserving_ReturnsFailure()
    {
        _sut.StartObserving();

        var result = _sut.StartObserving();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Already observing");
    }

    [Fact]
    public void StartObserving_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();

        var result = _sut.StartObserving();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public void StopObserving_WhenNotObserving_ReturnsSuccess()
    {
        var result = _sut.StopObserving();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void StopObserving_WhenObserving_ReturnsSuccess()
    {
        _sut.StartObserving();

        var result = _sut.StopObserving();

        result.IsSuccess.Should().BeTrue();
        _sut.IsObserving.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessFrameAsync_WhenNotObserving_ReturnsFailure()
    {
        var result = await _sut.ProcessFrameAsync(new byte[] { 1 }, 640, 480);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Not observing");
    }

    [Fact]
    public async Task ProcessFrameAsync_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.ProcessFrameAsync(new byte[] { 1 }, 640, 480);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessFrameAsync_NonAnalyzedFrame_ReturnsNone()
    {
        // Default config processes every 5th frame, so first frame should be skipped
        _sut.StartObserving();

        var result = await _sut.ProcessFrameAsync(new byte[] { 1 }, 640, 480);

        result.IsSuccess.Should().BeTrue();
        result.Value.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessFrameAsync_IncrementsFrameCount()
    {
        _sut.StartObserving();

        await _sut.ProcessFrameAsync(new byte[] { 1 }, 640, 480);

        _sut.FrameCount.Should().Be(1);
    }

    [Fact]
    public async Task ProcessFrameAsync_EveryNthFrame_AnalyzesFrame()
    {
        var config = new VisualSensorConfig(ProcessEveryNthFrame: 1);
        using var sensor = new VisualSensor(_mockVisionModel.Object, _virtualSelf, config);
        var analysisResult = new VisionAnalysisResult(
            "test scene", new List<DetectedObject>(), new List<DetectedFace>(),
            "indoor", null, null, 0.9, 50);
        _mockVisionModel
            .Setup(m => m.AnalyzeImageAsync(It.IsAny<byte[]>(), It.IsAny<string>(),
                It.IsAny<VisionAnalysisOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<VisionAnalysisResult, string>.Success(analysisResult));

        sensor.StartObserving();
        var result = await sensor.ProcessFrameAsync(new byte[] { 1 }, 640, 480);

        result.IsSuccess.Should().BeTrue();
        result.Value.HasValue.Should().BeTrue();
        result.Value.Value.Description.Should().Be("test scene");
    }

    [Fact]
    public async Task AnalyzeImageAsync_Success_ReturnsResult()
    {
        var analysisResult = new VisionAnalysisResult(
            "a cat", new List<DetectedObject>(), new List<DetectedFace>(),
            "indoor", null, null, 0.95, 100);
        _mockVisionModel
            .Setup(m => m.AnalyzeImageFileAsync(It.IsAny<string>(),
                It.IsAny<VisionAnalysisOptions?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<VisionAnalysisResult, string>.Success(analysisResult));

        var result = await _sut.AnalyzeImageAsync("test.jpg");

        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().Be("a cat");
    }

    [Fact]
    public async Task AnalyzeImageAsync_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.AnalyzeImageAsync("test.jpg");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task AskAboutImageAsync_DelegatesToModel()
    {
        _mockVisionModel
            .Setup(m => m.AnswerQuestionAsync(It.IsAny<byte[]>(), "png", "What is this?",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("It's a cat"));

        var result = await _sut.AskAboutImageAsync(new byte[] { 1 }, "png", "What is this?");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("It's a cat");
    }

    [Fact]
    public async Task AskAboutImageAsync_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.AskAboutImageAsync(new byte[] { 1 }, "png", "What?");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void FocusOn_SetsAttentionOnVirtualSelf()
    {
        _sut.FocusOn("user-face");

        _virtualSelf.CurrentState.AttentionFocus.Should().NotBeNull();
        _virtualSelf.CurrentState.AttentionFocus!.Target.Should().Be("user-face");
        _virtualSelf.CurrentState.AttentionFocus.Modality.Should().Be(SensorModality.Visual);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        _sut.Dispose();

        var act = () => _sut.Dispose();

        act.Should().NotThrow();
    }
}
