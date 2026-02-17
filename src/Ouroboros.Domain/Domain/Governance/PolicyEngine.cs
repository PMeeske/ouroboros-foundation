// <copyright file="PolicyEngine.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Concurrent;

namespace Ouroboros.Domain.Governance;

/// <summary>
/// Policy engine for evaluating and enforcing governance policies.
/// Phase 5: Governance, Safety, and Ops.
/// </summary>
public sealed class PolicyEngine : IPolicyEngine
{
    private readonly ConcurrentDictionary<Guid, Policy> _policies = new();
    private readonly ConcurrentBag<PolicyAuditEntry> _auditTrail = new();
    private readonly ConcurrentDictionary<Guid, ApprovalRequest> _pendingApprovals = new();
    private readonly ConcurrentDictionary<string, Func<object, bool>> _conditionEvaluators = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyEngine"/> class.
    /// </summary>
    public PolicyEngine()
    {
        RegisterDefaultConditionEvaluators();
    }

    /// <summary>
    /// Registers a new policy.
    /// </summary>
    public Result<Policy> RegisterPolicy(Policy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        if (_policies.ContainsKey(policy.Id))
        {
            return Result<Policy>.Failure($"Policy with ID {policy.Id} already exists");
        }

        _policies[policy.Id] = policy;
        
        AddAuditEntry(new PolicyAuditEntry
        {
            Policy = policy,
            Action = "RegisterPolicy",
            Actor = "System",
            Timestamp = DateTime.UtcNow
        });

        return Result<Policy>.Success(policy);
    }

    /// <summary>
    /// Updates an existing policy.
    /// </summary>
    public Result<Policy> UpdatePolicy(Policy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        if (!_policies.ContainsKey(policy.Id))
        {
            return Result<Policy>.Failure($"Policy with ID {policy.Id} not found");
        }

        var updated = policy with { UpdatedAt = DateTime.UtcNow };
        _policies[policy.Id] = updated;

        AddAuditEntry(new PolicyAuditEntry
        {
            Policy = updated,
            Action = "UpdatePolicy",
            Actor = "System",
            Timestamp = DateTime.UtcNow
        });

        return Result<Policy>.Success(updated);
    }

    /// <summary>
    /// Removes a policy from the engine.
    /// </summary>
    public Result<bool> RemovePolicy(Guid policyId)
    {
        if (!_policies.TryRemove(policyId, out var policy))
        {
            return Result<bool>.Failure($"Policy with ID {policyId} not found");
        }

        AddAuditEntry(new PolicyAuditEntry
        {
            Policy = policy,
            Action = "RemovePolicy",
            Actor = "System",
            Timestamp = DateTime.UtcNow
        });

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Gets all registered policies.
    /// </summary>
    public IReadOnlyList<Policy> GetPolicies(bool activeOnly = true)
    {
        var policies = _policies.Values.AsEnumerable();
        if (activeOnly)
        {
            policies = policies.Where(p => p.IsActive);
        }
        return policies.OrderByDescending(p => p.Priority).ToList();
    }

    /// <summary>
    /// Evaluates all active policies against the provided context.
    /// </summary>
    public async Task<Result<IReadOnlyList<PolicyEvaluationResult>>> EvaluatePoliciesAsync(
        object context,
        CancellationToken ct = default)
    {
        var activePolicies = GetPolicies(activeOnly: true);
        var results = new List<PolicyEvaluationResult>();

        foreach (var policy in activePolicies)
        {
            if (ct.IsCancellationRequested)
            {
                return Result<IReadOnlyList<PolicyEvaluationResult>>.Failure("Evaluation cancelled");
            }

            var result = await EvaluatePolicyAsync(policy, context, ct);
            if (result.IsSuccess)
            {
                results.Add(result.Value);
            }
        }

        return Result<IReadOnlyList<PolicyEvaluationResult>>.Success(results);
    }

    /// <summary>
    /// Evaluates a single policy against the provided context.
    /// </summary>
    public Task<Result<PolicyEvaluationResult>> EvaluatePolicyAsync(
        Policy policy,
        object context,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(context);

        var violations = new List<PolicyViolation>();

        // Evaluate rules
        foreach (var rule in policy.Rules)
        {
            if (EvaluateCondition(rule.Condition, context))
            {
                violations.Add(new PolicyViolation
                {
                    Rule = rule,
                    Severity = MapActionToSeverity(rule.Action),
                    Message = $"Rule '{rule.Name}' violated: {rule.Condition}",
                    RecommendedAction = rule.Action,
                    ActualValue = context,
                    DetectedAt = DateTime.UtcNow
                });
            }
        }

        // Evaluate quotas
        foreach (var quota in policy.Quotas)
        {
            if (quota.IsExceeded)
            {
                violations.Add(new PolicyViolation
                {
                    Rule = new PolicyRule
                    {
                        Id = Guid.NewGuid(),
                        Name = $"Quota:{quota.ResourceName}",
                        Condition = $"{quota.ResourceName} > {quota.MaxValue}",
                        Action = PolicyAction.Block
                    },
                    Severity = ViolationSeverity.High,
                    Message = $"Resource quota exceeded for {quota.ResourceName}: {quota.CurrentValue} > {quota.MaxValue} {quota.Unit}",
                    ActualValue = quota.CurrentValue,
                    ExpectedValue = quota.MaxValue,
                    RecommendedAction = PolicyAction.Block,
                    DetectedAt = DateTime.UtcNow
                });
            }
        }

        // Evaluate thresholds
        foreach (var threshold in policy.Thresholds)
        {
            // Threshold evaluation would require access to metrics
            // For now, this is a placeholder
        }

        var result = new PolicyEvaluationResult
        {
            Policy = policy,
            IsCompliant = violations.Count == 0,
            Violations = violations,
            EvaluatedAt = DateTime.UtcNow,
            Context = new Dictionary<string, object> { ["input"] = context }
        };

        AddAuditEntry(new PolicyAuditEntry
        {
            Policy = policy,
            Action = "EvaluatePolicy",
            Actor = "System",
            EvaluationResult = result,
            Timestamp = DateTime.UtcNow
        });

        return Task.FromResult(Result<PolicyEvaluationResult>.Success(result));
    }

    /// <summary>
    /// Simulates policy evaluation without enforcement.
    /// </summary>
    public async Task<Result<PolicySimulationResult>> SimulatePolicyAsync(
        Policy policy,
        object context,
        CancellationToken ct = default)
    {
        var evaluationResult = await EvaluatePolicyAsync(policy, context, ct);
        
        if (!evaluationResult.IsSuccess)
        {
            return Result<PolicySimulationResult>.Failure(evaluationResult.Error);
        }

        var simulation = new PolicySimulationResult
        {
            Policy = policy,
            EvaluationResult = evaluationResult.Value,
            WouldBlock = evaluationResult.Value.Violations.Any(v => v.RecommendedAction == PolicyAction.Block),
            RequiredApprovals = policy.ApprovalGates
                .Where(gate => EvaluateCondition(gate.Condition, context))
                .ToList(),
            SimulatedAt = DateTime.UtcNow
        };

        return Result<PolicySimulationResult>.Success(simulation);
    }

    /// <summary>
    /// Enforces policies and returns actions to take.
    /// </summary>
    public async Task<Result<PolicyEnforcementResult>> EnforcePoliciesAsync(
        object context,
        CancellationToken ct = default)
    {
        var evaluationsResult = await EvaluatePoliciesAsync(context, ct);
        
        if (!evaluationsResult.IsSuccess)
        {
            return Result<PolicyEnforcementResult>.Failure(evaluationsResult.Error);
        }

        var evaluations = evaluationsResult.Value;
        var actionsToTake = new List<PolicyAction>();
        var approvalsRequired = new List<ApprovalRequest>();

        foreach (var evaluation in evaluations)
        {
            foreach (var violation in evaluation.Violations)
            {
                actionsToTake.Add(violation.RecommendedAction);

                if (violation.RecommendedAction == PolicyAction.RequireApproval)
                {
                    // Find matching approval gate
                    var gate = evaluation.Policy.ApprovalGates.FirstOrDefault();
                    if (gate != null)
                    {
                        var request = new ApprovalRequest
                        {
                            Gate = gate,
                            OperationDescription = violation.Message,
                            Context = new Dictionary<string, object> { ["violation"] = violation },
                            Deadline = DateTime.UtcNow.Add(gate.ApprovalTimeout)
                        };
                        _pendingApprovals[request.Id] = request;
                        approvalsRequired.Add(request);
                    }
                }
            }
        }

        var enforcement = new PolicyEnforcementResult
        {
            Evaluations = evaluations,
            ActionsRequired = actionsToTake,
            ApprovalsRequired = approvalsRequired,
            IsBlocked = actionsToTake.Contains(PolicyAction.Block),
            EnforcedAt = DateTime.UtcNow
        };

        AddAuditEntry(new PolicyAuditEntry
        {
            Policy = evaluations.FirstOrDefault()?.Policy ?? Policy.Create("Multiple", "Multiple policies"),
            Action = "EnforcePolicies",
            Actor = "System",
            Metadata = new Dictionary<string, object>
            {
                ["violations_count"] = evaluations.Sum(e => e.Violations.Count),
                ["blocked"] = enforcement.IsBlocked
            },
            Timestamp = DateTime.UtcNow
        });

        return Result<PolicyEnforcementResult>.Success(enforcement);
    }

    /// <summary>
    /// Submits an approval for a pending request.
    /// </summary>
    public Result<ApprovalRequest> SubmitApproval(Guid requestId, Approval approval)
    {
        ArgumentNullException.ThrowIfNull(approval);

        if (!_pendingApprovals.TryGetValue(requestId, out var request))
        {
            return Result<ApprovalRequest>.Failure($"Approval request {requestId} not found");
        }

        if (request.IsExpired)
        {
            var expired = request with { Status = ApprovalStatus.Expired };
            _pendingApprovals[requestId] = expired;
            return Result<ApprovalRequest>.Failure("Approval request has expired");
        }

        var approvals = request.Approvals.ToList();
        approvals.Add(approval);

        var updated = request with { Approvals = approvals };

        // Check if we have enough approvals
        if (approvals.Count(a => a.Decision == ApprovalDecision.Approve) >= request.Gate.MinimumApprovals)
        {
            updated = updated with { Status = ApprovalStatus.Approved };
        }
        else if (approvals.Any(a => a.Decision == ApprovalDecision.Reject))
        {
            updated = updated with { Status = ApprovalStatus.Rejected };
        }

        _pendingApprovals[requestId] = updated;

        AddAuditEntry(new PolicyAuditEntry
        {
            Policy = Policy.Create("ApprovalGate", request.Gate.Name),
            Action = "SubmitApproval",
            Actor = approval.ApproverId,
            ApprovalRequest = updated,
            Timestamp = DateTime.UtcNow
        });

        return Result<ApprovalRequest>.Success(updated);
    }

    /// <summary>
    /// Gets pending approval requests.
    /// </summary>
    public IReadOnlyList<ApprovalRequest> GetPendingApprovals()
    {
        return _pendingApprovals.Values
            .Where(r => r.Status == ApprovalStatus.Pending && !r.IsExpired)
            .OrderBy(r => r.Deadline)
            .ToList();
    }

    /// <summary>
    /// Exports the audit trail.
    /// </summary>
    public IReadOnlyList<PolicyAuditEntry> GetAuditTrail(int? limit = null, DateTime? since = null)
    {
        var entries = _auditTrail.AsEnumerable();

        if (since.HasValue)
        {
            entries = entries.Where(e => e.Timestamp >= since.Value);
        }

        entries = entries.OrderByDescending(e => e.Timestamp);

        if (limit.HasValue)
        {
            entries = entries.Take(limit.Value);
        }

        return entries.ToList();
    }

    /// <summary>
    /// Registers a custom condition evaluator.
    /// </summary>
    public void RegisterConditionEvaluator(string name, Func<object, bool> evaluator)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(evaluator);
        _conditionEvaluators[name] = evaluator;
    }

    private void AddAuditEntry(PolicyAuditEntry entry)
    {
        _auditTrail.Add(entry);
    }

    private bool EvaluateCondition(string condition, object context)
    {
        // Simple condition evaluation - in production, this would be more sophisticated
        if (_conditionEvaluators.TryGetValue(condition, out var evaluator))
        {
            return evaluator(context);
        }

        // Default: always false for unknown conditions (fail-safe)
        return false;
    }

    private static ViolationSeverity MapActionToSeverity(PolicyAction action)
    {
        return action switch
        {
            PolicyAction.Log => ViolationSeverity.Low,
            PolicyAction.Alert => ViolationSeverity.Medium,
            PolicyAction.Throttle => ViolationSeverity.Medium,
            PolicyAction.Block => ViolationSeverity.High,
            PolicyAction.RequireApproval => ViolationSeverity.High,
            _ => ViolationSeverity.Medium
        };
    }

    private void RegisterDefaultConditionEvaluators()
    {
        // Register some common condition evaluators
        RegisterConditionEvaluator("always", _ => true);
        RegisterConditionEvaluator("never", _ => false);
    }
}