using Ouroboros.Abstractions.Monads;
using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class PolicyEngineTests
{
    private readonly PolicyEngine _engine = new();

    private static Policy CreateTestPolicy(
        string name = "TestPolicy",
        bool isActive = true,
        double priority = 1.0,
        IReadOnlyList<PolicyRule>? rules = null,
        IReadOnlyList<ResourceQuota>? quotas = null,
        IReadOnlyList<ApprovalGate>? approvalGates = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Description = $"Test policy: {name}",
        IsActive = isActive,
        Priority = priority,
        Rules = rules ?? Array.Empty<PolicyRule>(),
        Quotas = quotas ?? Array.Empty<ResourceQuota>(),
        ApprovalGates = approvalGates ?? Array.Empty<ApprovalGate>()
    };

    private static PolicyRule CreateTestRule(
        string name = "TestRule",
        string condition = "always",
        PolicyAction action = PolicyAction.Block) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Condition = condition,
        Action = action
    };

    private static ApprovalGate CreateTestGate(
        string name = "TestGate",
        string condition = "always",
        int minimumApprovals = 1,
        TimeSpan? timeout = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Condition = condition,
        MinimumApprovals = minimumApprovals,
        ApprovalTimeout = timeout ?? TimeSpan.FromHours(24)
    };

    #region RegisterPolicy

    [Fact]
    public void RegisterPolicy_ValidPolicy_ReturnsSuccess()
    {
        var policy = CreateTestPolicy();

        var result = _engine.RegisterPolicy(policy);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(policy);
    }

    [Fact]
    public void RegisterPolicy_DuplicateId_ReturnsFailure()
    {
        var policy = CreateTestPolicy();
        _engine.RegisterPolicy(policy);

        var result = _engine.RegisterPolicy(policy);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public void RegisterPolicy_NullPolicy_ThrowsArgumentNullException()
    {
        var act = () => _engine.RegisterPolicy(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RegisterPolicy_CreatesAuditEntry()
    {
        var policy = CreateTestPolicy();

        _engine.RegisterPolicy(policy);

        var audit = _engine.GetAuditTrail();
        audit.Should().ContainSingle(e => e.Action == "RegisterPolicy" && e.Policy.Id == policy.Id);
    }

    #endregion

    #region UpdatePolicy

    [Fact]
    public void UpdatePolicy_ExistingPolicy_ReturnsSuccess()
    {
        var policy = CreateTestPolicy();
        _engine.RegisterPolicy(policy);

        var updated = policy with { Name = "Updated" };
        var result = _engine.UpdatePolicy(updated);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated");
        result.Value.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdatePolicy_NonExistentPolicy_ReturnsFailure()
    {
        var policy = CreateTestPolicy();

        var result = _engine.UpdatePolicy(policy);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public void UpdatePolicy_NullPolicy_ThrowsArgumentNullException()
    {
        var act = () => _engine.UpdatePolicy(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region RemovePolicy

    [Fact]
    public void RemovePolicy_ExistingPolicy_ReturnsSuccess()
    {
        var policy = CreateTestPolicy();
        _engine.RegisterPolicy(policy);

        var result = _engine.RemovePolicy(policy.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public void RemovePolicy_NonExistentPolicy_ReturnsFailure()
    {
        var result = _engine.RemovePolicy(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public void RemovePolicy_RemovedPolicyNotInGetPolicies()
    {
        var policy = CreateTestPolicy();
        _engine.RegisterPolicy(policy);
        _engine.RemovePolicy(policy.Id);

        var policies = _engine.GetPolicies(activeOnly: false);

        policies.Should().NotContain(p => p.Id == policy.Id);
    }

    #endregion

    #region GetPolicies

    [Fact]
    public void GetPolicies_ActiveOnly_FiltersInactiveOnes()
    {
        var active = CreateTestPolicy("Active", isActive: true);
        var inactive = CreateTestPolicy("Inactive", isActive: false);
        _engine.RegisterPolicy(active);
        _engine.RegisterPolicy(inactive);

        var result = _engine.GetPolicies(activeOnly: true);

        result.Should().ContainSingle(p => p.Name == "Active");
    }

    [Fact]
    public void GetPolicies_AllPolicies_IncludesInactive()
    {
        var active = CreateTestPolicy("Active", isActive: true);
        var inactive = CreateTestPolicy("Inactive", isActive: false);
        _engine.RegisterPolicy(active);
        _engine.RegisterPolicy(inactive);

        var result = _engine.GetPolicies(activeOnly: false);

        result.Should().HaveCount(2);
    }

    [Fact]
    public void GetPolicies_OrdersByPriorityDescending()
    {
        var low = CreateTestPolicy("Low", priority: 1.0);
        var high = CreateTestPolicy("High", priority: 10.0);
        var mid = CreateTestPolicy("Mid", priority: 5.0);
        _engine.RegisterPolicy(low);
        _engine.RegisterPolicy(high);
        _engine.RegisterPolicy(mid);

        var result = _engine.GetPolicies();

        result[0].Priority.Should().Be(10.0);
        result[1].Priority.Should().Be(5.0);
        result[2].Priority.Should().Be(1.0);
    }

    #endregion

    #region EvaluatePolicyAsync

    [Fact]
    public async Task EvaluatePolicyAsync_NoViolations_ReturnsCompliant()
    {
        var policy = CreateTestPolicy(rules: new[]
        {
            CreateTestRule(condition: "never", action: PolicyAction.Block)
        });

        var result = await _engine.EvaluatePolicyAsync(policy, new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompliant.Should().BeTrue();
        result.Value.Violations.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluatePolicyAsync_RuleViolation_ReturnsNonCompliant()
    {
        var rule = CreateTestRule(condition: "always", action: PolicyAction.Block);
        var policy = CreateTestPolicy(rules: new[] { rule });

        var result = await _engine.EvaluatePolicyAsync(policy, new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().HaveCount(1);
        result.Value.Violations[0].RecommendedAction.Should().Be(PolicyAction.Block);
    }

    [Fact]
    public async Task EvaluatePolicyAsync_ExceededQuota_ReturnsViolation()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "cpu",
            MaxValue = 80.0,
            CurrentValue = 95.0,
            Unit = "percent"
        };
        var policy = CreateTestPolicy(quotas: new[] { quota });

        var result = await _engine.EvaluatePolicyAsync(policy, new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().ContainSingle();
        result.Value.Violations[0].Severity.Should().Be(ViolationSeverity.High);
    }

    [Fact]
    public async Task EvaluatePolicyAsync_NotExceededQuota_NoViolation()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "cpu",
            MaxValue = 80.0,
            CurrentValue = 50.0,
            Unit = "percent"
        };
        var policy = CreateTestPolicy(quotas: new[] { quota });

        var result = await _engine.EvaluatePolicyAsync(policy, new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompliant.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluatePolicyAsync_NullPolicy_ThrowsArgumentNullException()
    {
        var act = () => _engine.EvaluatePolicyAsync(null!, new object());

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluatePolicyAsync_NullContext_ThrowsArgumentNullException()
    {
        var policy = CreateTestPolicy();
        var act = () => _engine.EvaluatePolicyAsync(policy, null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluatePolicyAsync_CustomConditionEvaluator_IsUsed()
    {
        _engine.RegisterConditionEvaluator("is-string", ctx => ctx is string);
        var rule = CreateTestRule(condition: "is-string", action: PolicyAction.Alert);
        var policy = CreateTestPolicy(rules: new[] { rule });

        var resultString = await _engine.EvaluatePolicyAsync(policy, "hello");
        var resultInt = await _engine.EvaluatePolicyAsync(policy, 42);

        resultString.Value.IsCompliant.Should().BeFalse();
        resultInt.Value.IsCompliant.Should().BeTrue();
    }

    #endregion

    #region EvaluatePoliciesAsync

    [Fact]
    public async Task EvaluatePoliciesAsync_MultipleActivePolicies_EvaluatesAll()
    {
        var policy1 = CreateTestPolicy("P1", rules: new[] { CreateTestRule(condition: "always") });
        var policy2 = CreateTestPolicy("P2");
        _engine.RegisterPolicy(policy1);
        _engine.RegisterPolicy(policy2);

        var result = await _engine.EvaluatePoliciesAsync(new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task EvaluatePoliciesAsync_CancellationRequested_ReturnsFailure()
    {
        var policy = CreateTestPolicy();
        _engine.RegisterPolicy(policy);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await _engine.EvaluatePoliciesAsync(new object(), cts.Token);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("cancelled");
    }

    #endregion

    #region SimulatePolicyAsync

    [Fact]
    public async Task SimulatePolicyAsync_BlockingViolation_SetsWouldBlock()
    {
        var rule = CreateTestRule(condition: "always", action: PolicyAction.Block);
        var policy = CreateTestPolicy(rules: new[] { rule });

        var result = await _engine.SimulatePolicyAsync(policy, new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.WouldBlock.Should().BeTrue();
    }

    [Fact]
    public async Task SimulatePolicyAsync_NoViolation_WouldBlockIsFalse()
    {
        var policy = CreateTestPolicy();

        var result = await _engine.SimulatePolicyAsync(policy, new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.WouldBlock.Should().BeFalse();
    }

    [Fact]
    public async Task SimulatePolicyAsync_WithApprovalGate_MatchingCondition_ListsRequiredApprovals()
    {
        var gate = CreateTestGate(condition: "always");
        var policy = CreateTestPolicy(approvalGates: new[] { gate });

        var result = await _engine.SimulatePolicyAsync(policy, new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.RequiredApprovals.Should().ContainSingle();
    }

    #endregion

    #region EnforcePoliciesAsync

    [Fact]
    public async Task EnforcePoliciesAsync_BlockingViolation_IsBlocked()
    {
        var rule = CreateTestRule(condition: "always", action: PolicyAction.Block);
        var policy = CreateTestPolicy(rules: new[] { rule });
        _engine.RegisterPolicy(policy);

        var result = await _engine.EnforcePoliciesAsync(new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.IsBlocked.Should().BeTrue();
        result.Value.ActionsRequired.Should().Contain(PolicyAction.Block);
    }

    [Fact]
    public async Task EnforcePoliciesAsync_RequireApprovalViolation_CreatesApprovalRequest()
    {
        var rule = CreateTestRule(condition: "always", action: PolicyAction.RequireApproval);
        var gate = CreateTestGate();
        var policy = CreateTestPolicy(rules: new[] { rule }, approvalGates: new[] { gate });
        _engine.RegisterPolicy(policy);

        var result = await _engine.EnforcePoliciesAsync(new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.ApprovalsRequired.Should().NotBeEmpty();
    }

    [Fact]
    public async Task EnforcePoliciesAsync_NoViolations_NotBlocked()
    {
        var policy = CreateTestPolicy();
        _engine.RegisterPolicy(policy);

        var result = await _engine.EnforcePoliciesAsync(new object());

        result.IsSuccess.Should().BeTrue();
        result.Value.IsBlocked.Should().BeFalse();
        result.Value.ActionsRequired.Should().BeEmpty();
    }

    #endregion

    #region SubmitApproval

    [Fact]
    public async Task SubmitApproval_ValidApproval_ReturnsSuccess()
    {
        var rule = CreateTestRule(condition: "always", action: PolicyAction.RequireApproval);
        var gate = CreateTestGate(minimumApprovals: 1);
        var policy = CreateTestPolicy(rules: new[] { rule }, approvalGates: new[] { gate });
        _engine.RegisterPolicy(policy);

        var enforcement = await _engine.EnforcePoliciesAsync(new object());
        var requestId = enforcement.Value.ApprovalsRequired[0].Id;

        var approval = new Approval
        {
            ApproverId = "admin-1",
            Decision = ApprovalDecision.Approve
        };

        var result = _engine.SubmitApproval(requestId, approval);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(ApprovalStatus.Approved);
    }

    [Fact]
    public async Task SubmitApproval_Rejection_SetsRejectedStatus()
    {
        var rule = CreateTestRule(condition: "always", action: PolicyAction.RequireApproval);
        var gate = CreateTestGate(minimumApprovals: 2);
        var policy = CreateTestPolicy(rules: new[] { rule }, approvalGates: new[] { gate });
        _engine.RegisterPolicy(policy);

        var enforcement = await _engine.EnforcePoliciesAsync(new object());
        var requestId = enforcement.Value.ApprovalsRequired[0].Id;

        var rejection = new Approval
        {
            ApproverId = "admin-1",
            Decision = ApprovalDecision.Reject
        };

        var result = _engine.SubmitApproval(requestId, rejection);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(ApprovalStatus.Rejected);
    }

    [Fact]
    public void SubmitApproval_NonExistentRequest_ReturnsFailure()
    {
        var approval = new Approval
        {
            ApproverId = "admin-1",
            Decision = ApprovalDecision.Approve
        };

        var result = _engine.SubmitApproval(Guid.NewGuid(), approval);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public void SubmitApproval_NullApproval_ThrowsArgumentNullException()
    {
        var act = () => _engine.SubmitApproval(Guid.NewGuid(), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region GetPendingApprovals

    [Fact]
    public async Task GetPendingApprovals_WithPendingRequests_ReturnsThem()
    {
        var rule = CreateTestRule(condition: "always", action: PolicyAction.RequireApproval);
        var gate = CreateTestGate(minimumApprovals: 2);
        var policy = CreateTestPolicy(rules: new[] { rule }, approvalGates: new[] { gate });
        _engine.RegisterPolicy(policy);

        await _engine.EnforcePoliciesAsync(new object());

        var pending = _engine.GetPendingApprovals();

        pending.Should().NotBeEmpty();
    }

    [Fact]
    public void GetPendingApprovals_NoPendingRequests_ReturnsEmpty()
    {
        var pending = _engine.GetPendingApprovals();

        pending.Should().BeEmpty();
    }

    #endregion

    #region GetAuditTrail

    [Fact]
    public void GetAuditTrail_WithLimit_RespectsLimit()
    {
        for (int i = 0; i < 5; i++)
        {
            _engine.RegisterPolicy(CreateTestPolicy($"Policy{i}"));
        }

        var trail = _engine.GetAuditTrail(limit: 3);

        trail.Should().HaveCount(3);
    }

    [Fact]
    public void GetAuditTrail_WithSince_FiltersOlderEntries()
    {
        _engine.RegisterPolicy(CreateTestPolicy());

        var trail = _engine.GetAuditTrail(since: DateTime.UtcNow.AddMinutes(-1));

        trail.Should().NotBeEmpty();
    }

    [Fact]
    public void GetAuditTrail_WithFutureSince_ReturnsEmpty()
    {
        _engine.RegisterPolicy(CreateTestPolicy());

        var trail = _engine.GetAuditTrail(since: DateTime.UtcNow.AddHours(1));

        trail.Should().BeEmpty();
    }

    #endregion

    #region RegisterConditionEvaluator

    [Fact]
    public void RegisterConditionEvaluator_NullName_ThrowsArgumentNullException()
    {
        var act = () => _engine.RegisterConditionEvaluator(null!, _ => true);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RegisterConditionEvaluator_NullEvaluator_ThrowsArgumentNullException()
    {
        var act = () => _engine.RegisterConditionEvaluator("test", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_RegistersDefaultEvaluators()
    {
        // "always" and "never" are registered by default
        var alwaysRule = CreateTestRule(condition: "always", action: PolicyAction.Log);
        var neverRule = CreateTestRule(condition: "never", action: PolicyAction.Log);
        var policy = CreateTestPolicy(rules: new[] { alwaysRule, neverRule });

        var result = _engine.EvaluatePolicyAsync(policy, new object()).Result;

        result.Value.Violations.Should().ContainSingle(); // only "always" triggers
    }

    #endregion
}
