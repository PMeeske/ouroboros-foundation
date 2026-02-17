// <copyright file="VirtualSelf.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Core.EmbodiedInteraction;

using System.Reactive.Linq;
using System.Reactive.Subjects;
using Ouroboros.Core.Monads;

/// <summary>
/// The Virtual Self - an embodied agent with multimodal perception and action.
/// Integrates microphone (STT), camera (vision), and voice (TTS) capabilities.
/// </summary>
public sealed class VirtualSelf : IDisposable
{
    private readonly BehaviorSubject<VirtualSelfState> _stateSubject;
    private readonly Subject<PerceptionEvent> _perceptionSubject = new();
    private readonly Subject<FusedPerception> _fusedPerceptionSubject = new();
    private readonly List<PerceptionEvent> _perceptionBuffer = new();
    private readonly TimeSpan _fusionWindow;
    private readonly object _lock = new();
    private IDisposable? _fusionSubscription;
    private bool _disposed;

    /// <summary>
    /// Initializes a new Virtual Self.
    /// </summary>
    /// <param name="name">Persona name.</param>
    /// <param name="fusionWindowMs">Time window for fusing perceptions (ms).</param>
    public VirtualSelf(string name = "Ouroboros", int fusionWindowMs = 500)
    {
        _stateSubject = new BehaviorSubject<VirtualSelfState>(VirtualSelfState.CreateDefault(name));
        _fusionWindow = TimeSpan.FromMilliseconds(fusionWindowMs);

        // Set up perception fusion pipeline
        SetupFusionPipeline();
    }

    /// <summary>
    /// Gets the current state.
    /// </summary>
    public VirtualSelfState CurrentState => _stateSubject.Value;

    /// <summary>
    /// Observable stream of state changes.
    /// </summary>
    public IObservable<VirtualSelfState> State => _stateSubject.AsObservable();

    /// <summary>
    /// Observable stream of raw perceptions.
    /// </summary>
    public IObservable<PerceptionEvent> Perceptions => _perceptionSubject.AsObservable();

    /// <summary>
    /// Observable stream of fused multimodal perceptions.
    /// </summary>
    public IObservable<FusedPerception> FusedPerceptions => _fusedPerceptionSubject.AsObservable();

    /// <summary>
    /// Activates a sensor modality.
    /// </summary>
    public Result<VirtualSelfState, string> ActivateSensor(SensorModality modality)
    {
        if (_disposed) return Result<VirtualSelfState, string>.Failure("Virtual self is disposed");

        var newState = _stateSubject.Value.WithSensorActive(modality);
        _stateSubject.OnNext(newState);
        return Result<VirtualSelfState, string>.Success(newState);
    }

    /// <summary>
    /// Deactivates a sensor modality.
    /// </summary>
    public Result<VirtualSelfState, string> DeactivateSensor(SensorModality modality)
    {
        if (_disposed) return Result<VirtualSelfState, string>.Failure("Virtual self is disposed");

        var newState = _stateSubject.Value.WithSensorInactive(modality);
        _stateSubject.OnNext(newState);
        return Result<VirtualSelfState, string>.Success(newState);
    }

    /// <summary>
    /// Sets the embodiment state.
    /// </summary>
    public void SetState(EmbodimentState state)
    {
        if (_disposed) return;
        _stateSubject.OnNext(_stateSubject.Value.WithState(state));
    }

    /// <summary>
    /// Focuses attention on a target.
    /// </summary>
    public void FocusAttention(SensorModality modality, string target, double intensity = 1.0)
    {
        if (_disposed) return;
        var focus = new AttentionFocus(modality, target, intensity, DateTime.UtcNow);
        _stateSubject.OnNext(_stateSubject.Value.WithAttention(focus));
    }

    /// <summary>
    /// Publishes an audio perception (from STT).
    /// </summary>
    public void PublishAudioPerception(
        string transcribedText,
        double confidence = 1.0,
        string? language = null,
        TimeSpan? duration = null,
        bool isFinal = true)
    {
        if (_disposed) return;

        var perception = new AudioPerception(
            Guid.NewGuid(),
            DateTime.UtcNow,
            confidence,
            transcribedText,
            language,
            null,
            duration ?? TimeSpan.Zero,
            isFinal);

        PublishPerception(perception);
    }

    /// <summary>
    /// Publishes a visual perception (from vision model).
    /// </summary>
    public void PublishVisualPerception(
        string description,
        IReadOnlyList<DetectedObject>? objects = null,
        IReadOnlyList<DetectedFace>? faces = null,
        string? sceneType = null,
        string? emotion = null,
        double confidence = 1.0,
        byte[]? rawFrame = null)
    {
        if (_disposed) return;

        var perception = new VisualPerception(
            Guid.NewGuid(),
            DateTime.UtcNow,
            confidence,
            description,
            objects ?? Array.Empty<DetectedObject>(),
            faces ?? Array.Empty<DetectedFace>(),
            sceneType,
            emotion,
            rawFrame);

        PublishPerception(perception);
    }

    /// <summary>
    /// Publishes a text perception.
    /// </summary>
    public void PublishTextPerception(string text, string? source = null, double confidence = 1.0)
    {
        if (_disposed) return;

        var perception = new TextPerception(
            Guid.NewGuid(),
            DateTime.UtcNow,
            confidence,
            text,
            source);

        PublishPerception(perception);
    }

    private void PublishPerception(PerceptionEvent perception)
    {
        if (_disposed) return;

        lock (_lock)
        {
            _perceptionBuffer.Add(perception);
        }

        _perceptionSubject.OnNext(perception);
    }

    private void SetupFusionPipeline()
    {
        // Buffer and fuse perceptions at regular intervals
        _fusionSubscription = Observable.Interval(_fusionWindow)
            .Subscribe(_ => FusePerceptions());
    }

    private void FusePerceptions()
    {
        if (_disposed) return;

        List<PerceptionEvent> toFuse;
        lock (_lock)
        {
            if (_perceptionBuffer.Count == 0) return;

            var cutoff = DateTime.UtcNow - _fusionWindow;
            toFuse = _perceptionBuffer.Where(p => p.Timestamp >= cutoff).ToList();
            _perceptionBuffer.Clear();
        }

        if (toFuse.Count == 0) return;

        var audioPerceptions = toFuse.OfType<AudioPerception>().ToList();
        var visualPerceptions = toFuse.OfType<VisualPerception>().ToList();
        var textPerceptions = toFuse.OfType<TextPerception>().ToList();

        // Build integrated understanding
        var understanding = BuildIntegratedUnderstanding(audioPerceptions, visualPerceptions, textPerceptions);
        var avgConfidence = toFuse.Average(p => p.Confidence);

        var fused = new FusedPerception(
            Guid.NewGuid(),
            DateTime.UtcNow,
            audioPerceptions,
            visualPerceptions,
            textPerceptions,
            understanding,
            avgConfidence);

        // Guard against race condition where Dispose may run between the
        // initial _disposed check and this point, disposing the subject.
        if (!_disposed)
        {
            try
            {
                _fusedPerceptionSubject.OnNext(fused);
            }
            catch (ObjectDisposedException)
            {
                // Subject was disposed between the check and the call - safe to ignore.
            }
        }
    }

    private static string BuildIntegratedUnderstanding(
        IReadOnlyList<AudioPerception> audio,
        IReadOnlyList<VisualPerception> visual,
        IReadOnlyList<TextPerception> text)
    {
        var parts = new List<string>();

        if (audio.Count > 0)
        {
            var transcript = string.Join(" ", audio.Where(a => a.IsFinal).Select(a => a.TranscribedText));
            if (!string.IsNullOrWhiteSpace(transcript))
            {
                parts.Add($"[Heard] {transcript}");
            }
        }

        if (visual.Count > 0)
        {
            var desc = visual.Last().Description;
            if (!string.IsNullOrWhiteSpace(desc))
            {
                parts.Add($"[Saw] {desc}");
            }

            var objects = visual.SelectMany(v => v.Objects).Select(o => o.Label).Distinct().ToList();
            if (objects.Count > 0)
            {
                parts.Add($"[Objects] {string.Join(", ", objects)}");
            }

            var faces = visual.SelectMany(v => v.Faces).ToList();
            if (faces.Count > 0)
            {
                parts.Add($"[Faces] {faces.Count} detected");
            }
        }

        if (text.Count > 0)
        {
            var combined = string.Join(" ", text.Select(t => t.Text));
            parts.Add($"[Read] {combined}");
        }

        return parts.Count > 0 ? string.Join(" | ", parts) : "[No perceptions]";
    }

    /// <summary>
    /// Disposes the virtual self.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _fusionSubscription?.Dispose();

        _perceptionSubject.OnCompleted();
        _fusedPerceptionSubject.OnCompleted();
        _stateSubject.OnCompleted();

        _perceptionSubject.Dispose();
        _fusedPerceptionSubject.Dispose();
        _stateSubject.Dispose();
    }
}
