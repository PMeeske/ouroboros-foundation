// <copyright file="AudioSensor.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;

namespace Ouroboros.Core.EmbodiedInteraction;

using System.Reactive.Linq;
using System.Reactive.Subjects;
using Ouroboros.Core.Monads;

/// <summary>
/// Audio sensor that captures microphone input and performs STT.
/// </summary>
public sealed class AudioSensor : IDisposable
{
    private readonly AudioSensorConfig _config;
    private readonly ISttModel _sttModel;
    private readonly VirtualSelf _virtualSelf;
    private readonly Subject<AudioChunk> _audioChunks = new();
    private readonly Subject<TranscriptionResult> _transcriptions = new();
    private readonly Subject<VoiceActivity> _voiceActivity = new();
    private IStreamingTranscription? _streamingSession;
    private bool _isListening;
    private bool _disposed;

    /// <summary>
    /// Initializes a new audio sensor.
    /// </summary>
    /// <param name="sttModel">The STT model for transcription.</param>
    /// <param name="virtualSelf">The virtual self to publish perceptions to.</param>
    /// <param name="config">Sensor configuration.</param>
    public AudioSensor(
        ISttModel sttModel,
        VirtualSelf virtualSelf,
        AudioSensorConfig? config = null)
    {
        _sttModel = sttModel ?? throw new ArgumentNullException(nameof(sttModel));
        _virtualSelf = virtualSelf ?? throw new ArgumentNullException(nameof(virtualSelf));
        _config = config ?? new AudioSensorConfig();
    }

    /// <summary>
    /// Gets the STT model name.
    /// </summary>
    public string ModelName => _sttModel.ModelName;

    /// <summary>
    /// Gets whether currently listening.
    /// </summary>
    public bool IsListening => _isListening;

    /// <summary>
    /// Observable stream of audio chunks.
    /// </summary>
    public IObservable<AudioChunk> AudioChunks => _audioChunks.AsObservable();

    /// <summary>
    /// Observable stream of transcriptions.
    /// </summary>
    public IObservable<TranscriptionResult> Transcriptions => _transcriptions.AsObservable();

    /// <summary>
    /// Observable stream of voice activity.
    /// </summary>
    public IObservable<VoiceActivity> VoiceActivityEvents => _voiceActivity.AsObservable();

    /// <summary>
    /// Starts listening for audio input.
    /// </summary>
    public async Task<Result<Unit, string>> StartListeningAsync(CancellationToken ct = default)
    {
        if (_disposed) return Result<Unit, string>.Failure("Sensor is disposed");
        if (_isListening) return Result<Unit, string>.Failure("Already listening");

        try
        {
            if (_sttModel.SupportsStreaming)
            {
                _streamingSession = _sttModel.CreateStreamingSession(_config.Language);

                // Wire up streaming results to our subjects
                _streamingSession.Results.Subscribe(result =>
                {
                    _transcriptions.OnNext(result);

                    if (result.IsFinal)
                    {
                        // Publish to virtual self
                        _virtualSelf.PublishAudioPerception(
                            result.Text,
                            result.Confidence,
                            result.Language,
                            result.EndTime - result.StartTime,
                            result.IsFinal);
                    }
                });

                _streamingSession.VoiceActivity.Subscribe(activity =>
                {
                    _voiceActivity.OnNext(activity);
                });
            }

            _isListening = true;
            _virtualSelf.ActivateSensor(SensorModality.Audio);
            _virtualSelf.SetState(EmbodimentState.Listening);

            return Result<Unit, string>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit, string>.Failure($"Failed to start listening: {ex.Message}");
        }
    }

    /// <summary>
    /// Stops listening.
    /// </summary>
    public async Task<Result<Unit, string>> StopListeningAsync(CancellationToken ct = default)
    {
        if (!_isListening) return Result<Unit, string>.Success(Unit.Value);

        try
        {
            if (_streamingSession != null)
            {
                await _streamingSession.EndAudioAsync(ct);
                await _streamingSession.DisposeAsync();
                _streamingSession = null;
            }

            _isListening = false;
            _virtualSelf.DeactivateSensor(SensorModality.Audio);

            return Result<Unit, string>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit, string>.Failure($"Failed to stop listening: {ex.Message}");
        }
    }

    /// <summary>
    /// Processes an audio chunk (push from microphone).
    /// </summary>
    public async Task<Result<Unit, string>> ProcessAudioChunkAsync(
        byte[] audioData,
        bool isFinal = false,
        CancellationToken ct = default)
    {
        if (_disposed) return Result<Unit, string>.Failure("Sensor is disposed");
        if (!_isListening) return Result<Unit, string>.Failure("Not listening");

        try
        {
            var chunk = new AudioChunk(
                audioData,
                _config.SampleRate,
                _config.Channels,
                DateTime.UtcNow,
                isFinal);

            _audioChunks.OnNext(chunk);

            if (_streamingSession != null)
            {
                await _streamingSession.PushAudioAsync(audioData, ct);

                if (isFinal)
                {
                    await _streamingSession.EndAudioAsync(ct);
                }
            }

            return Result<Unit, string>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit, string>.Failure($"Failed to process audio: {ex.Message}");
        }
    }

    /// <summary>
    /// Transcribes a complete audio file.
    /// </summary>
    public async Task<Result<TranscriptionResult, string>> TranscribeFileAsync(
        string filePath,
        CancellationToken ct = default)
    {
        if (_disposed) return Result<TranscriptionResult, string>.Failure("Sensor is disposed");

        var result = await _sttModel.TranscribeAsync(filePath, _config.Language, ct);

        if (result.IsSuccess)
        {
            _transcriptions.OnNext(result.Value);
            _virtualSelf.PublishAudioPerception(
                result.Value.Text,
                result.Value.Confidence,
                result.Value.Language,
                result.Value.EndTime - result.Value.StartTime,
                true);
        }

        return result;
    }

    /// <summary>
    /// Disposes the sensor.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _isListening = false;
        _streamingSession?.DisposeAsync().AsTask().Wait();

        _audioChunks.OnCompleted();
        _transcriptions.OnCompleted();
        _voiceActivity.OnCompleted();

        _audioChunks.Dispose();
        _transcriptions.Dispose();
        _voiceActivity.Dispose();
    }
}
