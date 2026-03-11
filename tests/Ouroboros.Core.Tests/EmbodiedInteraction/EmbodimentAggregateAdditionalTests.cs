using System.Reactive.Linq;
using System.Reactive.Subjects;
using Ouroboros.Abstractions;
using Ouroboros.Core.EmbodiedInteraction;
using Ouroboros.Core.Monads;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class EmbodimentAggregateAdditionalTests : IDisposable
{
    private readonly EmbodimentAggregate _sut;

    public EmbodimentAggregateAdditionalTests()
    {
        _sut = new EmbodimentAggregate("agg-1", "TestAggregate");
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    // ========================================================================
    // RegisterProvider - domain events and perception forwarding
    // ========================================================================

    [Fact]
    public void RegisterProvider_RaisesProviderRegisteredEvent()
    {
        var provider = CreateMockProvider("p1");
        EmbodimentDomainEvent? captured = null;
        _sut.DomainEvents.Subscribe(e =>
        {
            if (e.EventType == EmbodimentDomainEventType.ProviderRegistered)
                captured = e;
        });

        _sut.RegisterProvider(provider.Object);

        captured.Should().NotBeNull();
        captured!.Details.Should().ContainKey("providerId");
        captured.Details["providerId"].Should().Be("p1");
    }

    [Fact]
    public void RegisterProvider_ForwardsProviderPerceptionsToUnifiedStream()
    {
        var perceptionSubject = new Subject<PerceptionData>();
        var provider = CreateMockProviderWithSubjects("p1", perceptionSubject, new Subject<EmbodimentProviderEvent>());
        _sut.RegisterProvider(provider.Object);

        PerceptionData? received = null;
        _sut.UnifiedPerceptions.Subscribe(p => received = p);

        var perception = new PerceptionData("sensor1", SensorModality.Audio, DateTime.UtcNow, new byte[] { 1 });
        perceptionSubject.OnNext(perception);

        received.Should().NotBeNull();
        received!.SensorId.Should().Be("sensor1");
    }

    [Fact]
    public void RegisterProvider_ForwardsProviderPerceptions_RaisesPerceptionReceivedEvent()
    {
        var perceptionSubject = new Subject<PerceptionData>();
        var provider = CreateMockProviderWithSubjects("p1", perceptionSubject, new Subject<EmbodimentProviderEvent>());
        _sut.RegisterProvider(provider.Object);

        EmbodimentDomainEvent? captured = null;
        _sut.DomainEvents.Subscribe(e =>
        {
            if (e.EventType == EmbodimentDomainEventType.PerceptionReceived)
                captured = e;
        });

        var perception = new PerceptionData("sensor1", SensorModality.Visual, DateTime.UtcNow, new byte[] { 1 });
        perceptionSubject.OnNext(perception);

        captured.Should().NotBeNull();
        captured!.Details["modality"].Should().Be("Visual");
    }

    // ========================================================================
    // HandleProviderEvent - event type mapping
    // ========================================================================

    [Theory]
    [InlineData(EmbodimentProviderEventType.Connected, EmbodimentDomainEventType.ProviderConnected)]
    [InlineData(EmbodimentProviderEventType.Disconnected, EmbodimentDomainEventType.ProviderDisconnected)]
    [InlineData(EmbodimentProviderEventType.MotionDetected, EmbodimentDomainEventType.MotionDetected)]
    [InlineData(EmbodimentProviderEventType.PersonDetected, EmbodimentDomainEventType.PersonDetected)]
    [InlineData(EmbodimentProviderEventType.Error, EmbodimentDomainEventType.ProviderError)]
    public void HandleProviderEvent_MapsEventTypesCorrectly(
        EmbodimentProviderEventType providerEventType,
        EmbodimentDomainEventType expectedDomainEventType)
    {
        var eventSubject = new Subject<EmbodimentProviderEvent>();
        var provider = CreateMockProviderWithSubjects("p1", new Subject<PerceptionData>(), eventSubject);
        _sut.RegisterProvider(provider.Object);

        EmbodimentDomainEvent? captured = null;
        _sut.DomainEvents.Subscribe(e =>
        {
            if (e.EventType == expectedDomainEventType)
                captured = e;
        });

        var providerEvent = new EmbodimentProviderEvent(providerEventType, DateTime.UtcNow, null);
        eventSubject.OnNext(providerEvent);

        captured.Should().NotBeNull();
        captured!.EventType.Should().Be(expectedDomainEventType);
    }

    [Fact]
    public void HandleProviderEvent_UnknownType_MapsToStateChanged()
    {
        var eventSubject = new Subject<EmbodimentProviderEvent>();
        var provider = CreateMockProviderWithSubjects("p1", new Subject<PerceptionData>(), eventSubject);
        _sut.RegisterProvider(provider.Object);

        var capturedEvents = new List<EmbodimentDomainEvent>();
        _sut.DomainEvents.Subscribe(e => capturedEvents.Add(e));

        // Use a default/fallback value for an unrecognized event type
        var providerEvent = new EmbodimentProviderEvent((EmbodimentProviderEventType)999, DateTime.UtcNow, null);
        eventSubject.OnNext(providerEvent);

        capturedEvents.Should().Contain(e => e.EventType == EmbodimentDomainEventType.StateChanged);
    }

    [Fact]
    public void HandleProviderEvent_WithDetails_MergesDetailsIntoDomainEvent()
    {
        var eventSubject = new Subject<EmbodimentProviderEvent>();
        var provider = CreateMockProviderWithSubjects("p1", new Subject<PerceptionData>(), eventSubject);
        _sut.RegisterProvider(provider.Object);

        EmbodimentDomainEvent? captured = null;
        _sut.DomainEvents.Subscribe(e =>
        {
            if (e.EventType == EmbodimentDomainEventType.MotionDetected)
                captured = e;
        });

        var details = new Dictionary<string, object> { ["region"] = "entrance" };
        var providerEvent = new EmbodimentProviderEvent(
            EmbodimentProviderEventType.MotionDetected, DateTime.UtcNow, details);
        eventSubject.OnNext(providerEvent);

        captured.Should().NotBeNull();
        captured!.Details.Should().ContainKey("region");
        captured.Details["region"].Should().Be("entrance");
        captured.Details.Should().ContainKey("providerId");
    }

    // ========================================================================
    // UnregisterProviderAsync
    // ========================================================================

    [Fact]
    public async Task UnregisterProviderAsync_WhenDisposed_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.UnregisterProviderAsync("p1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public async Task UnregisterProviderAsync_ValidProvider_DisconnectsAndDisposesProvider()
    {
        var provider = CreateMockProvider("p1");
        provider.Setup(p => p.DisconnectAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _sut.RegisterProvider(provider.Object);

        var result = await _sut.UnregisterProviderAsync("p1");

        result.IsSuccess.Should().BeTrue();
        provider.Verify(p => p.DisconnectAsync(It.IsAny<CancellationToken>()), Times.Once);
        provider.Verify(p => p.Dispose(), Times.Once);
        _sut.Providers.Should().NotContainKey("p1");
    }

    [Fact]
    public async Task UnregisterProviderAsync_RemovesSensorsAndActuatorsWithPrefix()
    {
        var provider = CreateMockProvider("p1");
        provider.Setup(p => p.DisconnectAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        provider.Setup(p => p.ConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EmbodimentCapabilities>.Success(EmbodimentCapabilities.AudioCapture));

        var sensor = new SensorInfo("mic1", "Mic", SensorModality.Audio, true, EmbodimentCapabilities.AudioCapture);
        var actuator = new ActuatorInfo("speaker1", "Speaker", ActuatorModality.Voice, true, EmbodimentCapabilities.Speech);
        provider.Setup(p => p.GetSensorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<SensorInfo>>.Success(new List<SensorInfo> { sensor }));
        provider.Setup(p => p.GetActuatorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ActuatorInfo>>.Success(new List<ActuatorInfo> { actuator }));

        _sut.RegisterProvider(provider.Object);
        await _sut.ActivateAsync();

        _sut.ActiveSensors.Should().ContainKey("p1:mic1");
        _sut.ActiveActuators.Should().ContainKey("p1:speaker1");

        await _sut.UnregisterProviderAsync("p1");

        _sut.ActiveSensors.Should().NotContainKey("p1:mic1");
        _sut.ActiveActuators.Should().NotContainKey("p1:speaker1");
    }

    [Fact]
    public async Task UnregisterProviderAsync_RaisesProviderUnregisteredEvent()
    {
        var provider = CreateMockProvider("p1");
        provider.Setup(p => p.DisconnectAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _sut.RegisterProvider(provider.Object);

        EmbodimentDomainEvent? captured = null;
        _sut.DomainEvents.Subscribe(e =>
        {
            if (e.EventType == EmbodimentDomainEventType.ProviderUnregistered)
                captured = e;
        });

        await _sut.UnregisterProviderAsync("p1");

        captured.Should().NotBeNull();
    }

    // ========================================================================
    // ActivateAsync with providers
    // ========================================================================

    [Fact]
    public async Task ActivateAsync_WithProvider_ConnectsAndLoadsResources()
    {
        var provider = CreateMockProvider("p1");
        provider.Setup(p => p.ConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EmbodimentCapabilities>.Success(
                EmbodimentCapabilities.AudioCapture | EmbodimentCapabilities.Speech));

        var sensor = new SensorInfo("mic1", "Mic", SensorModality.Audio, true, EmbodimentCapabilities.AudioCapture);
        provider.Setup(p => p.GetSensorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<SensorInfo>>.Success(new List<SensorInfo> { sensor }));
        provider.Setup(p => p.GetActuatorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ActuatorInfo>>.Success(new List<ActuatorInfo>()));

        _sut.RegisterProvider(provider.Object);

        var result = await _sut.ActivateAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveFlag(EmbodimentCapabilities.AudioCapture);
        _sut.State.Status.Should().Be(AggregateStatus.Active);
        _sut.ActiveSensors.Should().ContainKey("p1:mic1");
    }

    [Fact]
    public async Task ActivateAsync_AllProvidersFail_ReturnsFailure()
    {
        var provider = CreateMockProvider("p1");
        provider.Setup(p => p.ConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EmbodimentCapabilities>.Failure("connection failed"));
        _sut.RegisterProvider(provider.Object);

        var result = await _sut.ActivateAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("All providers failed");
        _sut.State.Status.Should().Be(AggregateStatus.Failed);
    }

    [Fact]
    public async Task ActivateAsync_SomeProvidersFail_StillActivates()
    {
        var p1 = CreateMockProvider("p1");
        p1.Setup(p => p.ConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EmbodimentCapabilities>.Success(EmbodimentCapabilities.AudioCapture));
        p1.Setup(p => p.GetSensorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<SensorInfo>>.Success(new List<SensorInfo>()));
        p1.Setup(p => p.GetActuatorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ActuatorInfo>>.Success(new List<ActuatorInfo>()));

        var p2 = CreateMockProvider("p2");
        p2.Setup(p => p.ConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EmbodimentCapabilities>.Failure("timeout"));

        _sut.RegisterProvider(p1.Object);
        _sut.RegisterProvider(p2.Object);

        var result = await _sut.ActivateAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.State.Status.Should().Be(AggregateStatus.Active);
    }

    [Fact]
    public async Task ActivateAsync_ProviderThrowsException_TreatsAsError()
    {
        var provider = CreateMockProvider("p1");
        provider.Setup(p => p.ConnectAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("hardware fault"));
        _sut.RegisterProvider(provider.Object);

        var result = await _sut.ActivateAsync();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ActivateAsync_WhenDisposed_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.ActivateAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public async Task ActivateAsync_RaisesAggregateActivatedEvent()
    {
        EmbodimentDomainEvent? captured = null;
        _sut.DomainEvents.Subscribe(e =>
        {
            if (e.EventType == EmbodimentDomainEventType.AggregateActivated)
                captured = e;
        });

        await _sut.ActivateAsync();

        captured.Should().NotBeNull();
        captured!.Details.Should().ContainKey("capabilities");
    }

    // ========================================================================
    // DeactivateAsync
    // ========================================================================

    [Fact]
    public async Task DeactivateAsync_WhenDisposed_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.DeactivateAsync();

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateAsync_WhenActive_DisconnectsAllProvidersAndClearsSensors()
    {
        var provider = CreateMockProvider("p1");
        provider.Setup(p => p.ConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EmbodimentCapabilities>.Success(EmbodimentCapabilities.AudioCapture));

        var sensor = new SensorInfo("mic1", "Mic", SensorModality.Audio, true, EmbodimentCapabilities.AudioCapture);
        provider.Setup(p => p.GetSensorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<SensorInfo>>.Success(new List<SensorInfo> { sensor }));
        provider.Setup(p => p.GetActuatorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ActuatorInfo>>.Success(new List<ActuatorInfo>()));
        provider.Setup(p => p.DisconnectAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut.RegisterProvider(provider.Object);
        await _sut.ActivateAsync();

        var result = await _sut.DeactivateAsync();

        result.IsSuccess.Should().BeTrue();
        _sut.State.Status.Should().Be(AggregateStatus.Inactive);
        _sut.State.Capabilities.Should().Be(EmbodimentCapabilities.None);
        _sut.ActiveSensors.Should().BeEmpty();
        _sut.ActiveActuators.Should().BeEmpty();
        provider.Verify(p => p.DisconnectAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_RaisesAggregateDeactivatedEvent()
    {
        await _sut.ActivateAsync();

        EmbodimentDomainEvent? captured = null;
        _sut.DomainEvents.Subscribe(e =>
        {
            if (e.EventType == EmbodimentDomainEventType.AggregateDeactivated)
                captured = e;
        });

        await _sut.DeactivateAsync();

        captured.Should().NotBeNull();
    }

    // ========================================================================
    // LoadProviderResourcesAsync - inactive sensors/actuators not added
    // ========================================================================

    [Fact]
    public async Task ActivateAsync_InactiveSensor_NotAddedToActiveSensors()
    {
        var provider = CreateMockProvider("p1");
        provider.Setup(p => p.ConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EmbodimentCapabilities>.Success(EmbodimentCapabilities.None));

        var inactiveSensor = new SensorInfo("mic1", "Mic", SensorModality.Audio, false, EmbodimentCapabilities.AudioCapture);
        provider.Setup(p => p.GetSensorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<SensorInfo>>.Success(new List<SensorInfo> { inactiveSensor }));
        provider.Setup(p => p.GetActuatorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ActuatorInfo>>.Success(new List<ActuatorInfo>()));

        _sut.RegisterProvider(provider.Object);
        await _sut.ActivateAsync();

        _sut.ActiveSensors.Should().NotContainKey("p1:mic1");
    }

    [Fact]
    public async Task ActivateAsync_InactiveActuator_NotAddedToActiveActuators()
    {
        var provider = CreateMockProvider("p1");
        provider.Setup(p => p.ConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EmbodimentCapabilities>.Success(EmbodimentCapabilities.None));
        provider.Setup(p => p.GetSensorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<SensorInfo>>.Success(new List<SensorInfo>()));

        var inactiveActuator = new ActuatorInfo("spk1", "Speaker", ActuatorModality.Voice, false, EmbodimentCapabilities.Speech);
        provider.Setup(p => p.GetActuatorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ActuatorInfo>>.Success(new List<ActuatorInfo> { inactiveActuator }));

        _sut.RegisterProvider(provider.Object);
        await _sut.ActivateAsync();

        _sut.ActiveActuators.Should().NotContainKey("p1:spk1");
    }

    // ========================================================================
    // Helpers
    // ========================================================================

    private static Mock<IEmbodimentProvider> CreateMockProvider(string providerId)
    {
        var mock = new Mock<IEmbodimentProvider>();
        mock.Setup(p => p.ProviderId).Returns(providerId);
        mock.Setup(p => p.ProviderName).Returns($"Mock {providerId}");
        mock.Setup(p => p.Perceptions).Returns(new Subject<PerceptionData>().AsObservable());
        mock.Setup(p => p.Events).Returns(new Subject<EmbodimentProviderEvent>().AsObservable());
        return mock;
    }

    private static Mock<IEmbodimentProvider> CreateMockProviderWithSubjects(
        string providerId,
        Subject<PerceptionData> perceptionSubject,
        Subject<EmbodimentProviderEvent> eventSubject)
    {
        var mock = new Mock<IEmbodimentProvider>();
        mock.Setup(p => p.ProviderId).Returns(providerId);
        mock.Setup(p => p.ProviderName).Returns($"Mock {providerId}");
        mock.Setup(p => p.Perceptions).Returns(perceptionSubject.AsObservable());
        mock.Setup(p => p.Events).Returns(eventSubject.AsObservable());
        return mock;
    }
}
