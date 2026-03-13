namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class CompositeMessageFilterTests
{
    private static NeuronMessage CreateTestMessage(string topic = "test.topic", object? payload = null)
    {
        return new NeuronMessage
        {
            SourceNeuron = "source",
            Topic = topic,
            Payload = payload ?? "test"
        };
    }

    [Fact]
    public void Constructor_NullFilters_ThrowsArgumentNull()
    {
        var act = () => new CompositeMessageFilter(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ShouldRouteAsync_EmptyFilters_ReturnsTrue()
    {
        var filter = new CompositeMessageFilter(new List<IMessageFilter>());
        var message = CreateTestMessage();

        bool result = await filter.ShouldRouteAsync(message);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldRouteAsync_AllFiltersAllow_ReturnsTrue()
    {
        var f1 = new Mock<IMessageFilter>();
        f1.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var f2 = new Mock<IMessageFilter>();
        f2.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var composite = new CompositeMessageFilter(new List<IMessageFilter> { f1.Object, f2.Object });
        var message = CreateTestMessage();

        bool result = await composite.ShouldRouteAsync(message);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldRouteAsync_OneFilterBlocks_ReturnsFalse()
    {
        var f1 = new Mock<IMessageFilter>();
        f1.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var f2 = new Mock<IMessageFilter>();
        f2.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var composite = new CompositeMessageFilter(new List<IMessageFilter> { f1.Object, f2.Object });
        var message = CreateTestMessage();

        bool result = await composite.ShouldRouteAsync(message);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldRouteAsync_FirstFilterBlocks_ShortCircuits()
    {
        var f1 = new Mock<IMessageFilter>();
        f1.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var f2 = new Mock<IMessageFilter>();
        f2.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var composite = new CompositeMessageFilter(new List<IMessageFilter> { f1.Object, f2.Object });
        var message = CreateTestMessage();

        await composite.ShouldRouteAsync(message);

        // f2 should not have been called since f1 blocked
        f2.Verify(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ShouldRouteAsync_SingleFilter_DelegatesCorrectly()
    {
        var filter = new Mock<IMessageFilter>();
        filter.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var composite = new CompositeMessageFilter(new List<IMessageFilter> { filter.Object });
        var message = CreateTestMessage();

        bool result = await composite.ShouldRouteAsync(message);

        result.Should().BeTrue();
        filter.Verify(f => f.ShouldRouteAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }
}
