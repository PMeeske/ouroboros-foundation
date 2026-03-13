// <copyright file="EmbodimentAggregate.Sensors.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;

namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Sensor, actuator, and body schema operations for the EmbodimentAggregate.
/// </summary>
public sealed partial class EmbodimentAggregate
{
    /// <summary>
    /// Activates a sensor across the aggregate.
    /// </summary>
    /// <param name="sensorId">Full sensor ID (providerId:sensorId).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result<SensorInfo>> ActivateSensorAsync(
        string sensorId,
        CancellationToken ct = default)
    {
        if (_disposed) return Result<SensorInfo>.Failure("Aggregate is disposed");

        (string? providerId, string? localSensorId) = ParseResourceId(sensorId);
        if (!_providers.TryGetValue(providerId, out IEmbodimentProvider? provider))
        {
            return Result<SensorInfo>.Failure($"Provider '{providerId}' not found");
        }

        Result<Unit> result = await provider.ActivateSensorAsync(localSensorId, ct).ConfigureAwait(false);
        if (result.IsFailure)
        {
            return Result<SensorInfo>.Failure(result.Error);
        }

        // Update local state
        Result<IReadOnlyList<SensorInfo>> sensorsResult = await provider.GetSensorsAsync(ct).ConfigureAwait(false);
        if (sensorsResult.IsSuccess)
        {
            SensorInfo? sensor = sensorsResult.Value.FirstOrDefault(s => s.SensorId == localSensorId);
            if (sensor != null)
            {
                string fullId = $"{providerId}:{localSensorId}";
                _activeSensors[fullId] = sensor;

                RaiseDomainEvent(new EmbodimentDomainEvent(
                    EmbodimentDomainEventType.SensorActivated,
                    DateTime.UtcNow,
                    new Dictionary<string, object>
                    {
                        ["sensorId"] = fullId,
                        ["modality"] = sensor.Modality.ToString()
                    }));

                return Result<SensorInfo>.Success(sensor);
            }
        }

        return Result<SensorInfo>.Failure("Sensor activated but not found in provider");
    }

    /// <summary>
    /// Deactivates a sensor.
    /// </summary>
    /// <param name="sensorId">Full sensor ID (providerId:sensorId).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result<Unit>> DeactivateSensorAsync(
        string sensorId,
        CancellationToken ct = default)
    {
        if (_disposed) return Result<Unit>.Failure("Aggregate is disposed");

        (string? providerId, string? localSensorId) = ParseResourceId(sensorId);
        if (!_providers.TryGetValue(providerId, out IEmbodimentProvider? provider))
        {
            return Result<Unit>.Failure($"Provider '{providerId}' not found");
        }

        Result<Unit> result = await provider.DeactivateSensorAsync(localSensorId, ct).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            _activeSensors.TryRemove(sensorId, out _);

            RaiseDomainEvent(new EmbodimentDomainEvent(
                EmbodimentDomainEventType.SensorDeactivated,
                DateTime.UtcNow,
                new Dictionary<string, object> { ["sensorId"] = sensorId }));
        }

        return result;
    }

    /// <summary>
    /// Reads perception data from a sensor.
    /// </summary>
    /// <param name="sensorId">Full sensor ID (providerId:sensorId).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the perception data.</returns>
    public async Task<Result<PerceptionData>> ReadSensorAsync(
        string sensorId,
        CancellationToken ct = default)
    {
        if (_disposed) return Result<PerceptionData>.Failure("Aggregate is disposed");

        (string? providerId, string? localSensorId) = ParseResourceId(sensorId);
        if (!_providers.TryGetValue(providerId, out IEmbodimentProvider? provider))
        {
            return Result<PerceptionData>.Failure($"Provider '{providerId}' not found");
        }

        return await provider.ReadSensorAsync(localSensorId, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an action through an actuator.
    /// </summary>
    /// <param name="actuatorId">Full actuator ID (providerId:actuatorId).</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the action outcome.</returns>
    public async Task<Result<ActionOutcome>> ExecuteActionAsync(
        string actuatorId,
        ActuatorAction action,
        CancellationToken ct = default)
    {
        if (_disposed) return Result<ActionOutcome>.Failure("Aggregate is disposed");

        (string? providerId, string? localActuatorId) = ParseResourceId(actuatorId);
        if (!_providers.TryGetValue(providerId, out IEmbodimentProvider? provider))
        {
            return Result<ActionOutcome>.Failure($"Provider '{providerId}' not found");
        }

        Result<ActionOutcome> result = await provider.ExecuteActionAsync(localActuatorId, action, ct).ConfigureAwait(false);

        if (result.IsSuccess)
        {
            RaiseDomainEvent(new EmbodimentDomainEvent(
                EmbodimentDomainEventType.ActionExecuted,
                DateTime.UtcNow,
                new Dictionary<string, object>
                {
                    ["actuatorId"] = actuatorId,
                    ["actionType"] = action.ActionType,
                    ["success"] = result.Value.Success
                }));
        }

        return result;
    }

    /// <summary>
    /// Creates a BodySchema from the current aggregate state.
    /// </summary>
    /// <returns>A BodySchema representing the aggregate's embodiment.</returns>
    public BodySchema ToBodySchema()
    {
        BodySchema schema = new BodySchema()
            .WithCapability(Capability.Reasoning)
            .WithCapability(Capability.Remembering);

        foreach ((string? id, SensorInfo? sensor) in _activeSensors)
        {
            HashSet<Capability> capabilities = sensor.Modality switch
            {
                SensorModality.Audio => new HashSet<Capability> { Capability.Hearing },
                SensorModality.Visual => new HashSet<Capability> { Capability.Seeing },
                SensorModality.Text => new HashSet<Capability> { Capability.Reading },
                _ => new HashSet<Capability>()
            };

            schema = schema.WithSensor(new SensorDescriptor(
                id,
                sensor.Modality,
                sensor.Name,
                sensor.IsActive,
                capabilities,
                sensor.Properties));
        }

        foreach ((string? id, ActuatorInfo? actuator) in _activeActuators)
        {
            HashSet<Capability> capabilities = actuator.Modality switch
            {
                ActuatorModality.Voice => new HashSet<Capability> { Capability.Speaking },
                ActuatorModality.Text => new HashSet<Capability> { Capability.Writing },
                _ => new HashSet<Capability>()
            };

            schema = schema.WithActuator(new ActuatorDescriptor(
                id,
                actuator.Modality,
                actuator.Name,
                actuator.IsActive,
                capabilities,
                actuator.Properties));
        }

        return schema;
    }
}
