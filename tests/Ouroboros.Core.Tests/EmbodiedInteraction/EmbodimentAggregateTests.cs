using System.Reactive.Linq;
using System.Reactive.Subjects;
using Ouroboros.Abstractions;
using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.EmbodiedInteraction;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class EmbodimentAggregateTests : IDisposable
{
    private readonly EmbodimentAggregate _sut;

    public EmbodimentAggregateTests()
    {
        _sut = new EmbodimentAggregate("test-aggregate", "TestAggregate");
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    [Fact]
    public void Constructor_SetsProperties()
    {
        _sut.AggregateId.Should().Be("test-aggregate");
        _sut.Name.Should().Be("TestAggregate");
        _sut.State.Status.Should().Be(AggregateStatus.Inactive);
        _sut.State.Capabilities.Should().Be(EmbodimentCapabilities.None);
    }

    [Fact]
    public void Constructor_NullId_ThrowsArgumentNullException()
    {
        Action act = () => new EmbodimentAggregate(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_DefaultName_UsesDefault()
    {
        using var aggregate = new EmbodimentAggregate("id");

        aggregate.Name.Should().Be("EmbodimentAggregate");
    }

    [Fact]
    public void RegisterProvider_NullProvider_ReturnsFailure()
    {
        var result = _sut.RegisterProvider(null!);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void RegisterProvider_ValidProvider_ReturnsSuccess()
    {
        var provider = CreateMockProvider("provider1");

        var result = _sut.RegisterProvider(provider.Object);

        result.IsSuccess.Should().BeTrue();
        _sut.Providers.Should().ContainKey("provider1");
    }

    [Fact]
    public void RegisterProvider_DuplicateProvider_ReturnsFailure()
    {
        var provider = CreateMockProvider("provider1");
        _sut.RegisterProvider(provider.Object);

        var result = _sut.RegisterProvider(provider.Object);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void RegisterProvider_AfterDispose_ReturnsFailure()
    {
        _sut.Dispose();
        var provider = CreateMockProvider("p1");

        var result = _sut.RegisterProvider(provider.Object);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UnregisterProviderAsync_UnknownProvider_ReturnsFailure()
    {
        var result = await _sut.UnregisterProviderAsync("unknown").ConfigureAwait(false);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ActivateAsync_NoProviders_ReturnsSuccess()
    {
        var result = await _sut.ActivateAsync().ConfigureAwait(false);

        result.IsSuccess.Should().BeTrue();
        _sut.State.Status.Should().Be(AggregateStatus.Active);
    }

    [Fact]
    public async Task ActivateAsync_AlreadyActive_ReturnsCachedCapabilities()
    {
        await _sut.ActivateAsync().ConfigureAwait(false);

        var result = await _sut.ActivateAsync().ConfigureAwait(false);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateAsync_WhenActive_ReturnsSuccess()
    {
        await _sut.ActivateAsync().ConfigureAwait(false);

        var result = await _sut.DeactivateAsync().ConfigureAwait(false);

        result.IsSuccess.Should().BeTrue();
        _sut.State.Status.Should().Be(AggregateStatus.Inactive);
    }

    [Fact]
    public async Task DeactivateAsync_AlreadyInactive_ReturnsSuccess()
    {
        var result = await _sut.DeactivateAsync().ConfigureAwait(false);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToBodySchema_WithNoSensors_ReturnsBaseSchema()
    {
        var schema = _sut.ToBodySchema();

        schema.Should().NotBeNull();
    }

    [Fact]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        _sut.Dispose();

        Action act = () => _sut.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void DomainEvents_IsObservable()
    {
        _sut.DomainEvents.Should().NotBeNull();
    }

    [Fact]
    public void UnifiedPerceptions_IsObservable()
    {
        _sut.UnifiedPerceptions.Should().NotBeNull();
    }

    [Fact]
    public async Task ActivateSensorAsync_UnknownProvider_ReturnsFailure()
    {
        var result = await _sut.ActivateSensorAsync("unknown:sensor1").ConfigureAwait(false);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateSensorAsync_UnknownProvider_ReturnsFailure()
    {
        var result = await _sut.DeactivateSensorAsync("unknown:sensor1").ConfigureAwait(false);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ReadSensorAsync_UnknownProvider_ReturnsFailure()
    {
        var result = await _sut.ReadSensorAsync("unknown:sensor1").ConfigureAwait(false);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteActionAsync_UnknownProvider_ReturnsFailure()
    {
        var action = new ActuatorAction("test", new Dictionary<string, object>());
        var result = await _sut.ExecuteActionAsync("unknown:actuator1", action).ConfigureAwait(false);

        result.IsFailure.Should().BeTrue();
    }

    private static Mock<IEmbodimentProvider> CreateMockProvider(string providerId)
    {
        var mock = new Mock<IEmbodimentProvider>();
        mock.Setup(p => p.ProviderId).Returns(providerId);
        mock.Setup(p => p.ProviderName).Returns($"Mock {providerId}");
        using var perceptionSubject = new Subject<PerceptionData>();
        using var eventSubject = new Subject<EmbodimentProviderEvent>();
        mock.Setup(p => p.Perceptions).Returns(perceptionSubject.AsObservable());
        mock.Setup(p => p.Events).Returns(eventSubject.AsObservable());
        return mock;
    }
}
