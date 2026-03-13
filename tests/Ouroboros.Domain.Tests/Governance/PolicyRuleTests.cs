using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class PolicyRuleTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var id = Guid.NewGuid();
        var rule = new PolicyRule
        {
            Id = id,
            Name = "MaxTokens",
            Condition = "tokens > 4096",
            Action = PolicyAction.Block
        };

        rule.Id.Should().Be(id);
        rule.Name.Should().Be("MaxTokens");
        rule.Condition.Should().Be("tokens > 4096");
        rule.Action.Should().Be(PolicyAction.Block);
        rule.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Construction_WithMetadata_SetsMetadata()
    {
        var metadata = new Dictionary<string, object> { ["source"] = "admin" };
        var rule = new PolicyRule
        {
            Id = Guid.NewGuid(),
            Name = "Rule",
            Condition = "always",
            Action = PolicyAction.Log,
            Metadata = metadata
        };

        rule.Metadata.Should().ContainKey("source");
        rule.Metadata["source"].Should().Be("admin");
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var id = Guid.NewGuid();
        var rule1 = new PolicyRule { Id = id, Name = "R", Condition = "c", Action = PolicyAction.Log };
        var rule2 = new PolicyRule { Id = id, Name = "R", Condition = "c", Action = PolicyAction.Log };

        rule1.Should().Be(rule2);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var rule = new PolicyRule { Id = Guid.NewGuid(), Name = "R", Condition = "c", Action = PolicyAction.Log };
        var modified = rule with { Action = PolicyAction.Block };

        modified.Action.Should().Be(PolicyAction.Block);
        rule.Action.Should().Be(PolicyAction.Log);
    }
}
