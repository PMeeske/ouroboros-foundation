namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Detected face in visual perception.
/// </summary>
/// <param name="FaceId">Tracking ID for the face.</param>
/// <param name="Confidence">Detection confidence.</param>
/// <param name="BoundingBox">Face bounding box.</param>
/// <param name="Emotion">Detected emotion if available.</param>
/// <param name="Age">Estimated age if available.</param>
/// <param name="IsKnown">Whether this is a recognized person.</param>
/// <param name="PersonId">ID of recognized person if known.</param>
public sealed record DetectedFace(
    string FaceId,
    double Confidence,
    (double X, double Y, double Width, double Height) BoundingBox,
    string? Emotion,
    int? Age,
    bool IsKnown,
    string? PersonId);