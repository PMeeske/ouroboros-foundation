namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Interface for vision/image analysis models.
/// </summary>
[Obsolete("No implementations exist. Scheduled for removal.")]
public interface IVisionModel
{
    /// <summary>
    /// Gets the model name/identifier.
    /// </summary>
    string ModelName { get; }

    /// <summary>
    /// Analyzes an image and returns a description.
    /// </summary>
    Task<Result<VisionAnalysisResult, string>> AnalyzeImageAsync(
        byte[] imageData,
        string format,
        VisionAnalysisOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Analyzes an image file.
    /// </summary>
    Task<Result<VisionAnalysisResult, string>> AnalyzeImageFileAsync(
        string filePath,
        VisionAnalysisOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Answers a question about an image.
    /// </summary>
    Task<Result<string, string>> AnswerQuestionAsync(
        byte[] imageData,
        string format,
        string question,
        CancellationToken ct = default);

    /// <summary>
    /// Detects objects in an image.
    /// </summary>
    Task<Result<IReadOnlyList<DetectedObject>, string>> DetectObjectsAsync(
        byte[] imageData,
        string format,
        int maxObjects = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Detects faces in an image.
    /// </summary>
    Task<Result<IReadOnlyList<DetectedFace>, string>> DetectFacesAsync(
        byte[] imageData,
        string format,
        bool analyzeEmotion = true,
        CancellationToken ct = default);

    /// <summary>
    /// Gets whether this model supports real-time streaming.
    /// </summary>
    bool SupportsStreaming { get; }
}