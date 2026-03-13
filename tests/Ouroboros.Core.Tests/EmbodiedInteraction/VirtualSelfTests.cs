using System.Reactive.Linq;
using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.EmbodiedInteraction;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class VirtualSelfTests : IDisposable
{
    private readonly VirtualSelf _sut;

    public VirtualSelfTests()
    {
        _sut = new VirtualSelf("TestSelf", fusionWindowMs: 5000);
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    [Fact]
    public void Constructor_DefaultName_SetsOuroboros()
    {
        using var self = new VirtualSelf();
        self.CurrentState.Name.Should().Be("Ouroboros");
    }

    [Fact]
    public void Constructor_CustomName_SetsName()
    {
        _sut.CurrentState.Name.Should().Be("TestSelf");
    }

    [Fact]
    public void Constructor_InitialState_IsDormant()
    {
        _sut.CurrentState.State.Should().Be(EmbodimentState.Dormant);
    }

    [Fact]
    public void Constructor_InitialState_HasNoActiveSensors()
    {
        _sut.CurrentState.ActiveSensors.Should().BeEmpty();
    }

    [Fact]
    public void ActivateSensor_ValidModality_ReturnsSuccess()
    {
        var result = _sut.ActivateSensor(SensorModality.Audio);

        result.IsSuccess.Should().BeTrue();
        result.Value.ActiveSensors.Should().Contain(SensorModality.Audio);
    }

    [Fact]
    public void ActivateSensor_ValidModality_UpdatesCurrentState()
    {
        _sut.ActivateSensor(SensorModality.Audio);

        _sut.CurrentState.ActiveSensors.Should().Contain(SensorModality.Audio);
    }

    [Fact]
    public void ActivateSensor_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();

        var result = _sut.ActivateSensor(SensorModality.Audio);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public void DeactivateSensor_ActiveSensor_ReturnsSuccess()
    {
        _sut.ActivateSensor(SensorModality.Audio);

        var result = _sut.DeactivateSensor(SensorModality.Audio);

        result.IsSuccess.Should().BeTrue();
        result.Value.ActiveSensors.Should().NotContain(SensorModality.Audio);
    }

    [Fact]
    public void DeactivateSensor_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();

        var result = _sut.DeactivateSensor(SensorModality.Audio);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void SetState_ChangesCurrentState()
    {
        _sut.SetState(EmbodimentState.Listening);

        _sut.CurrentState.State.Should().Be(EmbodimentState.Listening);
    }

    [Fact]
    public void SetState_AfterDispose_DoesNotThrow()
    {
        _sut.Dispose();

        var act = () => _sut.SetState(EmbodimentState.Listening);

        act.Should().NotThrow();
    }

    [Fact]
    public void FocusAttention_SetsAttentionFocus()
    {
        _sut.FocusAttention(SensorModality.Visual, "user-face", 0.8);

        _sut.CurrentState.AttentionFocus.Should().NotBeNull();
        _sut.CurrentState.AttentionFocus!.Target.Should().Be("user-face");
        _sut.CurrentState.AttentionFocus.Modality.Should().Be(SensorModality.Visual);
        _sut.CurrentState.AttentionFocus.Intensity.Should().Be(0.8);
    }

    [Fact]
    public void FocusAttention_AfterDispose_DoesNotThrow()
    {
        _sut.Dispose();

        var act = () => _sut.FocusAttention(SensorModality.Visual, "target");

        act.Should().NotThrow();
    }

    [Fact]
    public void PublishAudioPerception_EmitsPerceptionEvent()
    {
        PerceptionEvent? received = null;
        _sut.Perceptions.Subscribe(p => received = p);

        _sut.PublishAudioPerception("hello", 0.95, "en", TimeSpan.FromSeconds(1), true);

        received.Should().NotBeNull();
        received.Should().BeOfType<AudioPerception>();
        ((AudioPerception)received!).TranscribedText.Should().Be("hello");
    }

    [Fact]
    public void PublishVisualPerception_EmitsPerceptionEvent()
    {
        PerceptionEvent? received = null;
        _sut.Perceptions.Subscribe(p => received = p);

        _sut.PublishVisualPerception("a person standing", confidence: 0.9);

        received.Should().NotBeNull();
        received.Should().BeOfType<VisualPerception>();
        ((VisualPerception)received!).Description.Should().Be("a person standing");
    }

    [Fact]
    public void PublishTextPerception_EmitsPerceptionEvent()
    {
        PerceptionEvent? received = null;
        _sut.Perceptions.Subscribe(p => received = p);

        _sut.PublishTextPerception("some text", "chat");

        received.Should().NotBeNull();
        received.Should().BeOfType<TextPerception>();
        ((TextPerception)received!).Text.Should().Be("some text");
    }

    [Fact]
    public void PublishAudioPerception_AfterDispose_DoesNotThrow()
    {
        _sut.Dispose();

        var act = () => _sut.PublishAudioPerception("hello");

        act.Should().NotThrow();
    }

    [Fact]
    public void PublishVisualPerception_AfterDispose_DoesNotThrow()
    {
        _sut.Dispose();

        var act = () => _sut.PublishVisualPerception("desc");

        act.Should().NotThrow();
    }

    [Fact]
    public void PublishTextPerception_AfterDispose_DoesNotThrow()
    {
        _sut.Dispose();

        var act = () => _sut.PublishTextPerception("text");

        act.Should().NotThrow();
    }

    [Fact]
    public void State_Observable_EmitsOnChange()
    {
        var states = new List<VirtualSelfState>();
        _sut.State.Subscribe(s => states.Add(s));

        _sut.SetState(EmbodimentState.Listening);
        _sut.SetState(EmbodimentState.Observing);

        // BehaviorSubject emits initial + 2 changes
        states.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        _sut.Dispose();

        var act = () => _sut.Dispose();

        act.Should().NotThrow();
    }
}
