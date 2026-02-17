namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Types of embodiment domain events.
/// </summary>
public enum EmbodimentDomainEventType
{
    /// <summary>Aggregate activated.</summary>
    AggregateActivated,

    /// <summary>Aggregate deactivated.</summary>
    AggregateDeactivated,

    /// <summary>Provider registered.</summary>
    ProviderRegistered,

    /// <summary>Provider unregistered.</summary>
    ProviderUnregistered,

    /// <summary>Provider connected.</summary>
    ProviderConnected,

    /// <summary>Provider disconnected.</summary>
    ProviderDisconnected,

    /// <summary>Provider error.</summary>
    ProviderError,

    /// <summary>Sensor activated.</summary>
    SensorActivated,

    /// <summary>Sensor deactivated.</summary>
    SensorDeactivated,

    /// <summary>Perception received.</summary>
    PerceptionReceived,

    /// <summary>Action executed.</summary>
    ActionExecuted,

    /// <summary>State changed.</summary>
    StateChanged,

    /// <summary>Motion detected.</summary>
    MotionDetected,

    /// <summary>Person detected.</summary>
    PersonDetected
}