using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.EmbodiedInteraction;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class EmbodimentControllerTests : IDisposable
{
    private readonly Mock<VirtualSelf> _mockVirtualSelf;
    private readonly BodySchema _bodySchema;
    private readonly EmbodimentController _sut;

    public EmbodimentControllerTests()
    {
        _bodySchema = new BodySchema();
        // VirtualSelf requires a BodySchema - create a real one if it doesn't require mocking
        _mockVirtualSelf = new Mock<VirtualSelf>(_bodySchema) { CallBase = false };
        _sut = new EmbodimentController(_mockVirtualSelf.Object, _bodySchema);
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    [Fact]
    public void Constructor_NullVirtualSelf_ThrowsArgumentNullException()
    {
        Action act = () => new EmbodimentController(null!, _bodySchema);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullBodySchema_ThrowsArgumentNullException()
    {
        Action act = () => new EmbodimentController(_mockVirtualSelf.Object, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void VirtualSelf_ReturnsProvidedInstance()
    {
        _sut.VirtualSelf.Should().BeSameAs(_mockVirtualSelf.Object);
    }

    [Fact]
    public void BodySchema_ReturnsProvidedInstance()
    {
        _sut.BodySchema.Should().BeSameAs(_bodySchema);
    }

    [Fact]
    public void IsRunning_InitiallyFalse()
    {
        _sut.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void Perceptions_IsObservable()
    {
        _sut.Perceptions.Should().NotBeNull();
    }

    [Fact]
    public void ActionResults_IsObservable()
    {
        _sut.ActionResults.Should().NotBeNull();
    }

    [Fact]
    public async Task SpeakAsync_NoVoiceActuator_ReturnsFailure()
    {
        var result = await _sut.SpeakAsync("Hello");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No voice actuator");
    }

    [Fact]
    public async Task GetFusedPerceptionAsync_NotRunning_ReturnsFailure()
    {
        var result = await _sut.GetFusedPerceptionAsync(TimeSpan.FromSeconds(1));

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not running");
    }

    [Fact]
    public async Task ExecuteActionAsync_Disposed_ReturnsFailure()
    {
        _sut.Dispose();
        var request = new ActionRequest(
            ActuatorModality.Text,
            "test",
            "Hello",
            null);

        var result = await _sut.ExecuteActionAsync(request);

        result.Success.Should().BeFalse();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public async Task ExecuteActionAsync_TextModality_ReturnsSuccess()
    {
        var request = new ActionRequest(
            ActuatorModality.Text,
            "test",
            "Hello",
            null);

        var result = await _sut.ExecuteActionAsync(request);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteActionAsync_UnsupportedModality_ReturnsFailure()
    {
        var request = new ActionRequest(
            (ActuatorModality)99,
            "test",
            "Hello",
            null);

        var result = await _sut.ExecuteActionAsync(request);

        result.Success.Should().BeFalse();
    }

    [Fact]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        _sut.Dispose();

        Action act = () => _sut.Dispose();

        act.Should().NotThrow();
    }
}
