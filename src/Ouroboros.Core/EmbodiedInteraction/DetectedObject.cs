namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Detected object in visual perception.
/// </summary>
/// <param name="Label">Object class label.</param>
/// <param name="Confidence">Detection confidence.</param>
/// <param name="BoundingBox">Bounding box (x, y, width, height) normalized 0-1.</param>
/// <param name="Attributes">Additional attributes.</param>
public sealed record DetectedObject(
    string Label,
    double Confidence,
    (double X, double Y, double Width, double Height) BoundingBox,
    IReadOnlyDictionary<string, string>? Attributes);