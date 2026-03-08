// <copyright file="EmbodimentAggregate.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Ouroboros.Abstractions;

namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Domain aggregate that manages embodiment state and coordinates between
/// multiple embodiment providers. Follows DDD patterns where the aggregate
/// owns the business logic and providers serve as state sources.
/// </summary>
public sealed partial class EmbodimentAggregate : IDisposable
{
    private readonly ConcurrentDictionary<string, IEmbodimentProvider> _providers = new();
    private readonly ConcurrentDictionary<string, SensorInfo> _activeSensors = new();
    private readonly ConcurrentDictionary<string, ActuatorInfo> _activeActuators = new();
    private readonly Subject<EmbodimentDomainEvent> _domainEvents = new();
    private readonly Subject<PerceptionData> _unifiedPerceptions = new();
    private readonly List<IDisposable> _subscriptions = [];
    private readonly object _lock = new();

    private EmbodimentAggregateState _state;
    private bool _disposed;

    /// <summary>
    /// Initializes a new embodiment aggregate.
    /// </summary>
    /// <param name="aggregateId">Unique identifier for this aggregate.</param>
    /// <param name="name">Human-readable name.</param>
    public EmbodimentAggregate(string aggregateId, string name = "EmbodimentAggregate")
    {
        AggregateId = aggregateId ?? throw new ArgumentNullException(nameof(aggregateId));
        Name = name;
        _state = new EmbodimentAggregateState(
            AggregateId,
            Name,
            AggregateStatus.Inactive,
            EmbodimentCapabilities.None,
            DateTime.UtcNow);
    }

    /// <summary>
    /// Gets the aggregate identifier.
    /// </summary>
    public string AggregateId { get; }

    /// <summary>
    /// Gets the aggregate name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the current aggregate state.
    /// </summary>
    public EmbodimentAggregateState State => _state;

    /// <summary>
    /// Gets all registered providers.
    /// </summary>
    public IReadOnlyDictionary<string, IEmbodimentProvider> Providers => _providers;

    /// <summary>
    /// Gets all active sensors across all providers.
    /// </summary>
    public IReadOnlyDictionary<string, SensorInfo> ActiveSensors => _activeSensors;

    /// <summary>
    /// Gets all active actuators across all providers.
    /// </summary>
    public IReadOnlyDictionary<string, ActuatorInfo> ActiveActuators => _activeActuators;

    /// <summary>
    /// Observable stream of domain events.
    /// </summary>
    public IObservable<EmbodimentDomainEvent> DomainEvents => _domainEvents.AsObservable();

    /// <summary>
    /// Observable stream of unified perceptions from all providers.
    /// </summary>
    public IObservable<PerceptionData> UnifiedPerceptions => _unifiedPerceptions.AsObservable();

    /// <summary>
    /// Registers an embodiment provider with the aggregate.
    /// </summary>
    /// <param name="provider">The provider to register.</param>
    /// <returns>Result indicating success or failure.</returns>
    public Result<EmbodimentAggregate> RegisterProvider(IEmbodimentProvider provider)
    {
        if (_disposed) return Result<EmbodimentAggregate>.Failure("Aggregate is disposed");
        if (provider == null) return Result<EmbodimentAggregate>.Failure("Provider is required");

        if (!_providers.TryAdd(provider.ProviderId, provider))
        {
            return Result<EmbodimentAggregate>.Failure($"Provider '{provider.ProviderId}' is already registered");
        }

        // Subscribe to provider perceptions
        IDisposable perceptionSub = provider.Perceptions
            .Subscribe(perception =>
            {
                _unifiedPerceptions.OnNext(perception);
                RaiseDomainEvent(new EmbodimentDomainEvent(
                    EmbodimentDomainEventType.PerceptionReceived,
                    DateTime.UtcNow,
                    new Dictionary<string, object>
                    {
                        ["providerId"] = provider.ProviderId,
                        ["sensorId"] = perception.SensorId,
                        ["modality"] = perception.Modality.ToString()
                    }));
            });

        // Subscribe to provider events
        IDisposable eventSub = provider.Events
            .Subscribe(providerEvent =>
            {
                HandleProviderEvent(provider.ProviderId, providerEvent);
            });

        lock (_lock)
        {
            _subscriptions.Add(perceptionSub);
            _subscriptions.Add(eventSub);
        }

        RaiseDomainEvent(new EmbodimentDomainEvent(
            EmbodimentDomainEventType.ProviderRegistered,
            DateTime.UtcNow,
            new Dictionary<string, object>
            {
                ["providerId"] = provider.ProviderId,
                ["providerName"] = provider.ProviderName
            }));

        return Result<EmbodimentAggregate>.Success(this);
    }

    /// <summary>
    /// Unregisters an embodiment provider.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result<EmbodimentAggregate>> UnregisterProviderAsync(
        string providerId,
        CancellationToken ct = default)
    {
        if (_disposed) return Result<EmbodimentAggregate>.Failure("Aggregate is disposed");

        if (!_providers.TryRemove(providerId, out IEmbodimentProvider? provider))
        {
            return Result<EmbodimentAggregate>.Failure($"Provider '{providerId}' not found");
        }

        // Disconnect and dispose the provider
        await provider.DisconnectAsync(ct);
        provider.Dispose();

        // Remove associated sensors and actuators - collect keys first to avoid
        // modifying collection during iteration
        string[] sensorKeysToRemove = _activeSensors.Keys
            .Where(k => k.StartsWith($"{providerId}:", StringComparison.Ordinal))
            .ToArray();
        foreach (string? key in sensorKeysToRemove)
        {
            _activeSensors.TryRemove(key, out _);
        }

        string[] actuatorKeysToRemove = _activeActuators.Keys
            .Where(k => k.StartsWith($"{providerId}:", StringComparison.Ordinal))
            .ToArray();
        foreach (string? key in actuatorKeysToRemove)
        {
            _activeActuators.TryRemove(key, out _);
        }

        UpdateAggregateCapabilities();

        RaiseDomainEvent(new EmbodimentDomainEvent(
            EmbodimentDomainEventType.ProviderUnregistered,
            DateTime.UtcNow,
            new Dictionary<string, object> { ["providerId"] = providerId }));

        return Result<EmbodimentAggregate>.Success(this);
    }

    /// <summary>
    /// Connects all registered providers and activates the aggregate.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result containing the aggregate capabilities.</returns>
    public async Task<Result<EmbodimentCapabilities>> ActivateAsync(CancellationToken ct = default)
    {
        if (_disposed) return Result<EmbodimentCapabilities>.Failure("Aggregate is disposed");
        if (_state.Status == AggregateStatus.Active)
        {
            return Result<EmbodimentCapabilities>.Success(_state.Capabilities);
        }

        UpdateState(_state with { Status = AggregateStatus.Activating });

        EmbodimentCapabilities aggregateCapabilities = EmbodimentCapabilities.None;
        List<string> errors = new List<string>();

        foreach ((string? providerId, IEmbodimentProvider? provider) in _providers)
        {
            try
            {
                Result<EmbodimentCapabilities> connectResult = await provider.ConnectAsync(ct);
                if (connectResult.IsSuccess)
                {
                    aggregateCapabilities |= connectResult.Value;

                    // Load sensors and actuators
                    await LoadProviderResourcesAsync(providerId, provider, ct);
                }
                else
                {
                    errors.Add($"{providerId}: {connectResult.Error}");
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                errors.Add($"{providerId}: {ex.Message}");
            }
        }

        if (_providers.Count > 0 && errors.Count == _providers.Count)
        {
            UpdateState(_state with { Status = AggregateStatus.Failed });
            return Result<EmbodimentCapabilities>.Failure(
                $"All providers failed to connect: {string.Join("; ", errors)}");
        }

        UpdateState(_state with
        {
            Status = AggregateStatus.Active,
            Capabilities = aggregateCapabilities,
            LastUpdatedAt = DateTime.UtcNow
        });

        RaiseDomainEvent(new EmbodimentDomainEvent(
            EmbodimentDomainEventType.AggregateActivated,
            DateTime.UtcNow,
            new Dictionary<string, object>
            {
                ["capabilities"] = aggregateCapabilities.ToString(),
                ["providerCount"] = _providers.Count,
                ["sensorCount"] = _activeSensors.Count,
                ["actuatorCount"] = _activeActuators.Count
            }));

        return Result<EmbodimentCapabilities>.Success(aggregateCapabilities);
    }

    /// <summary>
    /// Deactivates the aggregate and disconnects all providers.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    public async Task<Result<Unit>> DeactivateAsync(CancellationToken ct = default)
    {
        if (_disposed) return Result<Unit>.Failure("Aggregate is disposed");
        if (_state.Status == AggregateStatus.Inactive)
        {
            return Result<Unit>.Success(Unit.Value);
        }

        UpdateState(_state with { Status = AggregateStatus.Deactivating });

        foreach ((string _, IEmbodimentProvider? provider) in _providers)
        {
            await provider.DisconnectAsync(ct);
        }

        _activeSensors.Clear();
        _activeActuators.Clear();

        UpdateState(_state with
        {
            Status = AggregateStatus.Inactive,
            Capabilities = EmbodimentCapabilities.None,
            LastUpdatedAt = DateTime.UtcNow
        });

        RaiseDomainEvent(new EmbodimentDomainEvent(
            EmbodimentDomainEventType.AggregateDeactivated,
            DateTime.UtcNow));

        return Result<Unit>.Success(Unit.Value);
    }


    private async Task LoadProviderResourcesAsync(
        string providerId,
        IEmbodimentProvider provider,
        CancellationToken ct)
    {
        Result<IReadOnlyList<SensorInfo>> sensorsResult = await provider.GetSensorsAsync(ct);
        if (sensorsResult.IsSuccess)
        {
            foreach (SensorInfo sensor in sensorsResult.Value)
            {
                string fullId = $"{providerId}:{sensor.SensorId}";
                if (sensor.IsActive)
                {
                    _activeSensors[fullId] = sensor;
                }
            }
        }

        Result<IReadOnlyList<ActuatorInfo>> actuatorsResult = await provider.GetActuatorsAsync(ct);
        if (actuatorsResult.IsSuccess)
        {
            foreach (ActuatorInfo actuator in actuatorsResult.Value)
            {
                string fullId = $"{providerId}:{actuator.ActuatorId}";
                if (actuator.IsActive)
                {
                    _activeActuators[fullId] = actuator;
                }
            }
        }
    }

    private void HandleProviderEvent(string providerId, EmbodimentProviderEvent providerEvent)
    {
        EmbodimentDomainEventType domainEventType = providerEvent.EventType switch
        {
            EmbodimentProviderEventType.Connected => EmbodimentDomainEventType.ProviderConnected,
            EmbodimentProviderEventType.Disconnected => EmbodimentDomainEventType.ProviderDisconnected,
            EmbodimentProviderEventType.MotionDetected => EmbodimentDomainEventType.MotionDetected,
            EmbodimentProviderEventType.PersonDetected => EmbodimentDomainEventType.PersonDetected,
            EmbodimentProviderEventType.Error => EmbodimentDomainEventType.ProviderError,
            _ => EmbodimentDomainEventType.StateChanged
        };

        Dictionary<string, object> details = new Dictionary<string, object> { ["providerId"] = providerId };
        if (providerEvent.Details != null)
        {
            foreach ((string? key, object? value) in providerEvent.Details)
            {
                details[key] = value;
            }
        }

        RaiseDomainEvent(new EmbodimentDomainEvent(domainEventType, providerEvent.Timestamp, details));
    }

    private void UpdateAggregateCapabilities()
    {
        EmbodimentCapabilities capabilities = EmbodimentCapabilities.None;
        foreach (IEmbodimentProvider? provider in _providers.Values.Where(p => p.IsConnected))
        {
            // We'd need to track capabilities per provider
            // For now, just update based on active sensors/actuators
        }

        UpdateState(_state with { Capabilities = capabilities, LastUpdatedAt = DateTime.UtcNow });
    }

    private void UpdateState(EmbodimentAggregateState newState)
    {
        _state = newState;
        RaiseDomainEvent(new EmbodimentDomainEvent(
            EmbodimentDomainEventType.StateChanged,
            DateTime.UtcNow,
            new Dictionary<string, object> { ["status"] = newState.Status.ToString() }));
    }

    private void RaiseDomainEvent(EmbodimentDomainEvent domainEvent)
    {
        _domainEvents.OnNext(domainEvent);
    }

    private static (string ProviderId, string ResourceId) ParseResourceId(string fullId)
    {
        string[] parts = fullId.Split(':', 2);
        return parts.Length == 2 ? (parts[0], parts[1]) : (fullId, fullId);
    }

    /// <summary>
    /// Disposes the aggregate and all providers.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (IDisposable sub in _subscriptions)
        {
            sub.Dispose();
        }

        foreach (IEmbodimentProvider provider in _providers.Values)
        {
            provider.Dispose();
        }

        _providers.Clear();
        _activeSensors.Clear();
        _activeActuators.Clear();

        _domainEvents.OnCompleted();
        _unifiedPerceptions.OnCompleted();

        _domainEvents.Dispose();
        _unifiedPerceptions.Dispose();
    }
}
