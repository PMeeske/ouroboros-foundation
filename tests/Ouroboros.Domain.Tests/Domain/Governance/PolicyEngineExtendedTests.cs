namespace Ouroboros.Tests.Domain.Governance;

using Ouroboros.Domain.Governance;

[Trait("Category", "Unit")]
public class PolicyEngineExtendedTests
{
    private readonly PolicyEngine _sut = new();

    private static Policy CreateTestPolicy(string name = "TestPolicy", bool isActive = true)
    {
        return new Policy
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "A test policy",
            IsActive = isActive,
            Priority = 1.0
        };
    }

    [Fact]
    public void RegisterPolicy_NewPolicy_ReturnsSuccess()
    {
        // Arrange
        var policy = CreateTestPolicy();

        // Act
        var result = _sut.RegisterPolicy(policy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("TestPolicy");
    }

    [Fact]
    public void RegisterPolicy_DuplicateId_ReturnsFailure()
    {
        // Arrange
        var policy = CreateTestPolicy();
        _sut.RegisterPolicy(policy);

        // Act
        var result = _sut.RegisterPolicy(policy);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public void RegisterPolicy_Null_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.RegisterPolicy(null!));
    }

    [Fact]
    public void UpdatePolicy_ExistingPolicy_ReturnsSuccess()
    {
        // Arrange
        var policy = CreateTestPolicy();
        _sut.RegisterPolicy(policy);
        var updated = policy with { Description = "Updated description" };

        // Act
        var result = _sut.UpdatePolicy(updated);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void UpdatePolicy_NonExistentPolicy_ReturnsFailure()
    {
        // Act
        var result = _sut.UpdatePolicy(CreateTestPolicy());

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public void RemovePolicy_ExistingPolicy_ReturnsSuccess()
    {
        // Arrange
        var policy = CreateTestPolicy();
        _sut.RegisterPolicy(policy);

        // Act
        var result = _sut.RemovePolicy(policy.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void RemovePolicy_NonExistent_ReturnsFailure()
    {
        // Act
        var result = _sut.RemovePolicy(Guid.NewGuid());

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void GetPolicies_ActiveOnly_FiltersInactive()
    {
        // Arrange
        _sut.RegisterPolicy(CreateTestPolicy("Active", isActive: true));
        _sut.RegisterPolicy(CreateTestPolicy("Inactive", isActive: false));

        // Act
        var policies = _sut.GetPolicies(activeOnly: true);

        // Assert
        policies.Should().HaveCount(1);
        policies[0].Name.Should().Be("Active");
    }

    [Fact]
    public void GetPolicies_IncludeInactive_ReturnsAll()
    {
        // Arrange
        _sut.RegisterPolicy(CreateTestPolicy("Active", isActive: true));
        _sut.RegisterPolicy(CreateTestPolicy("Inactive", isActive: false));

        // Act
        var policies = _sut.GetPolicies(activeOnly: false);

        // Assert
        policies.Should().HaveCount(2);
    }

    [Fact]
    public void GetPolicies_OrdersByPriorityDescending()
    {
        // Arrange
        _sut.RegisterPolicy(new Policy
        {
            Id = Guid.NewGuid(),
            Name = "Low",
            Description = "Low priority",
            Priority = 1.0
        });
        _sut.RegisterPolicy(new Policy
        {
            Id = Guid.NewGuid(),
            Name = "High",
            Description = "High priority",
            Priority = 10.0
        });

        // Act
        var policies = _sut.GetPolicies();

        // Assert
        policies[0].Name.Should().Be("High");
    }

    [Fact]
    public async Task EvaluatePoliciesAsync_NoPolicies_ReturnsEmptyList()
    {
        // Act
        var result = await _sut.EvaluatePoliciesAsync(new { test = true });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluatePolicyAsync_WithAlwaysCondition_DetectsViolation()
    {
        // Arrange
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            Name = "AlwaysViolated",
            Description = "Test",
            Rules = new List<PolicyRule>
            {
                new PolicyRule
                {
                    Id = Guid.NewGuid(),
                    Name = "AlwaysRule",
                    Condition = "always",
                    Action = PolicyAction.Block
                }
            }
        };

        // Act
        var result = await _sut.EvaluatePolicyAsync(policy, new { test = true });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().HaveCount(1);
    }

    [Fact]
    public async Task EvaluatePolicyAsync_WithNeverCondition_NoViolation()
    {
        // Arrange
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            Name = "NeverViolated",
            Description = "Test",
            Rules = new List<PolicyRule>
            {
                new PolicyRule
                {
                    Id = Guid.NewGuid(),
                    Name = "NeverRule",
                    Condition = "never",
                    Action = PolicyAction.Block
                }
            }
        };

        // Act
        var result = await _sut.EvaluatePolicyAsync(policy, new { test = true });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompliant.Should().BeTrue();
        result.Value.Violations.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluatePolicyAsync_WithExceededQuota_DetectsViolation()
    {
        // Arrange
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            Name = "QuotaPolicy",
            Description = "Test",
            Quotas = new List<ResourceQuota>
            {
                new ResourceQuota
                {
                    ResourceName = "CPU",
                    MaxValue = 80,
                    CurrentValue = 95,
                    Unit = "%"
                }
            }
        };

        // Act
        var result = await _sut.EvaluatePolicyAsync(policy, new { });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle();
    }

    [Fact]
    public void RegisterConditionEvaluator_CustomCondition_IsUsed()
    {
        // Arrange
        _sut.RegisterConditionEvaluator("is_dangerous", ctx => ctx is string s && s.Contains("danger"));

        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            Name = "CustomConditionPolicy",
            Description = "Test",
            Rules = new List<PolicyRule>
            {
                new PolicyRule
                {
                    Id = Guid.NewGuid(),
                    Name = "DangerRule",
                    Condition = "is_dangerous",
                    Action = PolicyAction.Alert
                }
            }
        };

        _sut.RegisterPolicy(policy);

        // Act
        var result = _sut.EvaluatePolicyAsync(policy, "this is danger").GetAwaiter().GetResult();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompliant.Should().BeFalse();
    }

    [Fact]
    public async Task SimulatePolicyAsync_ReturnsSimulationResult()
    {
        // Arrange
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            Name = "SimPolicy",
            Description = "Test",
            Rules = new List<PolicyRule>
            {
                new PolicyRule
                {
                    Id = Guid.NewGuid(),
                    Name = "BlockRule",
                    Condition = "always",
                    Action = PolicyAction.Block
                }
            }
        };

        // Act
        var result = await _sut.SimulatePolicyAsync(policy, new { });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.WouldBlock.Should().BeTrue();
    }

    [Fact]
    public async Task EnforcePoliciesAsync_WithBlockViolation_SetsIsBlocked()
    {
        // Arrange
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            Name = "BlockPolicy",
            Description = "Test",
            Rules = new List<PolicyRule>
            {
                new PolicyRule
                {
                    Id = Guid.NewGuid(),
                    Name = "BlockRule",
                    Condition = "always",
                    Action = PolicyAction.Block
                }
            }
        };
        _sut.RegisterPolicy(policy);

        // Act
        var result = await _sut.EnforcePoliciesAsync(new { });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsBlocked.Should().BeTrue();
    }

    [Fact]
    public void GetAuditTrail_ReturnsEntries()
    {
        // Arrange
        _sut.RegisterPolicy(CreateTestPolicy());

        // Act
        var trail = _sut.GetAuditTrail();

        // Assert
        trail.Should().NotBeEmpty();
        trail[0].Action.Should().Be("RegisterPolicy");
    }

    [Fact]
    public void GetAuditTrail_WithLimit_RespectsLimit()
    {
        // Arrange
        _sut.RegisterPolicy(CreateTestPolicy("P1"));
        _sut.RegisterPolicy(CreateTestPolicy("P2"));
        _sut.RegisterPolicy(CreateTestPolicy("P3"));

        // Act
        var trail = _sut.GetAuditTrail(limit: 2);

        // Assert
        trail.Should().HaveCount(2);
    }

    [Fact]
    public void GetAuditTrail_WithSince_FiltersOldEntries()
    {
        // Arrange
        _sut.RegisterPolicy(CreateTestPolicy());

        // Act
        var trail = _sut.GetAuditTrail(since: DateTime.UtcNow.AddMinutes(-1));

        // Assert
        trail.Should().NotBeEmpty();
    }

    [Fact]
    public void SubmitApproval_NonExistentRequest_ReturnsFailure()
    {
        // Act
        var result = _sut.SubmitApproval(Guid.NewGuid(), new Approval
        {
            ApproverId = "user",
            Decision = ApprovalDecision.Approve
        });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public void GetPendingApprovals_InitiallyEmpty()
    {
        // Act
        var pending = _sut.GetPendingApprovals();

        // Assert
        pending.Should().BeEmpty();
    }
}
