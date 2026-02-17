namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Visual perception from camera/video.
/// </summary>
public sealed record VisualPerception(
    Guid Id,
    DateTime Timestamp,
    double Confidence,
    string Description,
    IReadOnlyList<DetectedObject> Objects,
    IReadOnlyList<DetectedFace> Faces,
    string? SceneType,
    string? DominantEmotion,
    byte[]? RawFrame) : PerceptionEvent(Id, SensorModality.Visual, Timestamp, Confidence);