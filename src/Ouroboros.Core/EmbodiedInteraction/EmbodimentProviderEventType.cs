namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Types of embodiment provider events.
/// </summary>
public enum EmbodimentProviderEventType
{
    /// <summary>Provider connected.</summary>
    Connected,

    /// <summary>Provider disconnected.</summary>
    Disconnected,

    /// <summary>Sensor activated.</summary>
    SensorActivated,

    /// <summary>Sensor deactivated.</summary>
    SensorDeactivated,

    /// <summary>Error occurred.</summary>
    Error,

    /// <summary>State changed.</summary>
    StateChanged,

    /// <summary>Motion detected.</summary>
    MotionDetected,

    /// <summary>Person detected.</summary>
    PersonDetected
}