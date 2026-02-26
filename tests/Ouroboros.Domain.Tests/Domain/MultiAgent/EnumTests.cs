namespace Ouroboros.Tests.Domain.MultiAgent;

using Ouroboros.Domain.MultiAgent;

[Trait("Category", "Unit")]
public class EnumTests
{
    [Theory]
    [InlineData(ConsensusProtocol.Majority)]
    [InlineData(ConsensusProtocol.Unanimous)]
    [InlineData(ConsensusProtocol.Weighted)]
    [InlineData(ConsensusProtocol.Raft)]
    public void ConsensusProtocol_AllValues_AreDefined(ConsensusProtocol protocol)
    {
        // Assert
        Enum.IsDefined(protocol).Should().BeTrue();
    }

    [Fact]
    public void ConsensusProtocol_HasFourValues()
    {
        // Act
        var values = Enum.GetValues<ConsensusProtocol>();

        // Assert
        values.Should().HaveCount(4);
    }

    [Theory]
    [InlineData(AllocationStrategy.RoundRobin)]
    [InlineData(AllocationStrategy.SkillBased)]
    [InlineData(AllocationStrategy.LoadBalanced)]
    [InlineData(AllocationStrategy.Auction)]
    public void AllocationStrategy_AllValues_AreDefined(AllocationStrategy strategy)
    {
        // Assert
        Enum.IsDefined(strategy).Should().BeTrue();
    }

    [Theory]
    [InlineData(KnowledgeSyncStrategy.Full)]
    [InlineData(KnowledgeSyncStrategy.Incremental)]
    [InlineData(KnowledgeSyncStrategy.Selective)]
    [InlineData(KnowledgeSyncStrategy.Gossip)]
    public void KnowledgeSyncStrategy_AllValues_AreDefined(KnowledgeSyncStrategy strategy)
    {
        // Assert
        Enum.IsDefined(strategy).Should().BeTrue();
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
    public void MessageType_AllValues_AreDefined(MessageType type)
    {
        // Assert
        Enum.IsDefined(type).Should().BeTrue();
    }

    [Fact]
    public void MessageType_HasEightValues()
    {
        // Act
        var values = Enum.GetValues<MessageType>();

        // Assert
        values.Should().HaveCount(8);
    }

    [Theory]
    [InlineData(GroupType.Broadcast)]
    [InlineData(GroupType.RoundRobin)]
    [InlineData(GroupType.LoadBalanced)]
    public void GroupType_AllValues_AreDefined(GroupType type)
    {
        // Assert
        Enum.IsDefined(type).Should().BeTrue();
    }

    [Theory]
    [InlineData(Priority.Low)]
    [InlineData(Priority.Medium)]
    [InlineData(Priority.High)]
    [InlineData(Priority.Critical)]
    public void Priority_AllValues_AreDefined(Priority priority)
    {
        // Assert
        Enum.IsDefined(priority).Should().BeTrue();
    }

    [Theory]
    [InlineData(DependencyType.BlockedBy)]
    [InlineData(DependencyType.Requires)]
    [InlineData(DependencyType.Synchronize)]
    public void DependencyType_AllValues_AreDefined(DependencyType type)
    {
        // Assert
        Enum.IsDefined(type).Should().BeTrue();
    }
}
