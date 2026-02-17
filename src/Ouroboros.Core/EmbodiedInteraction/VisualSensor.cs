// <copyright file="VisualSensor.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;

namespace Ouroboros.Core.EmbodiedInteraction;

using System.Reactive.Linq;
using System.Reactive.Subjects;
using Ouroboros.Core.Monads;

/// <summary>
/// Visual sensor that captures camera input and performs vision analysis.
/// </summary>
public sealed class VisualSensor : IDisposable
{
    private readonly VisualSensorConfig _config;
    private readonly IVisionModel _visionModel;
    private readonly VirtualSelf _virtualSelf;
    private readonly Subject<VideoFrame> _frames = new();
    private readonly Subject<VisionAnalysisResult> _analysisResults = new();
    private long _frameCount;
    private bool _isObserving;
    private bool _disposed;

    /// <summary>
    /// Initializes a new visual sensor.
    /// </summary>
    /// <param name="visionModel">The vision model for analysis.</param>
    /// <param name="virtualSelf">The virtual self to publish perceptions to.</param>
    /// <param name="config">Sensor configuration.</param>
    public VisualSensor(
        IVisionModel visionModel,
        VirtualSelf virtualSelf,
        VisualSensorConfig? config = null)
    {
        _visionModel = visionModel ?? throw new ArgumentNullException(nameof(visionModel));
        _virtualSelf = virtualSelf ?? throw new ArgumentNullException(nameof(virtualSelf));
        _config = config ?? new VisualSensorConfig();
    }

    /// <summary>
    /// Gets the vision model name.
    /// </summary>
    public string ModelName => _visionModel.ModelName;

    /// <summary>
    /// Gets whether currently observing.
    /// </summary>
    public bool IsObserving => _isObserving;

    /// <summary>
    /// Gets the total frames processed.
    /// </summary>
    public long FrameCount => _frameCount;

    /// <summary>
    /// Observable stream of video frames.
    /// </summary>
    public IObservable<VideoFrame> Frames => _frames.AsObservable();

    /// <summary>
    /// Observable stream of analysis results.
    /// </summary>
    public IObservable<VisionAnalysisResult> AnalysisResults => _analysisResults.AsObservable();

    /// <summary>
    /// Starts observing (activates camera).
    /// </summary>
    public Result<Unit, string> StartObserving()
    {
        if (_disposed) return Result<Unit, string>.Failure("Sensor is disposed");
        if (_isObserving) return Result<Unit, string>.Failure("Already observing");

        _isObserving = true;
        _virtualSelf.ActivateSensor(SensorModality.Visual);
        _virtualSelf.SetState(EmbodimentState.Observing);

        return Result<Unit, string>.Success(Unit.Value);
    }

    /// <summary>
    /// Stops observing.
    /// </summary>
    public Result<Unit, string> StopObserving()
    {
        if (!_isObserving) return Result<Unit, string>.Success(Unit.Value);

        _isObserving = false;
        _virtualSelf.DeactivateSensor(SensorModality.Visual);

        return Result<Unit, string>.Success(Unit.Value);
    }

    /// <summary>
    /// Processes a video frame (push from camera).
    /// </summary>
    public async Task<Result<Option<VisionAnalysisResult>, string>> ProcessFrameAsync(
        byte[] frameData,
        int width,
        int height,
        string format = "rgb24",
        CancellationToken ct = default)
    {
        if (_disposed) return Result<Option<VisionAnalysisResult>, string>.Failure("Sensor is disposed");
        if (!_isObserving) return Result<Option<VisionAnalysisResult>, string>.Failure("Not observing");

        _frameCount++;

        var frame = new VideoFrame(
            frameData,
            width,
            height,
            format,
            _frameCount,
            DateTime.UtcNow);

        _frames.OnNext(frame);

        // Only analyze every Nth frame for efficiency
        if (_frameCount % _config.ProcessEveryNthFrame != 0)
        {
            return Result<Option<VisionAnalysisResult>, string>.Success(Option<VisionAnalysisResult>.None());
        }

        // Analyze the frame
        var options = new VisionAnalysisOptions(
            IncludeDescription: true,
            DetectObjects: _config.EnableObjectDetection,
            DetectFaces: _config.EnableFaceDetection,
            ClassifyScene: _config.EnableSceneClassification,
            MaxObjects: _config.MaxObjectsToDetect);

        var analysisResult = await _visionModel.AnalyzeImageAsync(frameData, format, options, ct);

        if (analysisResult.IsSuccess)
        {
            var result = analysisResult.Value;
            _analysisResults.OnNext(result);

            // Publish to virtual self
            _virtualSelf.PublishVisualPerception(
                result.Description,
                result.Objects,
                result.Faces,
                result.SceneType,
                result.Faces.FirstOrDefault()?.Emotion,
                result.Confidence,
                frameData);

            return Result<Option<VisionAnalysisResult>, string>.Success(
                Option<VisionAnalysisResult>.Some(result));
        }

        return Result<Option<VisionAnalysisResult>, string>.Failure(analysisResult.Error);
    }

    /// <summary>
    /// Analyzes a single image file.
    /// </summary>
    public async Task<Result<VisionAnalysisResult, string>> AnalyzeImageAsync(
        string filePath,
        CancellationToken ct = default)
    {
        if (_disposed) return Result<VisionAnalysisResult, string>.Failure("Sensor is disposed");

        var result = await _visionModel.AnalyzeImageFileAsync(filePath, null, ct);

        if (result.IsSuccess)
        {
            _analysisResults.OnNext(result.Value);
            _virtualSelf.PublishVisualPerception(
                result.Value.Description,
                result.Value.Objects,
                result.Value.Faces,
                result.Value.SceneType,
                result.Value.Faces.FirstOrDefault()?.Emotion,
                result.Value.Confidence);
        }

        return result;
    }

    /// <summary>
    /// Asks a question about an image.
    /// </summary>
    public async Task<Result<string, string>> AskAboutImageAsync(
        byte[] imageData,
        string format,
        string question,
        CancellationToken ct = default)
    {
        if (_disposed) return Result<string, string>.Failure("Sensor is disposed");

        return await _visionModel.AnswerQuestionAsync(imageData, format, question, ct);
    }

    /// <summary>
    /// Focuses visual attention on a specific area.
    /// </summary>
    public void FocusOn(string target)
    {
        _virtualSelf.FocusAttention(SensorModality.Visual, target, 1.0);
    }

    /// <summary>
    /// Disposes the sensor.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _isObserving = false;

        _frames.OnCompleted();
        _analysisResults.OnCompleted();

        _frames.Dispose();
        _analysisResults.Dispose();
    }
}
