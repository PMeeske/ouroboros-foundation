namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// A video frame from the camera.
/// </summary>
/// <param name="Data">Raw frame data (RGB or JPEG).</param>
/// <param name="Width">Frame width.</param>
/// <param name="Height">Frame height.</param>
/// <param name="Format">Data format (rgb24, jpeg, png).</param>
/// <param name="FrameNumber">Sequential frame number.</param>
/// <param name="Timestamp">Capture timestamp.</param>
public sealed record VideoFrame(
    byte[] Data,
    int Width,
    int Height,
    string Format,
    long FrameNumber,
    DateTime Timestamp);