using Ouroboros.Domain.MultiAgent;

namespace Ouroboros.Tests.MultiAgent;

[Trait("Category", "Unit")]
public class MessageTests
{
    private static AgentId CreateAgentId(string name = "agent") => new(Guid.NewGuid(), name);

    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var sender = CreateAgentId("sender");
        var recipient = CreateAgentId("recipient");
        var timestamp = DateTime.UtcNow;
        var conversationId = Guid.NewGuid();

        var message = new Message(sender, recipient, MessageType.Query, "Hello", timestamp, conversationId);

        message.Sender.Should().Be(sender);
        message.Recipient.Should().Be(recipient);
        message.Type.Should().Be(MessageType.Query);
        message.Payload.Should().Be("Hello");
        message.Timestamp.Should().Be(timestamp);
        message.ConversationId.Should().Be(conversationId);
    }

    [Fact]
    public void Constructor_NullRecipient_ForBroadcast_ShouldWork()
    {
        var message = new Message(
            CreateAgentId(), null, MessageType.Notification, "broadcast",
            DateTime.UtcNow, Guid.NewGuid());

        message.Recipient.Should().BeNull();
    }

    [Theory]
    [InlineData(MessageType.Query)]
    [InlineData(MessageType.Answer)]
    [InlineData(MessageType.Proposal)]
    [InlineData(MessageType.Vote)]
    [InlineData(MessageType.Knowledge)]
    [InlineData(MessageType.Request)]
    [InlineData(MessageType.Notification)]
    [InlineData(MessageType.Heartbeat)]
    public void Constructor_AllMessageTypes_ShouldWork(MessageType type)
    {
        var message = new Message(CreateAgentId(), CreateAgentId(), type, "payload",
            DateTime.UtcNow, Guid.NewGuid());

        message.Type.Should().Be(type);
    }

    [Fact]
    public void RecordEquality_SameValues_ShouldBeEqual()
    {
        var sender = CreateAgentId();
        var recipient = CreateAgentId();
        var timestamp = DateTime.UtcNow;
        var convId = Guid.NewGuid();

        var m1 = new Message(sender, recipient, MessageType.Query, "Hi", timestamp, convId);
        var m2 = new Message(sender, recipient, MessageType.Query, "Hi", timestamp, convId);

        m1.Should().Be(m2);
    }

    [Fact]
    public void Constructor_ComplexPayload_ShouldSetPayload()
    {
        var payload = new Dictionary<string, object> { ["key"] = "value" };

        var message = new Message(CreateAgentId(), null, MessageType.Knowledge, payload,
            DateTime.UtcNow, Guid.NewGuid());

        message.Payload.Should().BeOfType<Dictionary<string, object>>();
    }
}
