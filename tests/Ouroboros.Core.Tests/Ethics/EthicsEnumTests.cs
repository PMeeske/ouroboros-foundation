using Ouroboros.Core.Ethics;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class ConcernLevelTests
{
    [Theory]
    [InlineData(ConcernLevel.Info, 0)]
    [InlineData(ConcernLevel.Low, 1)]
    [InlineData(ConcernLevel.Medium, 2)]
    [InlineData(ConcernLevel.High, 3)]
    public void ConcernLevel_HasExpectedIntegerValue(ConcernLevel level, int expected)
    {
        ((int)level).Should().Be(expected);
    }

    [Fact]
    public void ConcernLevel_HasFourMembers()
    {
        Enum.GetValues<ConcernLevel>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData("Info", ConcernLevel.Info)]
    [InlineData("Low", ConcernLevel.Low)]
    [InlineData("Medium", ConcernLevel.Medium)]
    [InlineData("High", ConcernLevel.High)]
    public void ConcernLevel_ParsesFromString(string name, ConcernLevel expected)
    {
        Enum.Parse<ConcernLevel>(name).Should().Be(expected);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class EthicalClearanceLevelTests
{
    [Theory]
    [InlineData(EthicalClearanceLevel.Permitted, 0)]
    [InlineData(EthicalClearanceLevel.PermittedWithConcerns, 1)]
    [InlineData(EthicalClearanceLevel.RequiresHumanApproval, 2)]
    [InlineData(EthicalClearanceLevel.Denied, 3)]
    public void EthicalClearanceLevel_HasExpectedIntegerValue(EthicalClearanceLevel level, int expected)
    {
        ((int)level).Should().Be(expected);
    }

    [Fact]
    public void EthicalClearanceLevel_HasFourMembers()
    {
        Enum.GetValues<EthicalClearanceLevel>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData("Permitted", EthicalClearanceLevel.Permitted)]
    [InlineData("PermittedWithConcerns", EthicalClearanceLevel.PermittedWithConcerns)]
    [InlineData("RequiresHumanApproval", EthicalClearanceLevel.RequiresHumanApproval)]
    [InlineData("Denied", EthicalClearanceLevel.Denied)]
    public void EthicalClearanceLevel_ParsesFromString(string name, EthicalClearanceLevel expected)
    {
        Enum.Parse<EthicalClearanceLevel>(name).Should().Be(expected);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class EthicalPrincipleCategoryTests
{
    [Theory]
    [InlineData(EthicalPrincipleCategory.Safety, 0)]
    [InlineData(EthicalPrincipleCategory.Autonomy, 1)]
    [InlineData(EthicalPrincipleCategory.Transparency, 2)]
    [InlineData(EthicalPrincipleCategory.Privacy, 3)]
    [InlineData(EthicalPrincipleCategory.Fairness, 4)]
    [InlineData(EthicalPrincipleCategory.Integrity, 5)]
    public void EthicalPrincipleCategory_HasExpectedIntegerValue(EthicalPrincipleCategory category, int expected)
    {
        ((int)category).Should().Be(expected);
    }

    [Fact]
    public void EthicalPrincipleCategory_HasSixMembers()
    {
        Enum.GetValues<EthicalPrincipleCategory>().Should().HaveCount(6);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class ModificationTypeTests
{
    [Theory]
    [InlineData(ModificationType.CapabilityAddition, 0)]
    [InlineData(ModificationType.BehaviorModification, 1)]
    [InlineData(ModificationType.KnowledgeUpdate, 2)]
    [InlineData(ModificationType.GoalModification, 3)]
    [InlineData(ModificationType.EthicsModification, 4)]
    [InlineData(ModificationType.ConfigurationChange, 5)]
    public void ModificationType_HasExpectedIntegerValue(ModificationType type, int expected)
    {
        ((int)type).Should().Be(expected);
    }

    [Fact]
    public void ModificationType_HasSixMembers()
    {
        Enum.GetValues<ModificationType>().Should().HaveCount(6);
    }

    [Theory]
    [InlineData("CapabilityAddition", ModificationType.CapabilityAddition)]
    [InlineData("BehaviorModification", ModificationType.BehaviorModification)]
    [InlineData("KnowledgeUpdate", ModificationType.KnowledgeUpdate)]
    [InlineData("GoalModification", ModificationType.GoalModification)]
    [InlineData("EthicsModification", ModificationType.EthicsModification)]
    [InlineData("ConfigurationChange", ModificationType.ConfigurationChange)]
    public void ModificationType_ParsesFromString(string name, ModificationType expected)
    {
        Enum.Parse<ModificationType>(name).Should().Be(expected);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class ViolationSeverityTests
{
    [Theory]
    [InlineData(ViolationSeverity.Low, 0)]
    [InlineData(ViolationSeverity.Medium, 1)]
    [InlineData(ViolationSeverity.High, 2)]
    [InlineData(ViolationSeverity.Critical, 3)]
    public void ViolationSeverity_HasExpectedIntegerValue(ViolationSeverity severity, int expected)
    {
        ((int)severity).Should().Be(expected);
    }

    [Fact]
    public void ViolationSeverity_HasFourMembers()
    {
        Enum.GetValues<ViolationSeverity>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData("Low", ViolationSeverity.Low)]
    [InlineData("Medium", ViolationSeverity.Medium)]
    [InlineData("High", ViolationSeverity.High)]
    [InlineData("Critical", ViolationSeverity.Critical)]
    public void ViolationSeverity_ParsesFromString(string name, ViolationSeverity expected)
    {
        Enum.Parse<ViolationSeverity>(name).Should().Be(expected);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class HumanApprovalDecisionTests
{
    [Theory]
    [InlineData(HumanApprovalDecision.Approved, 0)]
    [InlineData(HumanApprovalDecision.Rejected, 1)]
    [InlineData(HumanApprovalDecision.TimedOut, 2)]
    public void HumanApprovalDecision_HasExpectedIntegerValue(HumanApprovalDecision decision, int expected)
    {
        ((int)decision).Should().Be(expected);
    }

    [Fact]
    public void HumanApprovalDecision_HasThreeMembers()
    {
        Enum.GetValues<HumanApprovalDecision>().Should().HaveCount(3);
    }
}
