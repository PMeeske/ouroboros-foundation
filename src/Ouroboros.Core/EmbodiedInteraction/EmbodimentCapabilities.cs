namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Capabilities supported by an embodiment provider.
/// </summary>
[Flags]
public enum EmbodimentCapabilities
{
    /// <summary>No special capabilities.</summary>
    None = 0,

    /// <summary>Can capture video frames.</summary>
    VideoCapture = 1 << 0,

    /// <summary>Can capture audio.</summary>
    AudioCapture = 1 << 1,

    /// <summary>Can output audio/speech.</summary>
    AudioOutput = 1 << 2,

    /// <summary>Can perform vision analysis.</summary>
    VisionAnalysis = 1 << 3,

    /// <summary>Can detect motion.</summary>
    MotionDetection = 1 << 4,

    /// <summary>Can control lighting.</summary>
    LightingControl = 1 << 5,

    /// <summary>Can control power (plugs/switches).</summary>
    PowerControl = 1 << 6,

    /// <summary>Can perform pan/tilt/zoom.</summary>
    PTZControl = 1 << 7,

    /// <summary>Supports two-way audio communication.</summary>
    TwoWayAudio = 1 << 8,

    /// <summary>Supports streaming video.</summary>
    VideoStreaming = 1 << 9
}