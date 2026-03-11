using System.Reactive.Linq;
using Ouroboros.Core.EmbodiedInteraction;
using Ouroboros.Core.Monads;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class VirtualSelfAdditionalTests : IDisposable
{
    private readonly VirtualSelf _sut;

    public VirtualSelfAdditionalTests()
    {
        _sut = new VirtualSelf("TestAgent", fusionWindowMs: 60000);
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    // ========================================================================
    // Constructor and properties
    // ========================================================================

    [Fact]
    public void Constructor_DefaultName_IsOuroboros()
    {
        using var vs = new VirtualSelf();
        vs.CurrentState.Name.Should().Be("Ouroboros");
    }

    [Fact]
    public void Constructor_CustomName_SetsName()
    {
        _sut.CurrentState.Name.Should().Be("TestAgent");
    }

    [Fact]
    public void CurrentState_InitiallyDormant()
    {
        _sut.CurrentState.State.Should().Be(EmbodimentState.Dormant);
    }

    [Fact]
    public void State_IsObservable()
    {
        _sut.State.Should().NotBeNull();
    }

    [Fact]
    public void Perceptions_IsObservable()
    {
        _sut.Perceptions.Should().NotBeNull();
    }

    [Fact]
    public void FusedPerceptions_IsObservable()
    {
        _sut.FusedPerceptions.Should().NotBeNull();
    }

    // ========================================================================
    // ActivateSensor / DeactivateSensor
    // ========================================================================

    [Fact]
    public void ActivateSensor_AddsToActiveSensors()
    {
        var result = _sut.ActivateSensor(SensorModality.Audio);

        result.IsSuccess.Should().BeTrue();
        result.Value.ActiveSensors.Should().Contain(SensorModality.Audio);
    }

    [Fact]
    public void DeactivateSensor_RemovesFromActiveSensors()
    {
        _sut.ActivateSensor(SensorModality.Audio);

        var result = _sut.DeactivateSensor(SensorModality.Audio);

        result.IsSuccess.Should().BeTrue();
        result.Value.ActiveSensors.Should().NotContain(SensorModality.Audio);
    }

    [Fact]
    public void ActivateSensor_WhenDisposed_ReturnsFailure()
    {
        _sut.Dispose();

        var result = _sut.ActivateSensor(SensorModality.Audio);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public void DeactivateSensor_WhenDisposed_ReturnsFailure()
    {
        _sut.Dispose();

        var result = _sut.DeactivateSensor(SensorModality.Audio);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disposed");
    }

    // ========================================================================
    // SetState
    // ========================================================================

    [Fact]
    public void SetState_UpdatesCurrentState()
    {
        _sut.SetState(EmbodimentState.Listening);

        _sut.CurrentState.State.Should().Be(EmbodimentState.Listening);
    }

    [Fact]
    public void SetState_WhenDisposed_DoesNotThrow()
    {
        _sut.Dispose();

        var act = () => _sut.SetState(EmbodimentState.Awake);

        act.Should().NotThrow();
    }

    [Fact]
    public void SetState_PublishesStateChange()
    {
        VirtualSelfState? received = null;
        _sut.State.Subscribe(s => received = s);

        _sut.SetState(EmbodimentState.Speaking);

        received.Should().NotBeNull();
        received!.State.Should().Be(EmbodimentState.Speaking);
    }

    // ========================================================================
    // FocusAttention
    // ========================================================================

    [Fact]
    public void FocusAttention_SetsAttentionFocusInState()
    {
        _sut.FocusAttention(SensorModality.Visual, "person", 0.9);

        _sut.CurrentState.AttentionFocus.Should().NotBeNull();
        _sut.CurrentState.AttentionFocus!.Target.Should().Be("person");
        _sut.CurrentState.AttentionFocus.Intensity.Should().Be(0.9);
        _sut.CurrentState.AttentionFocus.Modality.Should().Be(SensorModality.Visual);
    }

    [Fact]
    public void FocusAttention_WhenDisposed_DoesNotThrow()
    {
        _sut.Dispose();

        var act = () => _sut.FocusAttention(SensorModality.Audio, "speaker");

        act.Should().NotThrow();
    }

    // ========================================================================
    // PublishAudioPerception
    // ========================================================================

    [Fact]
    public void PublishAudioPerception_EmitsPerceptionEvent()
    {
        PerceptionEvent? received = null;
        _sut.Perceptions.Subscribe(p => received = p);

        _sut.PublishAudioPerception("Hello world", 0.95, "en", TimeSpan.FromSeconds(1), true);

        received.Should().NotBeNull();
        received.Should().BeOfType<AudioPerception>();
        var audio = (AudioPerception)received!;
        audio.TranscribedText.Should().Be("Hello world");
        audio.Confidence.Should().Be(0.95);
        audio.DetectedLanguage.Should().Be("en");
        audio.IsFinal.Should().BeTrue();
    }

    [Fact]
    public void PublishAudioPerception_DefaultValues_Work()
    {
        PerceptionEvent? received = null;
        _sut.Perceptions.Subscribe(p => received = p);

        _sut.PublishAudioPerception("test");

        received.Should().NotBeNull();
        var audio = (AudioPerception)received!;
        audio.Confidence.Should().Be(1.0);
        audio.Duration.Should().Be(TimeSpan.Zero);
        audio.IsFinal.Should().BeTrue();
    }

    [Fact]
    public void PublishAudioPerception_WhenDisposed_DoesNotThrow()
    {
        _sut.Dispose();

        var act = () => _sut.PublishAudioPerception("test");

        act.Should().NotThrow();
    }

    // ========================================================================
    // PublishVisualPerception
    // ========================================================================

    [Fact]
    public void PublishVisualPerception_EmitsPerceptionEvent()
    {
        PerceptionEvent? received = null;
        _sut.Perceptions.Subscribe(p => received = p);

        var objects = new List<DetectedObject>
        {
            new("cat", 0.9, (10, 20, 30, 40), null)
        };
        var faces = new List<DetectedFace>
        {
            new("face1", 0.95, (50, 60, 70, 80), "happy", 25, null)
        };

        _sut.PublishVisualPerception("a room with a cat", objects, faces, "indoor", "happy", 0.88, new byte[] { 1 });

        received.Should().NotBeNull();
        received.Should().BeOfType<VisualPerception>();
        var visual = (VisualPerception)received!;
        visual.Description.Should().Be("a room with a cat");
        visual.Objects.Should().HaveCount(1);
        visual.Faces.Should().HaveCount(1);
        visual.SceneType.Should().Be("indoor");
        visual.DominantEmotion.Should().Be("happy");
    }

    [Fact]
    public void PublishVisualPerception_NullOptionals_UsesDefaults()
    {
        PerceptionEvent? received = null;
        _sut.Perceptions.Subscribe(p => received = p);

        _sut.PublishVisualPerception("empty scene");

        received.Should().NotBeNull();
        var visual = (VisualPerception)received!;
        visual.Objects.Should().BeEmpty();
        visual.Faces.Should().BeEmpty();
    }

    [Fact]
    public void PublishVisualPerception_WhenDisposed_DoesNotThrow()
    {
        _sut.Dispose();

        var act = () => _sut.PublishVisualPerception("test");

        act.Should().NotThrow();
    }

    // ========================================================================
    // PublishTextPerception
    // ========================================================================

    [Fact]
    public void PublishTextPerception_EmitsPerceptionEvent()
    {
        PerceptionEvent? received = null;
        _sut.Perceptions.Subscribe(p => received = p);

        _sut.PublishTextPerception("Hello", "user", 0.99);

        received.Should().NotBeNull();
        received.Should().BeOfType<TextPerception>();
        var text = (TextPerception)received!;
        text.Text.Should().Be("Hello");
        text.Source.Should().Be("user");
        text.Confidence.Should().Be(0.99);
    }

    [Fact]
    public void PublishTextPerception_WhenDisposed_DoesNotThrow()
    {
        _sut.Dispose();

        var act = () => _sut.PublishTextPerception("test");

        act.Should().NotThrow();
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
    public void Dispose_CompletesAllSubjects()
    {
        bool perceptionCompleted = false;
        bool fusedCompleted = false;
        bool stateCompleted = false;

        _sut.Perceptions.Subscribe(_ => { }, () => perceptionCompleted = true);
        _sut.FusedPerceptions.Subscribe(_ => { }, () => fusedCompleted = true);
        _sut.State.Subscribe(_ => { }, () => stateCompleted = true);

        _sut.Dispose();

        perceptionCompleted.Should().BeTrue();
        fusedCompleted.Should().BeTrue();
        stateCompleted.Should().BeTrue();
    }
}
