using System.Reactive.Subjects;
using Ouroboros.Abstractions;
using Ouroboros.Core.EmbodiedInteraction;
using Ouroboros.Core.Monads;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

/// <summary>
/// Additional tests for EmbodimentAggregate.Sensors partial class covering
/// body schema with actuators, text sensors, and missing provider scenarios.
/// </summary>
[Trait("Category", "Unit")]
public class EmbodimentAggregateSensorAdditionalTests : IDisposable
{
    private readonly EmbodimentAggregate _aggregate;
    private readonly Mock<IEmbodimentProvider> _mockProvider;
    private readonly Subject<PerceptionData> _perceptionSubject;
    private readonly Subject<EmbodimentProviderEvent> _eventSubject;

    public EmbodimentAggregateSensorAdditionalTests()
    {
        _aggregate = new EmbodimentAggregate("test-aggregate", "TestAggregate");
        _perceptionSubject = new Subject<PerceptionData>();
        _eventSubject = new Subject<EmbodimentProviderEvent>();

        _mockProvider = new Mock<IEmbodimentProvider>();
        _mockProvider.Setup(p => p.ProviderId).Returns("provider1");
        _mockProvider.Setup(p => p.ProviderName).Returns("TestProvider");
        _mockProvider.Setup(p => p.Perceptions).Returns(_perceptionSubject);
        _mockProvider.Setup(p => p.Events).Returns(_eventSubject);
    }

    public void Dispose()
    {
        _perceptionSubject.Dispose();
        _eventSubject.Dispose();
        _aggregate.Dispose();
    }

    // ========================================================================
    // ActivateSensorAsync - GetSensorsAsync returns failure
    // ========================================================================

    [Fact]
    public async Task ActivateSensorAsync_GetSensorsAsyncFails_ReturnsFailure()
    {
        _aggregate.RegisterProvider(_mockProvider.Object);

        _mockProvider
            .Setup(p => p.ActivateSensorAsync("sensor1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success(Unit.Value));
        _mockProvider
            .Setup(p => p.GetSensorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<SensorInfo>>.Failure("could not list sensors"));

        var result = await _aggregate.ActivateSensorAsync("provider1:sensor1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found in provider");
    }

    // ========================================================================
    // DeactivateSensorAsync - provider failure propagation
    // ========================================================================

    [Fact]
    public async Task DeactivateSensorAsync_ProviderFails_PropagatesFailure()
    {
        _aggregate.RegisterProvider(_mockProvider.Object);

        _mockProvider
            .Setup(p => p.DeactivateSensorAsync("sensor1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Failure("hardware error"));

        var result = await _aggregate.DeactivateSensorAsync("provider1:sensor1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("hardware error");
    }

    [Fact]
    public async Task DeactivateSensorAsync_Success_RaisesSensorDeactivatedEvent()
    {
        _aggregate.RegisterProvider(_mockProvider.Object);

        _mockProvider
            .Setup(p => p.DeactivateSensorAsync("sensor1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success(Unit.Value));

        EmbodimentDomainEvent? captured = null;
        _aggregate.DomainEvents.Subscribe(e =>
        {
            if (e.EventType == EmbodimentDomainEventType.SensorDeactivated)
                captured = e;
        });

        await _aggregate.DeactivateSensorAsync("provider1:sensor1");

        captured.Should().NotBeNull();
        captured!.Details["sensorId"].Should().Be("provider1:sensor1");
    }

    // ========================================================================
    // ReadSensorAsync - success delegates to provider
    // ========================================================================

    [Fact]
    public async Task ReadSensorAsync_ParsesResourceIdCorrectly()
    {
        _aggregate.RegisterProvider(_mockProvider.Object);

        var perception = new PerceptionData("sensor1", SensorModality.Audio, DateTime.UtcNow, new byte[] { 1 });
        _mockProvider
            .Setup(p => p.ReadSensorAsync("sensor1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PerceptionData>.Success(perception));

        var result = await _aggregate.ReadSensorAsync("provider1:sensor1");

        result.IsSuccess.Should().BeTrue();
        _mockProvider.Verify(p => p.ReadSensorAsync("sensor1", It.IsAny<CancellationToken>()), Times.Once);
    }

    // ========================================================================
    // ExecuteActionAsync - failure does not raise event
    // ========================================================================

    [Fact]
    public async Task ExecuteActionAsync_Failure_DoesNotRaiseActionExecutedEvent()
    {
        _aggregate.RegisterProvider(_mockProvider.Object);

        var action = new ActuatorAction("speak", new Dictionary<string, object> { ["text"] = "hello" });
        _mockProvider
            .Setup(p => p.ExecuteActionAsync("actuator1", action, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ActionOutcome>.Failure("actuator offline"));

        EmbodimentDomainEvent? captured = null;
        _aggregate.DomainEvents.Subscribe(e =>
        {
            if (e.EventType == EmbodimentDomainEventType.ActionExecuted)
                captured = e;
        });

        var result = await _aggregate.ExecuteActionAsync("provider1:actuator1", action);

        result.IsFailure.Should().BeTrue();
        captured.Should().BeNull();
    }

    // ========================================================================
    // ToBodySchema with various sensor/actuator modalities
    // ========================================================================

    [Fact]
    public async Task ToBodySchema_WithTextSensor_IncludesReadingCapability()
    {
        _aggregate.RegisterProvider(_mockProvider.Object);

        var textSensor = new SensorInfo("text1", "TextInput", SensorModality.Text, true,
            EmbodimentCapabilities.None);
        _mockProvider
            .Setup(p => p.ActivateSensorAsync("text1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success(Unit.Value));
        _mockProvider
            .Setup(p => p.GetSensorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<SensorInfo>>.Success(
                new List<SensorInfo> { textSensor }));

        await _aggregate.ActivateSensorAsync("provider1:text1");

        var schema = _aggregate.ToBodySchema();

        schema.Sensors.Should().Contain(s => s.Modality == SensorModality.Text);
    }

    [Fact]
    public async Task ToBodySchema_WithVoiceActuator_IncludesSpeakingCapability()
    {
        _aggregate.RegisterProvider(_mockProvider.Object);

        var voiceActuator = new ActuatorInfo("voice1", "Voice", ActuatorModality.Voice, true,
            EmbodimentCapabilities.Speech);

        _mockProvider
            .Setup(p => p.ConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EmbodimentCapabilities>.Success(EmbodimentCapabilities.Speech));
        _mockProvider
            .Setup(p => p.GetSensorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<SensorInfo>>.Success(new List<SensorInfo>()));
        _mockProvider
            .Setup(p => p.GetActuatorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ActuatorInfo>>.Success(
                new List<ActuatorInfo> { voiceActuator }));

        await _aggregate.ActivateAsync();

        var schema = _aggregate.ToBodySchema();

        schema.Actuators.Should().Contain(a => a.Modality == ActuatorModality.Voice);
    }

    [Fact]
    public async Task ToBodySchema_WithTextActuator_IncludesWritingCapability()
    {
        _aggregate.RegisterProvider(_mockProvider.Object);

        var textActuator = new ActuatorInfo("text1", "TextOutput", ActuatorModality.Text, true,
            EmbodimentCapabilities.None);

        _mockProvider
            .Setup(p => p.ConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EmbodimentCapabilities>.Success(EmbodimentCapabilities.None));
        _mockProvider
            .Setup(p => p.GetSensorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<SensorInfo>>.Success(new List<SensorInfo>()));
        _mockProvider
            .Setup(p => p.GetActuatorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ActuatorInfo>>.Success(
                new List<ActuatorInfo> { textActuator }));

        await _aggregate.ActivateAsync();

        var schema = _aggregate.ToBodySchema();

        schema.Actuators.Should().Contain(a => a.Modality == ActuatorModality.Text);
    }

    // ========================================================================
    // ParseResourceId - single-part ID
    // ========================================================================

    [Fact]
    public async Task ActivateSensorAsync_SinglePartId_UsesIdAsBothProviderAndResource()
    {
        // When a sensor ID doesn't contain ":", ParseResourceId returns (fullId, fullId)
        // This means provider lookup will fail since "sensor1" is not a registered provider
        var result = await _aggregate.ActivateSensorAsync("sensor1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }
}
