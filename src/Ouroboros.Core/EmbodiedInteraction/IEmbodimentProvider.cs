// <copyright file="IEmbodimentProvider.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;

namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Repository-like abstraction for embodiment state and capabilities.
/// Provides a general interface for different embodiment backends
/// (Tapo cameras, virtual sensors, robotic systems, etc.).
/// </summary>
public interface IEmbodimentProvider : IDisposable
{
    /// <summary>
    /// Gets the unique identifier of this embodiment provider.
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    /// Gets the human-readable name of the provider.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets whether the provider is currently connected and operational.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Connects to the embodiment backend and initializes resources.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<EmbodimentCapabilities>> ConnectAsync(CancellationToken ct = default);

    /// <summary>
    /// Disconnects from the embodiment backend and releases resources.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<Unit>> DisconnectAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the current state of the embodiment.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the current embodiment state.</returns>
    Task<Result<EmbodimentState>> GetStateAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the available sensors from this provider.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the list of available sensors.</returns>
    Task<Result<IReadOnlyList<SensorInfo>>> GetSensorsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets the available actuators from this provider.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the list of available actuators.</returns>
    Task<Result<IReadOnlyList<ActuatorInfo>>> GetActuatorsAsync(CancellationToken ct = default);

    /// <summary>
    /// Activates a sensor by ID.
    /// </summary>
    /// <param name="sensorId">The sensor identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<Unit>> ActivateSensorAsync(string sensorId, CancellationToken ct = default);

    /// <summary>
    /// Deactivates a sensor by ID.
    /// </summary>
    /// <param name="sensorId">The sensor identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<Unit>> DeactivateSensorAsync(string sensorId, CancellationToken ct = default);

    /// <summary>
    /// Reads perception data from a sensor.
    /// </summary>
    /// <param name="sensorId">The sensor identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the perception data.</returns>
    Task<Result<PerceptionData>> ReadSensorAsync(string sensorId, CancellationToken ct = default);

    /// <summary>
    /// Executes an action through an actuator.
    /// </summary>
    /// <param name="actuatorId">The actuator identifier.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the action outcome.</returns>
    Task<Result<ActionOutcome>> ExecuteActionAsync(
        string actuatorId,
        ActuatorAction action,
        CancellationToken ct = default);

    /// <summary>
    /// Observable stream of perception events from all active sensors.
    /// </summary>
    IObservable<PerceptionData> Perceptions { get; }

    /// <summary>
    /// Observable stream of provider state changes.
    /// </summary>
    IObservable<EmbodimentProviderEvent> Events { get; }
}