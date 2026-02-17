// <copyright file="BodySchema.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Core.EmbodiedInteraction;

using System.Collections.Immutable;
using Ouroboros.Core.Monads;

/// <summary>
/// The body schema represents the agent's model of its own embodiment -
/// its sensors, actuators, capabilities, and limitations.
/// Based on cognitive science concept of body schema.
/// </summary>
public sealed record BodySchema
{
    private readonly ImmutableDictionary<string, SensorDescriptor> _sensors;
    private readonly ImmutableDictionary<string, ActuatorDescriptor> _actuators;
    private readonly ImmutableHashSet<Capability> _capabilities;
    private readonly ImmutableList<Limitation> _limitations;

    /// <summary>
    /// Initializes a new body schema.
    /// </summary>
    public BodySchema()
    {
        _sensors = ImmutableDictionary<string, SensorDescriptor>.Empty;
        _actuators = ImmutableDictionary<string, ActuatorDescriptor>.Empty;
        _capabilities = ImmutableHashSet<Capability>.Empty;
        _limitations = ImmutableList<Limitation>.Empty;
    }

    private BodySchema(
        ImmutableDictionary<string, SensorDescriptor> sensors,
        ImmutableDictionary<string, ActuatorDescriptor> actuators,
        ImmutableHashSet<Capability> capabilities,
        ImmutableList<Limitation> limitations)
    {
        _sensors = sensors;
        _actuators = actuators;
        _capabilities = capabilities;
        _limitations = limitations;
    }

    /// <summary>
    /// Gets all registered sensors.
    /// </summary>
    public IReadOnlyDictionary<string, SensorDescriptor> Sensors => _sensors;

    /// <summary>
    /// Gets all registered actuators.
    /// </summary>
    public IReadOnlyDictionary<string, ActuatorDescriptor> Actuators => _actuators;

    /// <summary>
    /// Gets aggregate capabilities.
    /// </summary>
    public IReadOnlySet<Capability> Capabilities => _capabilities;

    /// <summary>
    /// Gets known limitations.
    /// </summary>
    public IReadOnlyList<Limitation> Limitations => _limitations;

    /// <summary>
    /// Adds a sensor to the body schema.
    /// </summary>
    public BodySchema WithSensor(SensorDescriptor sensor)
    {
        var newSensors = _sensors.SetItem(sensor.Id, sensor);
        var newCapabilities = _capabilities.Union(sensor.Capabilities);
        return new BodySchema(newSensors, _actuators, newCapabilities, _limitations);
    }

    /// <summary>
    /// Adds an actuator to the body schema.
    /// </summary>
    public BodySchema WithActuator(ActuatorDescriptor actuator)
    {
        var newActuators = _actuators.SetItem(actuator.Id, actuator);
        var newCapabilities = _capabilities.Union(actuator.Capabilities);
        return new BodySchema(_sensors, newActuators, newCapabilities, _limitations);
    }

    /// <summary>
    /// Adds a capability.
    /// </summary>
    public BodySchema WithCapability(Capability capability)
    {
        return new BodySchema(_sensors, _actuators, _capabilities.Add(capability), _limitations);
    }

    /// <summary>
    /// Adds a limitation.
    /// </summary>
    public BodySchema WithLimitation(Limitation limitation)
    {
        return new BodySchema(_sensors, _actuators, _capabilities, _limitations.Add(limitation));
    }

    /// <summary>
    /// Removes a sensor.
    /// </summary>
    public BodySchema WithoutSensor(string sensorId)
    {
        if (!_sensors.ContainsKey(sensorId)) return this;

        var sensor = _sensors[sensorId];
        var newSensors = _sensors.Remove(sensorId);

        // Recompute capabilities from remaining sensors and actuators
        var remainingCaps = ComputeCapabilities(newSensors.Values, _actuators.Values);
        return new BodySchema(newSensors, _actuators, remainingCaps, _limitations);
    }

    /// <summary>
    /// Removes an actuator.
    /// </summary>
    public BodySchema WithoutActuator(string actuatorId)
    {
        if (!_actuators.ContainsKey(actuatorId)) return this;

        var newActuators = _actuators.Remove(actuatorId);
        var remainingCaps = ComputeCapabilities(_sensors.Values, newActuators.Values);
        return new BodySchema(_sensors, newActuators, remainingCaps, _limitations);
    }

    /// <summary>
    /// Checks if a capability is present.
    /// </summary>
    public bool HasCapability(Capability capability) =>
        _capabilities.Contains(capability);

    /// <summary>
    /// Gets a sensor by ID.
    /// </summary>
    public Option<SensorDescriptor> GetSensor(string id) =>
        _sensors.TryGetValue(id, out var sensor)
            ? Option<SensorDescriptor>.Some(sensor)
            : Option<SensorDescriptor>.None();

    /// <summary>
    /// Gets an actuator by ID.
    /// </summary>
    public Option<ActuatorDescriptor> GetActuator(string id) =>
        _actuators.TryGetValue(id, out var actuator)
            ? Option<ActuatorDescriptor>.Some(actuator)
            : Option<ActuatorDescriptor>.None();

    /// <summary>
    /// Gets sensors by modality.
    /// </summary>
    public IEnumerable<SensorDescriptor> GetSensorsByModality(SensorModality modality) =>
        _sensors.Values.Where(s => s.Modality == modality);

    /// <summary>
    /// Gets actuators by modality.
    /// </summary>
    public IEnumerable<ActuatorDescriptor> GetActuatorsByModality(ActuatorModality modality) =>
        _actuators.Values.Where(a => a.Modality == modality);

    /// <summary>
    /// Checks if any sensor provides a capability.
    /// </summary>
    public bool CanPerceive(Capability capability) =>
        _sensors.Values.Any(s => s.Capabilities.Contains(capability) && s.IsActive);

    /// <summary>
    /// Checks if any actuator provides a capability.
    /// </summary>
    public bool CanAct(Capability capability) =>
        _actuators.Values.Any(a => a.Capabilities.Contains(capability) && a.IsActive);

    /// <summary>
    /// Generates a description of embodiment for self-awareness.
    /// </summary>
    public string DescribeSelf()
    {
        var parts = new List<string> { "I am an AI assistant with the following embodiment:" };

        if (_sensors.Any())
        {
            parts.Add("\nSensors:");
            foreach (var sensor in _sensors.Values.Where(s => s.IsActive))
            {
                parts.Add($"  - {sensor.Name} ({sensor.Modality}): {string.Join(", ", sensor.Capabilities)}");
            }
        }

        if (_actuators.Any())
        {
            parts.Add("\nActuators:");
            foreach (var actuator in _actuators.Values.Where(a => a.IsActive))
            {
                parts.Add($"  - {actuator.Name} ({actuator.Modality}): {string.Join(", ", actuator.Capabilities)}");
            }
        }

        if (_capabilities.Any())
        {
            parts.Add($"\nCapabilities: {string.Join(", ", _capabilities)}");
        }

        if (_limitations.Any())
        {
            parts.Add("\nLimitations:");
            foreach (var limitation in _limitations)
            {
                parts.Add($"  - {limitation.Type}: {limitation.Description}");
            }
        }

        return string.Join("\n", parts);
    }

    /// <summary>
    /// Creates a default body schema for a conversational AI.
    /// </summary>
    public static BodySchema CreateConversational() =>
        new BodySchema()
            .WithSensor(SensorDescriptor.Text("text-in", "Text Input"))
            .WithActuator(ActuatorDescriptor.Text("text-out", "Text Output"))
            .WithCapability(Capability.Reasoning)
            .WithCapability(Capability.Remembering)
            .WithCapability(Capability.Learning)
            .WithLimitation(new Limitation(
                LimitationType.MemoryBounded,
                "Limited context window"))
            .WithLimitation(new Limitation(
                LimitationType.KnowledgeGap,
                "Training data has a cutoff date"));

    /// <summary>
    /// Creates a multimodal body schema with audio and visual.
    /// </summary>
    public static BodySchema CreateMultimodal() =>
        CreateConversational()
            .WithSensor(SensorDescriptor.Audio("mic", "Microphone"))
            .WithSensor(SensorDescriptor.Visual("camera", "Camera"))
            .WithActuator(ActuatorDescriptor.Voice("voice", "Voice Output"))
            .WithCapability(Capability.EmotionPerception)
            .WithCapability(Capability.EmotionExpression);

    private static ImmutableHashSet<Capability> ComputeCapabilities(
        IEnumerable<SensorDescriptor> sensors,
        IEnumerable<ActuatorDescriptor> actuators)
    {
        var caps = ImmutableHashSet<Capability>.Empty;

        foreach (var s in sensors.Where(s => s.IsActive))
            caps = caps.Union(s.Capabilities);

        foreach (var a in actuators.Where(a => a.IsActive))
            caps = caps.Union(a.Capabilities);

        return caps;
    }
}
