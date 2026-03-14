using System.Reactive.Linq;
using System.Reactive.Subjects;
using Ouroboros.Abstractions;
using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.EmbodiedInteraction;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class EmbodimentAggregateSensorsTests : IDisposable
{
    private readonly EmbodimentAggregate _sut;
    private readonly Mock<IEmbodimentProvider> _mockProvider;

    public EmbodimentAggregateSensorsTests()
    {
        _sut = new EmbodimentAggregate("test-agg", "TestAggregate");
        _mockProvider = CreateMockProvider("prov1");
        _sut.RegisterProvider(_mockProvider.Object);
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    [Fact]
    public async Task ActivateSensorAsync_UnknownProvider_ReturnsFailure()
    {
        var result = await _sut.ActivateSensorAsync("unknown:sensor1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task ActivateSensorAsync_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.ActivateSensorAsync("prov1:sensor1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public async Task ActivateSensorAsync_ProviderActivationFails_ReturnsFailure()
    {
        _mockProvider
            .Setup(p => p.ActivateSensorAsync("sensor1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Failure("sensor error"));

        var result = await _sut.ActivateSensorAsync("prov1:sensor1");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ActivateSensorAsync_ProviderReturnsSuccess_ReturnsSuccess()
    {
        var sensorInfo = new SensorInfo("sensor1", "Mic", SensorModality.Audio, true,
            EmbodimentCapabilities.AudioCapture, null);
        _mockProvider
            .Setup(p => p.ActivateSensorAsync("sensor1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success(Unit.Value));
        _mockProvider
            .Setup(p => p.GetSensorsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<SensorInfo>>.Success(
                new List<SensorInfo> { sensorInfo }));

        var result = await _sut.ActivateSensorAsync("prov1:sensor1");

        result.IsSuccess.Should().BeTrue();
        result.Value.SensorId.Should().Be("sensor1");
    }

    [Fact]
    public async Task DeactivateSensorAsync_UnknownProvider_ReturnsFailure()
    {
        var result = await _sut.DeactivateSensorAsync("unknown:sensor1");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateSensorAsync_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.DeactivateSensorAsync("prov1:sensor1");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateSensorAsync_Success_ReturnsSuccess()
    {
        _mockProvider
            .Setup(p => p.DeactivateSensorAsync("sensor1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success(Unit.Value));

        var result = await _sut.DeactivateSensorAsync("prov1:sensor1");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ReadSensorAsync_UnknownProvider_ReturnsFailure()
    {
        var result = await _sut.ReadSensorAsync("unknown:sensor1");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ReadSensorAsync_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();

        var result = await _sut.ReadSensorAsync("prov1:sensor1");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ReadSensorAsync_ValidProvider_DelegatesAndReturns()
    {
        var perceptionData = new PerceptionData("sensor1", SensorModality.Audio,
            DateTime.UtcNow, "audio data");
        _mockProvider
            .Setup(p => p.ReadSensorAsync("sensor1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PerceptionData>.Success(perceptionData));

        var result = await _sut.ReadSensorAsync("prov1:sensor1");

        result.IsSuccess.Should().BeTrue();
        result.Value.SensorId.Should().Be("sensor1");
    }

    [Fact]
    public async Task ExecuteActionAsync_UnknownProvider_ReturnsFailure()
    {
        var action = new ActuatorAction("speak", new Dictionary<string, object>());

        var result = await _sut.ExecuteActionAsync("unknown:actuator1", action);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteActionAsync_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();
        var action = new ActuatorAction("speak", new Dictionary<string, object>());

        var result = await _sut.ExecuteActionAsync("prov1:actuator1", action);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteActionAsync_Success_ReturnsOutcome()
    {
        var action = new ActuatorAction("speak", new Dictionary<string, object>());
        var outcome = new ActionOutcome("actuator1", "speak", true, TimeSpan.FromMilliseconds(50));
        _mockProvider
            .Setup(p => p.ExecuteActionAsync("actuator1", action, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ActionOutcome>.Success(outcome));

        var result = await _sut.ExecuteActionAsync("prov1:actuator1", action);

        result.IsSuccess.Should().BeTrue();
        result.Value.Success.Should().BeTrue();
    }

    [Fact]
    public void ToBodySchema_EmptyAggregate_HasBaseCapabilities()
    {
        var schema = _sut.ToBodySchema();

        schema.Should().NotBeNull();
    }

    private static Mock<IEmbodimentProvider> CreateMockProvider(string id)
    {
        var mock = new Mock<IEmbodimentProvider>();
        mock.Setup(p => p.ProviderId).Returns(id);
        mock.Setup(p => p.ProviderName).Returns($"Provider-{id}");
        mock.Setup(p => p.IsConnected).Returns(true);
        using var perceptionSubject = new Subject<PerceptionData>();
        using var eventSubject = new Subject<EmbodimentProviderEvent>();
        mock.Setup(p => p.Perceptions)
            .Returns(perceptionSubject.AsObservable());
        mock.Setup(p => p.Events)
            .Returns(eventSubject.AsObservable());
        return mock;
    }
}
