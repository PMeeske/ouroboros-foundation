namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Represents the virtual body schema - the agent's model of its own embodiment.
/// </summary>
/// <param name="Id">Unique identifier for this virtual self instance.</param>
/// <param name="Name">Display name/persona.</param>
/// <param name="State">Current embodiment state.</param>
/// <param name="Capabilities">Available sensory/motor capabilities.</param>
/// <param name="ActiveSensors">Currently active sensor modalities.</param>
/// <param name="ActiveActuators">Currently active output modalities.</param>
/// <param name="AttentionFocus">Current focus of attention.</param>
/// <param name="EnergyLevel">Simulated energy/resource level (0-1).</param>
/// <param name="CreatedAt">When this virtual self was instantiated.</param>
/// <param name="LastActiveAt">Last activity timestamp.</param>
public sealed record VirtualSelfState(
    Guid Id,
    string Name,
    EmbodimentState State,
    IReadOnlySet<string> Capabilities,
    IReadOnlySet<SensorModality> ActiveSensors,
    IReadOnlySet<ActuatorModality> ActiveActuators,
    AttentionFocus? AttentionFocus,
    double EnergyLevel,
    DateTime CreatedAt,
    DateTime LastActiveAt)
{
    /// <summary>
    /// Creates a default virtual self state.
    /// </summary>
    public static VirtualSelfState CreateDefault(string name = "Ouroboros") =>
        new(
            Guid.NewGuid(),
            name,
            EmbodimentState.Dormant,
            new HashSet<string> { "audio_input", "audio_output", "visual_input", "text_input", "text_output" },
            new HashSet<SensorModality>(),
            new HashSet<ActuatorModality>(),
            null,
            1.0,
            DateTime.UtcNow,
            DateTime.UtcNow);

    /// <summary>
    /// Returns a new state with updated modality.
    /// </summary>
    public VirtualSelfState WithState(EmbodimentState newState) =>
        this with { State = newState, LastActiveAt = DateTime.UtcNow };

    /// <summary>
    /// Returns a new state with sensor activated.
    /// </summary>
    public VirtualSelfState WithSensorActive(SensorModality sensor)
    {
        var newSensors = new HashSet<SensorModality>(ActiveSensors) { sensor };
        return this with { ActiveSensors = newSensors, LastActiveAt = DateTime.UtcNow };
    }

    /// <summary>
    /// Returns a new state with sensor deactivated.
    /// </summary>
    public VirtualSelfState WithSensorInactive(SensorModality sensor)
    {
        var newSensors = new HashSet<SensorModality>(ActiveSensors);
        newSensors.Remove(sensor);
        return this with { ActiveSensors = newSensors, LastActiveAt = DateTime.UtcNow };
    }

    /// <summary>
    /// Returns a new state with attention focus updated.
    /// </summary>
    public VirtualSelfState WithAttention(AttentionFocus focus) =>
        this with { AttentionFocus = focus, LastActiveAt = DateTime.UtcNow };

    /// <summary>
    /// Gets whether the agent is currently perceiving.
    /// </summary>
    public bool IsPerceiving => ActiveSensors.Count > 0;

    /// <summary>
    /// Gets whether the agent can hear.
    /// </summary>
    public bool CanHear => Capabilities.Contains("audio_input");

    /// <summary>
    /// Gets whether the agent can see.
    /// </summary>
    public bool CanSee => Capabilities.Contains("visual_input");

    /// <summary>
    /// Gets whether the agent can speak.
    /// </summary>
    public bool CanSpeak => Capabilities.Contains("audio_output");
}