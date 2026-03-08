// <copyright file="ImmutableEthicsFramework.Evaluations.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Ethics;

/// <summary>
/// Extended evaluation methods for the ImmutableEthicsFramework.
/// Covers plan, goal, skill, research, and self-modification evaluations.
/// </summary>
public sealed partial class ImmutableEthicsFramework
{
    /// <inheritdoc/>
    public async Task<Result<EthicalClearance, string>> EvaluatePlanAsync(
        PlanContext planContext,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(planContext);

        try
        {
            List<EthicalViolation> allViolations = new List<EthicalViolation>();
            List<EthicalConcern> allConcerns = new List<EthicalConcern>();

            // Evaluate each step in the plan
            foreach (PlanStep step in planContext.Plan.Steps)
            {
                step.Parameters.TryGetValue("target", out object? targetValue);

                ProposedAction proposedAction = new ProposedAction
                {
                    ActionType = step.Action,
                    Description = $"{step.Action}: {step.ExpectedOutcome}",
                    Parameters = step.Parameters,
                    TargetEntity = targetValue?.ToString(),
                    PotentialEffects = new[] { step.ExpectedOutcome }
                };

                (IReadOnlyList<EthicalViolation>? violations, IReadOnlyList<EthicalConcern>? concerns) = _reasoner.AnalyzeAction(
                    proposedAction,
                    planContext.ActionContext,
                    _corePrinciples);

                allViolations.AddRange(violations);
                allConcerns.AddRange(concerns);
            }

            // Check for high-risk plan patterns
            if (planContext.EstimatedRisk > 0.7)
            {
                allConcerns.Add(new EthicalConcern
                {
                    RelatedPrinciple = EthicalPrinciple.HumanOversight,
                    Description = $"Plan has high estimated risk: {planContext.EstimatedRisk:F2}",
                    Level = ConcernLevel.High,
                    RecommendedAction = "Request human review before execution"
                });
            }

            EthicalClearance clearance;

            if (allViolations.Count > 0)
            {
                clearance = EthicalClearance.Denied(
                    $"Plan '{planContext.Plan.Goal}' contains {allViolations.Count} ethical violation(s)",
                    allViolations,
                    _corePrinciples);
            }
            else if (planContext.EstimatedRisk > 0.7 || allConcerns.Any(c => c.Level == ConcernLevel.High))
            {
                clearance = EthicalClearance.RequiresApproval(
                    $"Plan '{planContext.Plan.Goal}' requires human approval",
                    allConcerns,
                    _corePrinciples);
            }
            else if (allConcerns.Count > 0)
            {
                clearance = new EthicalClearance
                {
                    IsPermitted = true,
                    Level = EthicalClearanceLevel.PermittedWithConcerns,
                    RelevantPrinciples = _corePrinciples,
                    Violations = Array.Empty<EthicalViolation>(),
                    Concerns = allConcerns,
                    Reasoning = $"Plan '{planContext.Plan.Goal}' is permitted with {allConcerns.Count} concern(s)"
                };
            }
            else
            {
                clearance = EthicalClearance.Permitted(
                    $"Plan '{planContext.Plan.Goal}' complies with ethical principles",
                    _corePrinciples);
            }

            await LogEvaluationAsync("Plan", planContext.Plan.Goal, planContext.ActionContext, clearance, ct);

            return Result<EthicalClearance, string>.Success(clearance);
        }
        catch (InvalidOperationException ex)
        {
            return Result<EthicalClearance, string>.Failure($"Plan evaluation failed: {ex.Message}");
        }
        catch (IOException ex)
        {
            return Result<EthicalClearance, string>.Failure($"Plan evaluation failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<EthicalClearance, string>> EvaluateGoalAsync(
        Goal goal,
        ActionContext context,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(goal);
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            List<EthicalViolation> violations = new List<EthicalViolation>();
            List<EthicalConcern> concerns = new List<EthicalConcern>();

            if (_reasoner.ContainsHarmfulPatterns(goal.Description))
            {
                violations.Add(new EthicalViolation
                {
                    ViolatedPrinciple = EthicalPrinciple.DoNoHarm,
                    Description = "Goal description contains harmful intent",
                    Severity = ViolationSeverity.Critical,
                    Evidence = $"Goal: {goal.Description}",
                    AffectedParties = new[] { "Users", "System" }
                });
            }

            if (goal.Type.Equals("Safety", StringComparison.OrdinalIgnoreCase))
            {
                concerns.Add(new EthicalConcern
                {
                    RelatedPrinciple = EthicalPrinciple.DoNoHarm,
                    Description = "Safety goal identified",
                    Level = ConcernLevel.Info,
                    RecommendedAction = "Prioritize this goal"
                });
            }

            if (goal.Priority > 0.9 && !goal.Type.Equals("Safety", StringComparison.OrdinalIgnoreCase))
            {
                concerns.Add(new EthicalConcern
                {
                    RelatedPrinciple = EthicalPrinciple.HumanOversight,
                    Description = "Very high priority goal detected",
                    Level = ConcernLevel.Medium,
                    RecommendedAction = "Ensure goal alignment with user intent"
                });
            }

            EthicalClearance clearance;

            if (violations.Count > 0)
            {
                clearance = EthicalClearance.Denied(
                    $"Goal '{goal.Description}' violates ethical principles",
                    violations,
                    _corePrinciples);
            }
            else if (concerns.Any(c => c.Level == ConcernLevel.High))
            {
                clearance = EthicalClearance.RequiresApproval(
                    $"Goal '{goal.Description}' requires human approval",
                    concerns,
                    _corePrinciples);
            }
            else
            {
                clearance = EthicalClearance.Permitted(
                    $"Goal '{goal.Description}' is ethically aligned",
                    _corePrinciples,
                    confidenceScore: violations.Count == 0 && concerns.Count == 0 ? 1.0 : 0.8);
            }

            await LogEvaluationAsync("Goal", goal.Description, context, clearance, ct);

            return Result<EthicalClearance, string>.Success(clearance);
        }
        catch (InvalidOperationException ex)
        {
            return Result<EthicalClearance, string>.Failure($"Goal evaluation failed: {ex.Message}");
        }
        catch (IOException ex)
        {
            return Result<EthicalClearance, string>.Failure($"Goal evaluation failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<EthicalClearance, string>> EvaluateSkillAsync(
        SkillUsageContext skillContext,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(skillContext);

        try
        {
            List<EthicalViolation> violations = new List<EthicalViolation>();
            List<EthicalConcern> concerns = new List<EthicalConcern>();

            if (_reasoner.ContainsHarmfulPatterns(skillContext.Skill.Description))
            {
                violations.Add(new EthicalViolation
                {
                    ViolatedPrinciple = EthicalPrinciple.DoNoHarm,
                    Description = "Skill contains harmful operations",
                    Severity = ViolationSeverity.High,
                    Evidence = $"Skill: {skillContext.Skill.Description}",
                    AffectedParties = new[] { "Users", "System" }
                });
            }

            if (skillContext.HistoricalSuccessRate < 0.5 && skillContext.Skill.UsageCount > 5)
            {
                concerns.Add(new EthicalConcern
                {
                    RelatedPrinciple = EthicalPrinciple.Transparency,
                    Description = $"Skill has low success rate: {skillContext.HistoricalSuccessRate:P0}",
                    Level = ConcernLevel.Medium,
                    RecommendedAction = "Consider alternative approaches or improve skill"
                });
            }

            EthicalClearance clearance;

            if (violations.Count > 0)
            {
                clearance = EthicalClearance.Denied(
                    $"Skill '{skillContext.Skill.Name}' violates ethical principles",
                    violations,
                    _corePrinciples);
            }
            else if (concerns.Count > 0)
            {
                clearance = new EthicalClearance
                {
                    IsPermitted = true,
                    Level = EthicalClearanceLevel.PermittedWithConcerns,
                    RelevantPrinciples = _corePrinciples,
                    Violations = Array.Empty<EthicalViolation>(),
                    Concerns = concerns,
                    Reasoning = $"Skill '{skillContext.Skill.Name}' usage permitted with concerns"
                };
            }
            else
            {
                clearance = EthicalClearance.Permitted(
                    $"Skill '{skillContext.Skill.Name}' usage is ethically compliant",
                    _corePrinciples);
            }

            await LogEvaluationAsync("Skill", skillContext.Skill.Name, skillContext.ActionContext, clearance, ct);

            return Result<EthicalClearance, string>.Success(clearance);
        }
        catch (InvalidOperationException ex)
        {
            return Result<EthicalClearance, string>.Failure($"Skill evaluation failed: {ex.Message}");
        }
        catch (IOException ex)
        {
            return Result<EthicalClearance, string>.Failure($"Skill evaluation failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<EthicalClearance, string>> EvaluateResearchAsync(
        string researchDescription,
        ActionContext context,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(researchDescription);
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            List<EthicalViolation> violations = new List<EthicalViolation>();
            List<EthicalConcern> concerns = new List<EthicalConcern>();

            if (_reasoner.ContainsHarmfulPatterns(researchDescription))
            {
                violations.Add(new EthicalViolation
                {
                    ViolatedPrinciple = EthicalPrinciple.DoNoHarm,
                    Description = "Research activity may cause harm",
                    Severity = ViolationSeverity.High,
                    Evidence = researchDescription,
                    AffectedParties = new[] { "Users", "System", "Data subjects" }
                });
            }

            string[] sensitiveKeywords = new[] { "user data", "personal", "private", "confidential", "experiment on users" };
            if (sensitiveKeywords.Any(k => researchDescription.Contains(k, StringComparison.OrdinalIgnoreCase)))
            {
                concerns.Add(new EthicalConcern
                {
                    RelatedPrinciple = EthicalPrinciple.Privacy,
                    Description = "Research involves sensitive data or human subjects",
                    Level = ConcernLevel.High,
                    RecommendedAction = "Require explicit consent and human oversight"
                });
            }

            EthicalClearance clearance;

            if (violations.Count > 0)
            {
                clearance = EthicalClearance.Denied(
                    "Research activity violates ethical principles",
                    violations,
                    _corePrinciples);
            }
            else if (concerns.Any(c => c.Level == ConcernLevel.High))
            {
                clearance = EthicalClearance.RequiresApproval(
                    "Research activity requires human approval",
                    concerns,
                    _corePrinciples);
            }
            else
            {
                clearance = EthicalClearance.Permitted(
                    "Research activity is ethically compliant",
                    _corePrinciples);
            }

            await LogEvaluationAsync("Research", researchDescription, context, clearance, ct);

            return Result<EthicalClearance, string>.Success(clearance);
        }
        catch (InvalidOperationException ex)
        {
            return Result<EthicalClearance, string>.Failure($"Research evaluation failed: {ex.Message}");
        }
        catch (IOException ex)
        {
            return Result<EthicalClearance, string>.Failure($"Research evaluation failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<EthicalClearance, string>> EvaluateSelfModificationAsync(
        SelfModificationRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            List<EthicalViolation> violations = new List<EthicalViolation>();
            List<EthicalConcern> concerns = new List<EthicalConcern>();

            concerns.Add(new EthicalConcern
            {
                RelatedPrinciple = EthicalPrinciple.HumanOversight,
                Description = "Self-modification detected",
                Level = ConcernLevel.High,
                RecommendedAction = "Require human approval for all self-modifications"
            });

            if (request.Type == ModificationType.EthicsModification)
            {
                violations.Add(new EthicalViolation
                {
                    ViolatedPrinciple = EthicalPrinciple.SafeSelfImprovement,
                    Description = "Attempted modification of ethical constraints",
                    Severity = ViolationSeverity.Critical,
                    Evidence = request.Description,
                    AffectedParties = new[] { "System integrity", "All users" }
                });
            }

            if (_reasoner.ContainsHarmfulPatterns(request.Description))
            {
                violations.Add(new EthicalViolation
                {
                    ViolatedPrinciple = EthicalPrinciple.DoNoHarm,
                    Description = "Self-modification may introduce harmful behavior",
                    Severity = ViolationSeverity.Critical,
                    Evidence = request.Description,
                    AffectedParties = new[] { "System", "Users" }
                });
            }

            if (!request.IsReversible && request.ImpactLevel > 0.7)
            {
                concerns.Add(new EthicalConcern
                {
                    RelatedPrinciple = EthicalPrinciple.SafeSelfImprovement,
                    Description = "High-impact irreversible modification",
                    Level = ConcernLevel.High,
                    RecommendedAction = "Require extensive review and testing before approval"
                });
            }

            EthicalClearance clearance = violations.Count > 0
                ? EthicalClearance.Denied(
                    "Self-modification violates ethical principles",
                    violations,
                    _corePrinciples)
                : EthicalClearance.RequiresApproval(
                    "Self-modification requires mandatory human approval",
                    concerns,
                    _corePrinciples);

            await LogEvaluationAsync(
                "SelfModification",
                $"{request.Type}: {request.Description}",
                request.ActionContext,
                clearance,
                ct);

            return Result<EthicalClearance, string>.Success(clearance);
        }
        catch (InvalidOperationException ex)
        {
            return Result<EthicalClearance, string>.Failure($"Self-modification evaluation failed: {ex.Message}");
        }
        catch (IOException ex)
        {
            return Result<EthicalClearance, string>.Failure($"Self-modification evaluation failed: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task ReportEthicalConcernAsync(
        EthicalConcern concern,
        ActionContext context,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(concern);
        ArgumentNullException.ThrowIfNull(context);

        EthicalClearance clearance = new EthicalClearance
        {
            IsPermitted = true,
            Level = EthicalClearanceLevel.PermittedWithConcerns,
            RelevantPrinciples = new[] { concern.RelatedPrinciple },
            Violations = Array.Empty<EthicalViolation>(),
            Concerns = new[] { concern },
            Reasoning = "Ethical concern reported"
        };

        await LogEvaluationAsync("ConcernReport", concern.Description, context, clearance, ct);
    }
}
