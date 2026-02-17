namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Result from vision model analysis.
/// </summary>
/// <param name="Description">Natural language description of the scene.</param>
/// <param name="Objects">Detected objects.</param>
/// <param name="Faces">Detected faces.</param>
/// <param name="SceneType">Scene classification.</param>
/// <param name="DominantColors">Dominant colors in the scene.</param>
/// <param name="Text">OCR text if detected.</param>
/// <param name="Confidence">Overall confidence.</param>
/// <param name="ProcessingTimeMs">Processing time in milliseconds.</param>
public sealed record VisionAnalysisResult(
    string Description,
    IReadOnlyList<DetectedObject> Objects,
    IReadOnlyList<DetectedFace> Faces,
    string? SceneType,
    IReadOnlyList<string>? DominantColors,
    string? Text,
    double Confidence,
    long ProcessingTimeMs);