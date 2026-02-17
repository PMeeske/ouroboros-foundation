namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Configuration for the visual sensor (camera).
/// </summary>
/// <param name="Width">Frame width in pixels.</param>
/// <param name="Height">Frame height in pixels.</param>
/// <param name="FrameRate">Frames per second.</param>
/// <param name="EnableObjectDetection">Enable object detection.</param>
/// <param name="EnableFaceDetection">Enable face detection.</param>
/// <param name="EnableSceneClassification">Enable scene classification.</param>
/// <param name="EnableEmotionDetection">Enable emotion detection on faces.</param>
/// <param name="ProcessEveryNthFrame">Process every Nth frame (for efficiency).</param>
/// <param name="MaxObjectsToDetect">Maximum objects to return per frame.</param>
public sealed record VisualSensorConfig(
    int Width = 640,
    int Height = 480,
    int FrameRate = 30,
    bool EnableObjectDetection = true,
    bool EnableFaceDetection = true,
    bool EnableSceneClassification = true,
    bool EnableEmotionDetection = true,
    int ProcessEveryNthFrame = 5,
    int MaxObjectsToDetect = 20);