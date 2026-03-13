using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class PolicyTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            Name = "TestPolicy",
            Description = "A test policy"
        };

        policy.Name.Should().Be("TestPolicy");
        policy.Description.Should().Be("A test policy");
        policy.Priority.Should().Be(1.0);
        policy.IsActive.Should().BeTrue();
        policy.Rules.Should().BeEmpty();
        policy.Quotas.Should().BeEmpty();
        policy.Thresholds.Should().BeEmpty();
        policy.ApprovalGates.Should().BeEmpty();
        policy.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        policy.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ReturnsNewPolicyWithGeneratedId()
    {
        var policy = Policy.Create("Security", "Security policy");

        policy.Id.Should().NotBeEmpty();
        policy.Name.Should().Be("Security");
        policy.Description.Should().Be("Security policy");
        policy.IsActive.Should().BeTrue();
        policy.Priority.Should().Be(1.0);
    }

    [Fact]
    public void Create_MultipleCalls_GenerateDistinctIds()
    {
        var p1 = Policy.Create("P1", "Desc1");
        var p2 = Policy.Create("P2", "Desc2");

        p1.Id.Should().NotBe(p2.Id);
    }

    [Fact]
    public void Construction_WithRulesAndQuotas_SetsCollections()
    {
        var rules = new[]
        {
            new PolicyRule { Id = Guid.NewGuid(), Name = "R1", Condition = "c1", Action = PolicyAction.Log }
        };
        var quotas = new[]
        {
            new ResourceQuota { ResourceName = "cpu", MaxValue = 80, Unit = "%" }
        };

        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            Name = "P",
            Description = "D",
            Rules = rules,
            Quotas = quotas
        };

        policy.Rules.Should().HaveCount(1);
        policy.Quotas.Should().HaveCount(1);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var policy = Policy.Create("Original", "Desc");
        var modified = policy with { Name = "Modified", IsActive = false };

        modified.Name.Should().Be("Modified");
        modified.IsActive.Should().BeFalse();
        modified.Id.Should().Be(policy.Id);
        policy.Name.Should().Be("Original");
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var id = Guid.NewGuid();
        var ts = DateTime.UtcNow;
        var p1 = new Policy { Id = id, Name = "P", Description = "D", CreatedAt = ts, UpdatedAt = ts };
        var p2 = new Policy { Id = id, Name = "P", Description = "D", CreatedAt = ts, UpdatedAt = ts };

        p1.Should().Be(p2);
    }
}
