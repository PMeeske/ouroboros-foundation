using FluentAssertions;
using Ouroboros.Domain.Governance;
using Xunit;

namespace Ouroboros.Tests.Domain.Governance;

[Trait("Category", "Unit")]
public class PolicyEngineTests
{
    private readonly PolicyEngine _sut = new();

    private static Policy CreatePolicy(string name = "TestPolicy", bool isActive = true, double priority = 1.0)
    {
        return new Policy
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = $"Description for {name}",
            IsActive = isActive,
            Priority = priority
        };
    }

    private static PolicyRule CreateRule(string condition, PolicyAction action = PolicyAction.Block)
    {
        return new PolicyRule
        {
            Id = Guid.NewGuid(),
            Name = $"Rule_{condition}",
            Condition = condition,
            Action = action
        };
    }

    // ===== RegisterPolicy =====

    [Fact]
    public void RegisterPolicy_WithValidPolicy_ShouldSucceed()
    {
        var policy = CreatePolicy();

        var result = _sut.RegisterPolicy(policy);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(policy);
    }

    [Fact]
    public void RegisterPolicy_WithDuplicateId_ShouldFail()
    {
        var policy = CreatePolicy();
        _sut.RegisterPolicy(policy);

        var result = _sut.RegisterPolicy(policy);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public void RegisterPolicy_WithNull_ShouldThrow()
    {
        var act = () => _sut.RegisterPolicy(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ===== UpdatePolicy =====

    [Fact]
    public void UpdatePolicy_ExistingPolicy_ShouldSucceed()
    {
        var policy = CreatePolicy();
        _sut.RegisterPolicy(policy);

        var updated = policy with { Description = "Updated" };
        var result = _sut.UpdatePolicy(updated);

        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().Be("Updated");
    }

    [Fact]
    public void UpdatePolicy_NonExistentPolicy_ShouldFail()
    {
        var policy = CreatePolicy();

        var result = _sut.UpdatePolicy(policy);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public void UpdatePolicy_WithNull_ShouldThrow()
    {
        var act = () => _sut.UpdatePolicy(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ===== RemovePolicy =====

    [Fact]
    public void RemovePolicy_ExistingPolicy_ShouldSucceed()
    {
        var policy = CreatePolicy();
        _sut.RegisterPolicy(policy);

        var result = _sut.RemovePolicy(policy.Id);

        result.IsSuccess.Should().BeTrue();
        _sut.GetPolicies(activeOnly: false).Should().BeEmpty();
    }

    [Fact]
    public void RemovePolicy_NonExistentId_ShouldFail()
    {
        var result = _sut.RemovePolicy(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    // ===== GetPolicies =====

    [Fact]
    public void GetPolicies_ActiveOnly_ShouldFilterInactive()
    {
        _sut.RegisterPolicy(CreatePolicy("Active", isActive: true));
        _sut.RegisterPolicy(CreatePolicy("Inactive", isActive: false));

        var active = _sut.GetPolicies(activeOnly: true);

        active.Should().HaveCount(1);
        active[0].Name.Should().Be("Active");
    }

    [Fact]
    public void GetPolicies_AllPolicies_ShouldReturnAll()
    {
        _sut.RegisterPolicy(CreatePolicy("Active", isActive: true));
        _sut.RegisterPolicy(CreatePolicy("Inactive", isActive: false));

        var all = _sut.GetPolicies(activeOnly: false);

        all.Should().HaveCount(2);
    }

    [Fact]
    public void GetPolicies_ShouldOrderByPriorityDescending()
    {
        _sut.RegisterPolicy(CreatePolicy("Low", priority: 1.0));
        _sut.RegisterPolicy(CreatePolicy("High", priority: 10.0));
        _sut.RegisterPolicy(CreatePolicy("Medium", priority: 5.0));

        var policies = _sut.GetPolicies(activeOnly: false);

        policies[0].Name.Should().Be("High");
        policies[1].Name.Should().Be("Medium");
        policies[2].Name.Should().Be("Low");
    }

    // ===== EvaluatePolicyAsync =====

    [Fact]
    public async Task EvaluatePolicyAsync_WithNoMatchingRules_ShouldBeCompliant()
    {
        var policy = CreatePolicy() with
        {
            Rules = new[] { CreateRule("nonexistent_condition") }
        };

        var result = await _sut.EvaluatePolicyAsync(policy, new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompliant.Should().BeTrue();
        result.Value.Violations.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluatePolicyAsync_WithAlwaysCondition_ShouldViolate()
    {
        var policy = CreatePolicy() with
        {
            Rules = new[] { CreateRule("always", PolicyAction.Block) }
        };

        var result = await _sut.EvaluatePolicyAsync(policy, new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().HaveCount(1);
        result.Value.Violations[0].RecommendedAction.Should().Be(PolicyAction.Block);
    }

    [Fact]
    public async Task EvaluatePolicyAsync_WithNeverCondition_ShouldBeCompliant()
    {
        var policy = CreatePolicy() with
        {
            Rules = new[] { CreateRule("never") }
        };

        var result = await _sut.EvaluatePolicyAsync(policy, new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompliant.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluatePolicyAsync_WithExceededQuota_ShouldViolate()
    {
        var policy = CreatePolicy() with
        {
            Quotas = new[]
            {
                new ResourceQuota
                {
                    ResourceName = "cpu",
                    MaxValue = 100.0,
                    CurrentValue = 150.0,
                    Unit = "cores"
                }
            }
        };

        var result = await _sut.EvaluatePolicyAsync(policy, new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle();
        result.Value.Violations[0].Severity.Should().Be(ViolationSeverity.High);
    }

    [Fact]
    public async Task EvaluatePolicyAsync_WithNonExceededQuota_ShouldBeCompliant()
    {
        var policy = CreatePolicy() with
        {
            Quotas = new[]
            {
                new ResourceQuota
                {
                    ResourceName = "cpu",
                    MaxValue = 100.0,
                    CurrentValue = 50.0,
                    Unit = "cores"
                }
            }
        };

        var result = await _sut.EvaluatePolicyAsync(policy, new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompliant.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluatePolicyAsync_WithNullPolicy_ShouldThrow()
    {
        var act = async () => await _sut.EvaluatePolicyAsync(null!, new object());

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluatePolicyAsync_WithNullContext_ShouldThrow()
    {
        var policy = CreatePolicy();

        var act = async () => await _sut.EvaluatePolicyAsync(policy, null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ===== EvaluatePoliciesAsync =====

    [Fact]
    public async Task EvaluatePoliciesAsync_ShouldEvaluateAllActive()
    {
        _sut.RegisterPolicy(CreatePolicy("P1") with
        {
            Rules = new[] { CreateRule("always") }
        });
        _sut.RegisterPolicy(CreatePolicy("P2"));

        var result = await _sut.EvaluatePoliciesAsync(new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    // ===== SimulatePolicyAsync =====

    [Fact]
    public async Task SimulatePolicyAsync_ShouldReturnSimulationResult()
    {
        var policy = CreatePolicy() with
        {
            Rules = new[] { CreateRule("always", PolicyAction.Block) }
        };

        var result = await _sut.SimulatePolicyAsync(policy, new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.WouldBlock.Should().BeTrue();
        result.Value.EvaluationResult.Should().NotBeNull();
    }

    [Fact]
    public async Task SimulatePolicyAsync_NoViolations_ShouldNotBlock()
    {
        var policy = CreatePolicy();

        var result = await _sut.SimulatePolicyAsync(policy, new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.WouldBlock.Should().BeFalse();
    }

    // ===== EnforcePoliciesAsync =====

    [Fact]
    public async Task EnforcePoliciesAsync_WithBlockingViolation_ShouldBeBlocked()
    {
        _sut.RegisterPolicy(CreatePolicy() with
        {
            Rules = new[] { CreateRule("always", PolicyAction.Block) }
        });

        var result = await _sut.EnforcePoliciesAsync(new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.IsBlocked.Should().BeTrue();
        result.Value.ActionsRequired.Should().Contain(PolicyAction.Block);
    }

    [Fact]
    public async Task EnforcePoliciesAsync_NoViolations_ShouldNotBeBlocked()
    {
        _sut.RegisterPolicy(CreatePolicy());

        var result = await _sut.EnforcePoliciesAsync(new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.IsBlocked.Should().BeFalse();
    }

    // ===== RegisterConditionEvaluator =====

    [Fact]
    public async Task RegisterConditionEvaluator_ShouldBeUsedInEvaluation()
    {
        _sut.RegisterConditionEvaluator("custom_check", _ => true);
        var policy = CreatePolicy() with
        {
            Rules = new[] { CreateRule("custom_check", PolicyAction.Alert) }
        };

        var result = await _sut.EvaluatePolicyAsync(policy, new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompliant.Should().BeFalse();
    }

    [Fact]
    public void RegisterConditionEvaluator_WithNullName_ShouldThrow()
    {
        var act = () => _sut.RegisterConditionEvaluator(null!, _ => true);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RegisterConditionEvaluator_WithNullEvaluator_ShouldThrow()
    {
        var act = () => _sut.RegisterConditionEvaluator("test", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ===== SubmitApproval =====

    [Fact]
    public void SubmitApproval_ForNonExistentRequest_ShouldFail()
    {
        var approval = new Approval { ApproverId = "user1", Decision = ApprovalDecision.Approve };

        var result = _sut.SubmitApproval(Guid.NewGuid(), approval);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public void SubmitApproval_WithNull_ShouldThrow()
    {
        var act = () => _sut.SubmitApproval(Guid.NewGuid(), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ===== GetAuditTrail =====

    [Fact]
    public void GetAuditTrail_AfterRegistration_ShouldContainEntry()
    {
        _sut.RegisterPolicy(CreatePolicy());

        var trail = _sut.GetAuditTrail();

        trail.Should().NotBeEmpty();
        trail.Should().Contain(e => e.Action == "RegisterPolicy");
    }

    [Fact]
    public void GetAuditTrail_WithLimit_ShouldRespectLimit()
    {
        _sut.RegisterPolicy(CreatePolicy("P1"));
        _sut.RegisterPolicy(CreatePolicy("P2"));
        _sut.RegisterPolicy(CreatePolicy("P3"));

        var trail = _sut.GetAuditTrail(limit: 2);

        trail.Should().HaveCount(2);
    }

    [Fact]
    public void GetAuditTrail_WithSinceFilter_ShouldFilterOldEntries()
    {
        _sut.RegisterPolicy(CreatePolicy());

        var trail = _sut.GetAuditTrail(since: DateTime.UtcNow.AddMinutes(1));

        trail.Should().BeEmpty();
    }

    // ===== GetPendingApprovals =====

    [Fact]
    public void GetPendingApprovals_WhenEmpty_ShouldReturnEmptyList()
    {
        var approvals = _sut.GetPendingApprovals();

        approvals.Should().BeEmpty();
    }
}
