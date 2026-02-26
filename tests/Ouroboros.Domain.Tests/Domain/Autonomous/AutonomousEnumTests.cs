namespace Ouroboros.Tests.Domain.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class AutonomousEnumTests
{
    [Theory]
    [InlineData(IntentionCategory.SelfReflection)]
    [InlineData(IntentionCategory.CodeModification)]
    [InlineData(IntentionCategory.Learning)]
    [InlineData(IntentionCategory.UserCommunication)]
    [InlineData(IntentionCategory.MemoryManagement)]
    [InlineData(IntentionCategory.NeuronCommunication)]
    [InlineData(IntentionCategory.GoalPursuit)]
    [InlineData(IntentionCategory.SafetyCheck)]
    [InlineData(IntentionCategory.Exploration)]
    public void IntentionCategory_AllValues_AreDefined(IntentionCategory category)
    {
        Enum.IsDefined(category).Should().BeTrue();
    }

    [Fact]
    public void IntentionCategory_HasNineValues()
    {
        Enum.GetValues<IntentionCategory>().Should().HaveCount(9);
    }

    [Theory]
    [InlineData(IntentionPriority.Low, 0)]
    [InlineData(IntentionPriority.Normal, 1)]
    [InlineData(IntentionPriority.High, 2)]
    [InlineData(IntentionPriority.Critical, 3)]
    public void IntentionPriority_ValuesAreOrdered(IntentionPriority priority, int expected)
    {
        ((int)priority).Should().Be(expected);
    }

    [Theory]
    [InlineData(IntentionStatus.Pending)]
    [InlineData(IntentionStatus.Approved)]
    [InlineData(IntentionStatus.Rejected)]
    [InlineData(IntentionStatus.Executing)]
    [InlineData(IntentionStatus.Completed)]
    [InlineData(IntentionStatus.Failed)]
    [InlineData(IntentionStatus.Expired)]
    [InlineData(IntentionStatus.Cancelled)]
    public void IntentionStatus_AllValues_AreDefined(IntentionStatus status)
    {
        Enum.IsDefined(status).Should().BeTrue();
    }

    [Fact]
    public void IntentionStatus_HasEightValues()
    {
        Enum.GetValues<IntentionStatus>().Should().HaveCount(8);
    }

    [Theory]
    [InlineData(NeuronType.Processor)]
    [InlineData(NeuronType.Aggregator)]
    [InlineData(NeuronType.Observer)]
    [InlineData(NeuronType.Responder)]
    [InlineData(NeuronType.Core)]
    [InlineData(NeuronType.Memory)]
    [InlineData(NeuronType.CodeReflection)]
    [InlineData(NeuronType.Symbolic)]
    [InlineData(NeuronType.Communication)]
    [InlineData(NeuronType.Safety)]
    [InlineData(NeuronType.Affect)]
    [InlineData(NeuronType.Executive)]
    [InlineData(NeuronType.Learning)]
    [InlineData(NeuronType.Cognitive)]
    [InlineData(NeuronType.Custom)]
    public void NeuronType_AllValues_AreDefined(NeuronType type)
    {
        Enum.IsDefined(type).Should().BeTrue();
    }

    [Fact]
    public void NeuronType_HasFifteenValues()
    {
        Enum.GetValues<NeuronType>().Should().HaveCount(15);
    }

    [Theory]
    [InlineData(VoicePriority.Low, 0)]
    [InlineData(VoicePriority.Normal, 1)]
    [InlineData(VoicePriority.High, 2)]
    [InlineData(VoicePriority.Critical, 3)]
    public void VoicePriority_ValuesAreOrdered(VoicePriority priority, int expected)
    {
        ((int)priority).Should().Be(expected);
    }
}
