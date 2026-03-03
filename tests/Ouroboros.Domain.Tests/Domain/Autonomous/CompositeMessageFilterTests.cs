namespace Ouroboros.Tests.Domain.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class CompositeMessageFilterTests
{
    private static NeuronMessage CreateMessage(string topic = "test.topic") => new()
    {
        SourceNeuron = "source",
        Topic = topic,
        Payload = "payload"
    };

    [Fact]
    public async Task ShouldRouteAsync_AllFiltersApprove_ReturnsTrue()
    {
        // Arrange
        var filter1 = new Mock<IMessageFilter>();
        filter1.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var filter2 = new Mock<IMessageFilter>();
        filter2.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = new CompositeMessageFilter(new List<IMessageFilter> { filter1.Object, filter2.Object });

        // Act
        bool result = await sut.ShouldRouteAsync(CreateMessage());

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldRouteAsync_FirstFilterRejects_ReturnsFalse()
    {
        // Arrange
        var filter1 = new Mock<IMessageFilter>();
        filter1.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var filter2 = new Mock<IMessageFilter>();
        filter2.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = new CompositeMessageFilter(new List<IMessageFilter> { filter1.Object, filter2.Object });

        // Act
        bool result = await sut.ShouldRouteAsync(CreateMessage());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldRouteAsync_LastFilterRejects_ReturnsFalse()
    {
        // Arrange
        var filter1 = new Mock<IMessageFilter>();
        filter1.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var filter2 = new Mock<IMessageFilter>();
        filter2.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = new CompositeMessageFilter(new List<IMessageFilter> { filter1.Object, filter2.Object });

        // Act
        bool result = await sut.ShouldRouteAsync(CreateMessage());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldRouteAsync_EmptyFilters_ReturnsTrue()
    {
        // Arrange
        var sut = new CompositeMessageFilter(new List<IMessageFilter>());

        // Act
        bool result = await sut.ShouldRouteAsync(CreateMessage());

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldRouteAsync_ShortCircuits_WhenFirstFilterRejects()
    {
        // Arrange
        var filter1 = new Mock<IMessageFilter>();
        filter1.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var filter2 = new Mock<IMessageFilter>();

        var sut = new CompositeMessageFilter(new List<IMessageFilter> { filter1.Object, filter2.Object });

        // Act
        await sut.ShouldRouteAsync(CreateMessage());

        // Assert - filter2 should never be called
        filter2.Verify(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Constructor_NullFilters_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CompositeMessageFilter(null!));
    }

    [Fact]
    public async Task ShouldRouteAsync_PassesMessageToFilters()
    {
        // Arrange
        var message = CreateMessage("specific.topic");
        NeuronMessage? capturedMessage = null;

        var filter = new Mock<IMessageFilter>();
        filter.Setup(f => f.ShouldRouteAsync(It.IsAny<NeuronMessage>(), It.IsAny<CancellationToken>()))
            .Callback<NeuronMessage, CancellationToken>((m, _) => capturedMessage = m)
            .ReturnsAsync(true);

        var sut = new CompositeMessageFilter(new List<IMessageFilter> { filter.Object });

        // Act
        await sut.ShouldRouteAsync(message);

        // Assert
        capturedMessage.Should().NotBeNull();
        capturedMessage!.Topic.Should().Be("specific.topic");
    }
}
