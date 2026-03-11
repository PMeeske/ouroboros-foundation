using Ouroboros.Core.Ethics;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class ActionContextTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var state = new Dictionary<string, object> { ["key"] = "value" };

        var sut = new ActionContext
        {
            AgentId = "agent-1",
            Environment = "production",
            State = state
        };

        sut.AgentId.Should().Be("agent-1");
        sut.Environment.Should().Be("production");
        sut.State.Should().ContainKey("key");
    }

    [Fact]
    public void Construction_WithOptionalProperties_SetsValues()
    {
        var recentActions = new[] { "action1", "action2" };
        var timestamp = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var sut = new ActionContext
        {
            AgentId = "agent-1",
            UserId = "user-42",
            Environment = "testing",
            State = new Dictionary<string, object>(),
            RecentActions = recentActions,
            Timestamp = timestamp
        };

        sut.UserId.Should().Be("user-42");
        sut.RecentActions.Should().HaveCount(2);
        sut.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void Construction_Defaults_UserId_IsNull()
    {
        var sut = new ActionContext
        {
            AgentId = "a",
            Environment = "dev",
            State = new Dictionary<string, object>()
        };

        sut.UserId.Should().BeNull();
    }

    [Fact]
    public void Construction_Defaults_RecentActions_IsEmpty()
    {
        var sut = new ActionContext
        {
            AgentId = "a",
            Environment = "dev",
            State = new Dictionary<string, object>()
        };

        sut.RecentActions.Should().BeEmpty();
    }

    [Fact]
    public void Construction_Defaults_Timestamp_IsRecentUtcNow()
    {
        var before = DateTime.UtcNow;

        var sut = new ActionContext
        {
            AgentId = "a",
            Environment = "dev",
            State = new Dictionary<string, object>()
        };

        var after = DateTime.UtcNow;
        sut.Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var state = new Dictionary<string, object>();
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new ActionContext { AgentId = "a", Environment = "dev", State = state, Timestamp = timestamp };
        var b = new ActionContext { AgentId = "a", Environment = "dev", State = state, Timestamp = timestamp };

        a.Should().Be(b);
    }

    [Fact]
    public void RecordEquality_DifferentValues_AreNotEqual()
    {
        var state = new Dictionary<string, object>();
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new ActionContext { AgentId = "a", Environment = "dev", State = state, Timestamp = timestamp };
        var b = new ActionContext { AgentId = "b", Environment = "dev", State = state, Timestamp = timestamp };

        a.Should().NotBe(b);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var state = new Dictionary<string, object>();
        var original = new ActionContext { AgentId = "a", Environment = "dev", State = state };

        var modified = original with { AgentId = "b" };

        modified.AgentId.Should().Be("b");
        modified.Environment.Should().Be("dev");
        original.AgentId.Should().Be("a");
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class EthicalClearanceTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var principles = new[] { EthicalPrinciple.DoNoHarm };
        var violations = Array.Empty<EthicalViolation>();
        var concerns = Array.Empty<EthicalConcern>();

        var sut = new EthicalClearance
        {
            IsPermitted = true,
            Level = EthicalClearanceLevel.Permitted,
            RelevantPrinciples = principles,
            Violations = violations,
            Concerns = concerns,
            Reasoning = "All clear"
        };

        sut.IsPermitted.Should().BeTrue();
        sut.Level.Should().Be(EthicalClearanceLevel.Permitted);
        sut.RelevantPrinciples.Should().HaveCount(1);
        sut.Violations.Should().BeEmpty();
        sut.Concerns.Should().BeEmpty();
        sut.Reasoning.Should().Be("All clear");
    }

    [Fact]
    public void Construction_Defaults_RecommendedMitigations_IsEmpty()
    {
        var sut = new EthicalClearance
        {
            IsPermitted = true,
            Level = EthicalClearanceLevel.Permitted,
            RelevantPrinciples = Array.Empty<EthicalPrinciple>(),
            Violations = Array.Empty<EthicalViolation>(),
            Concerns = Array.Empty<EthicalConcern>(),
            Reasoning = "ok"
        };

        sut.RecommendedMitigations.Should().BeEmpty();
    }

    [Fact]
    public void Construction_Defaults_ConfidenceScore_IsOne()
    {
        var sut = new EthicalClearance
        {
            IsPermitted = true,
            Level = EthicalClearanceLevel.Permitted,
            RelevantPrinciples = Array.Empty<EthicalPrinciple>(),
            Violations = Array.Empty<EthicalViolation>(),
            Concerns = Array.Empty<EthicalConcern>(),
            Reasoning = "ok"
        };

        sut.ConfidenceScore.Should().Be(1.0);
    }

    [Fact]
    public void Permitted_Factory_ReturnsPermittedClearance()
    {
        var result = EthicalClearance.Permitted("Action is safe");

        result.IsPermitted.Should().BeTrue();
        result.Level.Should().Be(EthicalClearanceLevel.Permitted);
        result.Reasoning.Should().Be("Action is safe");
        result.Violations.Should().BeEmpty();
        result.Concerns.Should().BeEmpty();
        result.RelevantPrinciples.Should().BeEmpty();
        result.ConfidenceScore.Should().Be(1.0);
    }

    [Fact]
    public void Permitted_Factory_WithPrinciples_SetsPrinciples()
    {
        var principles = new[] { EthicalPrinciple.DoNoHarm, EthicalPrinciple.Honesty };

        var result = EthicalClearance.Permitted("Safe", principles);

        result.RelevantPrinciples.Should().HaveCount(2);
        result.RelevantPrinciples.Should().Contain(EthicalPrinciple.DoNoHarm);
    }

    [Fact]
    public void Permitted_Factory_WithConfidence_SetsConfidence()
    {
        var result = EthicalClearance.Permitted("ok", confidenceScore: 0.85);

        result.ConfidenceScore.Should().Be(0.85);
    }

    [Fact]
    public void Denied_Factory_ReturnsDeniedClearance()
    {
        var violation = new EthicalViolation
        {
            ViolatedPrinciple = EthicalPrinciple.DoNoHarm,
            Description = "Causes harm",
            Severity = ViolationSeverity.Critical,
            Evidence = "Direct evidence",
            AffectedParties = new[] { "user" }
        };
        var violations = new[] { violation };

        var result = EthicalClearance.Denied("Harmful action", violations);

        result.IsPermitted.Should().BeFalse();
        result.Level.Should().Be(EthicalClearanceLevel.Denied);
        result.Reasoning.Should().Be("Harmful action");
        result.Violations.Should().HaveCount(1);
        result.Concerns.Should().BeEmpty();
        result.ConfidenceScore.Should().Be(1.0);
    }

    [Fact]
    public void Denied_Factory_WithPrinciples_SetsPrinciples()
    {
        var violations = new[]
        {
            new EthicalViolation
            {
                ViolatedPrinciple = EthicalPrinciple.Privacy,
                Description = "Data leak",
                Severity = ViolationSeverity.High,
                Evidence = "evidence",
                AffectedParties = new[] { "users" }
            }
        };
        var principles = new[] { EthicalPrinciple.Privacy };

        var result = EthicalClearance.Denied("Privacy violation", violations, principles);

        result.RelevantPrinciples.Should().HaveCount(1);
        result.RelevantPrinciples.Should().Contain(EthicalPrinciple.Privacy);
    }

    [Fact]
    public void RequiresApproval_Factory_ReturnsRequiresHumanApproval()
    {
        var result = EthicalClearance.RequiresApproval("Needs human review");

        result.IsPermitted.Should().BeFalse();
        result.Level.Should().Be(EthicalClearanceLevel.RequiresHumanApproval);
        result.Reasoning.Should().Be("Needs human review");
        result.Violations.Should().BeEmpty();
        result.Concerns.Should().BeEmpty();
    }

    [Fact]
    public void RequiresApproval_Factory_WithConcerns_SetsConcerns()
    {
        var concern = new EthicalConcern
        {
            RelatedPrinciple = EthicalPrinciple.Transparency,
            Description = "Opaque reasoning",
            Level = ConcernLevel.Medium,
            RecommendedAction = "Add explanation"
        };
        var concerns = new[] { concern };

        var result = EthicalClearance.RequiresApproval("Unclear", concerns);

        result.Concerns.Should().HaveCount(1);
    }

    [Fact]
    public void RequiresApproval_Factory_WithPrinciples_SetsPrinciples()
    {
        var principles = new[] { EthicalPrinciple.HumanOversight };

        var result = EthicalClearance.RequiresApproval("Review needed", null, principles);

        result.RelevantPrinciples.Should().HaveCount(1);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class EthicalConcernTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var sut = new EthicalConcern
        {
            RelatedPrinciple = EthicalPrinciple.Fairness,
            Description = "Potential bias detected",
            Level = ConcernLevel.High,
            RecommendedAction = "Review for bias"
        };

        sut.RelatedPrinciple.Should().Be(EthicalPrinciple.Fairness);
        sut.Description.Should().Be("Potential bias detected");
        sut.Level.Should().Be(ConcernLevel.High);
        sut.RecommendedAction.Should().Be("Review for bias");
    }

    [Fact]
    public void Construction_Defaults_Id_IsNewGuid()
    {
        var sut = new EthicalConcern
        {
            RelatedPrinciple = EthicalPrinciple.Fairness,
            Description = "concern",
            Level = ConcernLevel.Low,
            RecommendedAction = "review"
        };

        sut.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Construction_Defaults_RaisedAt_IsRecentUtcNow()
    {
        var before = DateTime.UtcNow;

        var sut = new EthicalConcern
        {
            RelatedPrinciple = EthicalPrinciple.Fairness,
            Description = "concern",
            Level = ConcernLevel.Low,
            RecommendedAction = "review"
        };

        var after = DateTime.UtcNow;
        sut.RaisedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void TwoConcerns_HaveDifferentIds()
    {
        var a = new EthicalConcern
        {
            RelatedPrinciple = EthicalPrinciple.Fairness,
            Description = "concern",
            Level = ConcernLevel.Low,
            RecommendedAction = "review"
        };
        var b = new EthicalConcern
        {
            RelatedPrinciple = EthicalPrinciple.Fairness,
            Description = "concern",
            Level = ConcernLevel.Low,
            RecommendedAction = "review"
        };

        a.Id.Should().NotBe(b.Id);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class EthicalPrincipleTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var sut = new EthicalPrinciple
        {
            Id = "custom_principle",
            Name = "Custom Principle",
            Description = "A custom ethical principle",
            Category = EthicalPrincipleCategory.Safety,
            Priority = 0.75,
            IsMandatory = false
        };

        sut.Id.Should().Be("custom_principle");
        sut.Name.Should().Be("Custom Principle");
        sut.Description.Should().Be("A custom ethical principle");
        sut.Category.Should().Be(EthicalPrincipleCategory.Safety);
        sut.Priority.Should().Be(0.75);
        sut.IsMandatory.Should().BeFalse();
    }

    [Fact]
    public void DoNoHarm_HasCorrectProperties()
    {
        var sut = EthicalPrinciple.DoNoHarm;

        sut.Id.Should().Be("do_no_harm");
        sut.Name.Should().Be("Do No Harm");
        sut.Category.Should().Be(EthicalPrincipleCategory.Safety);
        sut.Priority.Should().Be(1.0);
        sut.IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void RespectAutonomy_HasCorrectProperties()
    {
        var sut = EthicalPrinciple.RespectAutonomy;

        sut.Id.Should().Be("respect_autonomy");
        sut.Category.Should().Be(EthicalPrincipleCategory.Autonomy);
        sut.Priority.Should().Be(0.95);
        sut.IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void Honesty_HasCorrectProperties()
    {
        var sut = EthicalPrinciple.Honesty;

        sut.Id.Should().Be("honesty");
        sut.Category.Should().Be(EthicalPrincipleCategory.Transparency);
        sut.Priority.Should().Be(0.90);
        sut.IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void Privacy_HasCorrectProperties()
    {
        var sut = EthicalPrinciple.Privacy;

        sut.Id.Should().Be("privacy");
        sut.Category.Should().Be(EthicalPrincipleCategory.Privacy);
        sut.Priority.Should().Be(0.90);
        sut.IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void Fairness_HasCorrectProperties()
    {
        var sut = EthicalPrinciple.Fairness;

        sut.Id.Should().Be("fairness");
        sut.Category.Should().Be(EthicalPrincipleCategory.Fairness);
        sut.Priority.Should().Be(0.85);
        sut.IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void Transparency_HasCorrectProperties()
    {
        var sut = EthicalPrinciple.Transparency;

        sut.Id.Should().Be("transparency");
        sut.Category.Should().Be(EthicalPrincipleCategory.Transparency);
        sut.Priority.Should().Be(0.80);
        sut.IsMandatory.Should().BeFalse();
    }

    [Fact]
    public void HumanOversight_HasCorrectProperties()
    {
        var sut = EthicalPrinciple.HumanOversight;

        sut.Id.Should().Be("human_oversight");
        sut.Category.Should().Be(EthicalPrincipleCategory.Autonomy);
        sut.Priority.Should().Be(0.95);
        sut.IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void PreventMisuse_HasCorrectProperties()
    {
        var sut = EthicalPrinciple.PreventMisuse;

        sut.Id.Should().Be("prevent_misuse");
        sut.Category.Should().Be(EthicalPrincipleCategory.Safety);
        sut.Priority.Should().Be(1.0);
        sut.IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void SafeSelfImprovement_HasCorrectProperties()
    {
        var sut = EthicalPrinciple.SafeSelfImprovement;

        sut.Id.Should().Be("safe_self_improvement");
        sut.Category.Should().Be(EthicalPrincipleCategory.Integrity);
        sut.Priority.Should().Be(1.0);
        sut.IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void Corrigibility_HasCorrectProperties()
    {
        var sut = EthicalPrinciple.Corrigibility;

        sut.Id.Should().Be("corrigibility");
        sut.Category.Should().Be(EthicalPrincipleCategory.Autonomy);
        sut.Priority.Should().Be(1.0);
        sut.IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void GetCorePrinciples_ReturnsTenPrinciples()
    {
        var principles = EthicalPrinciple.GetCorePrinciples();

        principles.Should().HaveCount(10);
    }

    [Fact]
    public void GetCorePrinciples_ContainsAllPredefinedPrinciples()
    {
        var principles = EthicalPrinciple.GetCorePrinciples();

        principles.Should().Contain(EthicalPrinciple.DoNoHarm);
        principles.Should().Contain(EthicalPrinciple.RespectAutonomy);
        principles.Should().Contain(EthicalPrinciple.Honesty);
        principles.Should().Contain(EthicalPrinciple.Privacy);
        principles.Should().Contain(EthicalPrinciple.Fairness);
        principles.Should().Contain(EthicalPrinciple.Transparency);
        principles.Should().Contain(EthicalPrinciple.HumanOversight);
        principles.Should().Contain(EthicalPrinciple.PreventMisuse);
        principles.Should().Contain(EthicalPrinciple.SafeSelfImprovement);
        principles.Should().Contain(EthicalPrinciple.Corrigibility);
    }

    [Fact]
    public void GetCorePrinciples_AllMandatoryExceptTransparency()
    {
        var principles = EthicalPrinciple.GetCorePrinciples();

        var nonMandatory = principles.Where(p => !p.IsMandatory).ToList();
        nonMandatory.Should().ContainSingle()
            .Which.Id.Should().Be("transparency");
    }

    [Fact]
    public void RecordEquality_SameId_AreEqual()
    {
        var a = new EthicalPrinciple
        {
            Id = "test", Name = "Test", Description = "desc",
            Category = EthicalPrincipleCategory.Safety, Priority = 1.0, IsMandatory = true
        };
        var b = new EthicalPrinciple
        {
            Id = "test", Name = "Test", Description = "desc",
            Category = EthicalPrincipleCategory.Safety, Priority = 1.0, IsMandatory = true
        };

        a.Should().Be(b);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class EthicalViolationTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var sut = new EthicalViolation
        {
            ViolatedPrinciple = EthicalPrinciple.DoNoHarm,
            Description = "Action causes harm",
            Severity = ViolationSeverity.Critical,
            Evidence = "Evidence of harm",
            AffectedParties = new[] { "user-1", "user-2" }
        };

        sut.ViolatedPrinciple.Should().Be(EthicalPrinciple.DoNoHarm);
        sut.Description.Should().Be("Action causes harm");
        sut.Severity.Should().Be(ViolationSeverity.Critical);
        sut.Evidence.Should().Be("Evidence of harm");
        sut.AffectedParties.Should().HaveCount(2);
    }

    [Fact]
    public void Construction_Defaults_DetectedAt_IsRecentUtcNow()
    {
        var before = DateTime.UtcNow;

        var sut = new EthicalViolation
        {
            ViolatedPrinciple = EthicalPrinciple.DoNoHarm,
            Description = "harm",
            Severity = ViolationSeverity.Low,
            Evidence = "evidence",
            AffectedParties = Array.Empty<string>()
        };

        var after = DateTime.UtcNow;
        sut.DetectedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var original = new EthicalViolation
        {
            ViolatedPrinciple = EthicalPrinciple.DoNoHarm,
            Description = "harm",
            Severity = ViolationSeverity.Low,
            Evidence = "evidence",
            AffectedParties = Array.Empty<string>()
        };

        var modified = original with { Severity = ViolationSeverity.Critical };

        modified.Severity.Should().Be(ViolationSeverity.Critical);
        original.Severity.Should().Be(ViolationSeverity.Low);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class EthicsAuditEntryTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var clearance = EthicalClearance.Permitted("ok");
        var timestamp = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var sut = new EthicsAuditEntry
        {
            Timestamp = timestamp,
            AgentId = "agent-1",
            EvaluationType = "Action",
            Description = "Test evaluation",
            Clearance = clearance
        };

        sut.Timestamp.Should().Be(timestamp);
        sut.AgentId.Should().Be("agent-1");
        sut.EvaluationType.Should().Be("Action");
        sut.Description.Should().Be("Test evaluation");
        sut.Clearance.Should().Be(clearance);
    }

    [Fact]
    public void Construction_Defaults_Id_IsNewGuid()
    {
        var sut = new EthicsAuditEntry
        {
            Timestamp = DateTime.UtcNow,
            AgentId = "a",
            EvaluationType = "Action",
            Description = "desc",
            Clearance = EthicalClearance.Permitted("ok")
        };

        sut.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Construction_Defaults_UserId_IsNull()
    {
        var sut = new EthicsAuditEntry
        {
            Timestamp = DateTime.UtcNow,
            AgentId = "a",
            EvaluationType = "Action",
            Description = "desc",
            Clearance = EthicalClearance.Permitted("ok")
        };

        sut.UserId.Should().BeNull();
    }

    [Fact]
    public void Construction_Defaults_Context_IsEmpty()
    {
        var sut = new EthicsAuditEntry
        {
            Timestamp = DateTime.UtcNow,
            AgentId = "a",
            EvaluationType = "Action",
            Description = "desc",
            Clearance = EthicalClearance.Permitted("ok")
        };

        sut.Context.Should().BeEmpty();
    }

    [Fact]
    public void Construction_WithOptionalProperties_SetsValues()
    {
        var context = new Dictionary<string, object> { ["source"] = "test" };

        var sut = new EthicsAuditEntry
        {
            Timestamp = DateTime.UtcNow,
            AgentId = "a",
            UserId = "user-1",
            EvaluationType = "Plan",
            Description = "plan eval",
            Clearance = EthicalClearance.Permitted("ok"),
            Context = context
        };

        sut.UserId.Should().Be("user-1");
        sut.Context.Should().ContainKey("source");
    }

    [Fact]
    public void TwoEntries_HaveDifferentIds()
    {
        var a = new EthicsAuditEntry
        {
            Timestamp = DateTime.UtcNow, AgentId = "a",
            EvaluationType = "Action", Description = "d",
            Clearance = EthicalClearance.Permitted("ok")
        };
        var b = new EthicsAuditEntry
        {
            Timestamp = DateTime.UtcNow, AgentId = "a",
            EvaluationType = "Action", Description = "d",
            Clearance = EthicalClearance.Permitted("ok")
        };

        a.Id.Should().NotBe(b.Id);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class GoalTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var id = Guid.NewGuid();

        var sut = new Goal
        {
            Id = id,
            Description = "Improve performance",
            Type = "optimization",
            Priority = 0.8
        };

        sut.Id.Should().Be(id);
        sut.Description.Should().Be("Improve performance");
        sut.Type.Should().Be("optimization");
        sut.Priority.Should().Be(0.8);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var id = Guid.NewGuid();
        var a = new Goal { Id = id, Description = "d", Type = "t", Priority = 0.5 };
        var b = new Goal { Id = id, Description = "d", Type = "t", Priority = 0.5 };

        a.Should().Be(b);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class PlanTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var steps = new[]
        {
            new PlanStep
            {
                Action = "step1",
                Parameters = new Dictionary<string, object>(),
                ExpectedOutcome = "outcome1"
            }
        };

        var sut = new Plan
        {
            Goal = "Achieve objective",
            Steps = steps
        };

        sut.Goal.Should().Be("Achieve objective");
        sut.Steps.Should().HaveCount(1);
    }

    [Fact]
    public void Construction_Defaults_ConfidenceScores_IsEmpty()
    {
        var sut = new Plan
        {
            Goal = "g",
            Steps = Array.Empty<PlanStep>()
        };

        sut.ConfidenceScores.Should().BeEmpty();
    }

    [Fact]
    public void Construction_Defaults_CreatedAt_IsRecentUtcNow()
    {
        var before = DateTime.UtcNow;

        var sut = new Plan
        {
            Goal = "g",
            Steps = Array.Empty<PlanStep>()
        };

        var after = DateTime.UtcNow;
        sut.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Construction_WithConfidenceScores_SetsValues()
    {
        var scores = new Dictionary<string, double> { ["feasibility"] = 0.9 };

        var sut = new Plan
        {
            Goal = "g",
            Steps = Array.Empty<PlanStep>(),
            ConfidenceScores = scores
        };

        sut.ConfidenceScores.Should().ContainKey("feasibility");
        sut.ConfidenceScores["feasibility"].Should().Be(0.9);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class PlanStepTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var parameters = new Dictionary<string, object> { ["target"] = "file.txt" };

        var sut = new PlanStep
        {
            Action = "read_file",
            Parameters = parameters,
            ExpectedOutcome = "File contents retrieved"
        };

        sut.Action.Should().Be("read_file");
        sut.Parameters.Should().ContainKey("target");
        sut.ExpectedOutcome.Should().Be("File contents retrieved");
    }

    [Fact]
    public void Construction_Defaults_ConfidenceScore_IsOne()
    {
        var sut = new PlanStep
        {
            Action = "a",
            Parameters = new Dictionary<string, object>(),
            ExpectedOutcome = "o"
        };

        sut.ConfidenceScore.Should().Be(1.0);
    }

    [Fact]
    public void Construction_WithConfidenceScore_SetsValue()
    {
        var sut = new PlanStep
        {
            Action = "a",
            Parameters = new Dictionary<string, object>(),
            ExpectedOutcome = "o",
            ConfidenceScore = 0.7
        };

        sut.ConfidenceScore.Should().Be(0.7);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class ProposedActionTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var parameters = new Dictionary<string, object> { ["path"] = "/etc/passwd" };
        var effects = new[] { "Reads sensitive file" };

        var sut = new ProposedAction
        {
            ActionType = "file_operation",
            Description = "Read system file",
            Parameters = parameters,
            PotentialEffects = effects
        };

        sut.ActionType.Should().Be("file_operation");
        sut.Description.Should().Be("Read system file");
        sut.Parameters.Should().ContainKey("path");
        sut.PotentialEffects.Should().HaveCount(1);
    }

    [Fact]
    public void Construction_Defaults_TargetEntity_IsNull()
    {
        var sut = new ProposedAction
        {
            ActionType = "t",
            Description = "d",
            Parameters = new Dictionary<string, object>(),
            PotentialEffects = Array.Empty<string>()
        };

        sut.TargetEntity.Should().BeNull();
    }

    [Fact]
    public void Construction_Defaults_Metadata_IsEmpty()
    {
        var sut = new ProposedAction
        {
            ActionType = "t",
            Description = "d",
            Parameters = new Dictionary<string, object>(),
            PotentialEffects = Array.Empty<string>()
        };

        sut.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Construction_WithOptionalProperties_SetsValues()
    {
        var metadata = new Dictionary<string, object> { ["source"] = "agent" };

        var sut = new ProposedAction
        {
            ActionType = "network_request",
            Description = "Call API",
            Parameters = new Dictionary<string, object>(),
            PotentialEffects = new[] { "External call" },
            TargetEntity = "https://api.example.com",
            Metadata = metadata
        };

        sut.TargetEntity.Should().Be("https://api.example.com");
        sut.Metadata.Should().ContainKey("source");
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class SelfModificationRequestTests
{
    private static ActionContext CreateContext() => new()
    {
        AgentId = "agent-1",
        Environment = "testing",
        State = new Dictionary<string, object>()
    };

    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var sut = new SelfModificationRequest
        {
            Type = ModificationType.CapabilityAddition,
            Description = "Add new skill",
            Justification = "Improve capabilities",
            ActionContext = CreateContext(),
            ExpectedImprovements = new[] { "Better performance" },
            PotentialRisks = new[] { "Instability" },
            IsReversible = true,
            ImpactLevel = 0.5
        };

        sut.Type.Should().Be(ModificationType.CapabilityAddition);
        sut.Description.Should().Be("Add new skill");
        sut.Justification.Should().Be("Improve capabilities");
        sut.ActionContext.AgentId.Should().Be("agent-1");
        sut.ExpectedImprovements.Should().HaveCount(1);
        sut.PotentialRisks.Should().HaveCount(1);
        sut.IsReversible.Should().BeTrue();
        sut.ImpactLevel.Should().Be(0.5);
    }

    [Fact]
    public void Construction_EthicsModification_SetsType()
    {
        var sut = new SelfModificationRequest
        {
            Type = ModificationType.EthicsModification,
            Description = "Relax constraint",
            Justification = "Performance",
            ActionContext = CreateContext(),
            ExpectedImprovements = Array.Empty<string>(),
            PotentialRisks = new[] { "Safety degradation" },
            IsReversible = false,
            ImpactLevel = 0.95
        };

        sut.Type.Should().Be(ModificationType.EthicsModification);
        sut.IsReversible.Should().BeFalse();
        sut.ImpactLevel.Should().Be(0.95);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class SkillTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var sut = new Skill
        {
            Name = "CodeReview",
            Description = "Reviews code for quality"
        };

        sut.Name.Should().Be("CodeReview");
        sut.Description.Should().Be("Reviews code for quality");
    }

    [Fact]
    public void Construction_Defaults_Prerequisites_IsEmpty()
    {
        var sut = new Skill { Name = "s", Description = "d" };
        sut.Prerequisites.Should().BeEmpty();
    }

    [Fact]
    public void Construction_Defaults_Steps_IsEmpty()
    {
        var sut = new Skill { Name = "s", Description = "d" };
        sut.Steps.Should().BeEmpty();
    }

    [Fact]
    public void Construction_Defaults_SuccessRate_IsZero()
    {
        var sut = new Skill { Name = "s", Description = "d" };
        sut.SuccessRate.Should().Be(0.0);
    }

    [Fact]
    public void Construction_Defaults_UsageCount_IsZero()
    {
        var sut = new Skill { Name = "s", Description = "d" };
        sut.UsageCount.Should().Be(0);
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var steps = new[]
        {
            new PlanStep
            {
                Action = "analyze",
                Parameters = new Dictionary<string, object>(),
                ExpectedOutcome = "Analysis complete"
            }
        };

        var sut = new Skill
        {
            Name = "Analysis",
            Description = "Analyzes data",
            Prerequisites = new[] { "data_access" },
            Steps = steps,
            SuccessRate = 0.85,
            UsageCount = 42
        };

        sut.Prerequisites.Should().HaveCount(1);
        sut.Steps.Should().HaveCount(1);
        sut.SuccessRate.Should().Be(0.85);
        sut.UsageCount.Should().Be(42);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class HumanApprovalRequestTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var clearance = EthicalClearance.RequiresApproval("Needs review");

        var sut = new HumanApprovalRequest
        {
            Category = "action",
            Description = "Delete user data",
            Clearance = clearance
        };

        sut.Category.Should().Be("action");
        sut.Description.Should().Be("Delete user data");
        sut.Clearance.Should().Be(clearance);
    }

    [Fact]
    public void Construction_Defaults_Id_IsNewGuid()
    {
        var sut = new HumanApprovalRequest
        {
            Category = "action",
            Description = "d",
            Clearance = EthicalClearance.RequiresApproval("r")
        };

        sut.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Construction_Defaults_Context_IsEmpty()
    {
        var sut = new HumanApprovalRequest
        {
            Category = "action",
            Description = "d",
            Clearance = EthicalClearance.RequiresApproval("r")
        };

        sut.Context.Should().BeEmpty();
    }

    [Fact]
    public void Construction_Defaults_Timeout_IsFiveMinutes()
    {
        var sut = new HumanApprovalRequest
        {
            Category = "action",
            Description = "d",
            Clearance = EthicalClearance.RequiresApproval("r")
        };

        sut.Timeout.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void Construction_Defaults_CreatedAt_IsRecentUtcNow()
    {
        var before = DateTime.UtcNow;

        var sut = new HumanApprovalRequest
        {
            Category = "action",
            Description = "d",
            Clearance = EthicalClearance.RequiresApproval("r")
        };

        var after = DateTime.UtcNow;
        sut.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Construction_NullTimeout_IsAllowed()
    {
        var sut = new HumanApprovalRequest
        {
            Category = "action",
            Description = "d",
            Clearance = EthicalClearance.RequiresApproval("r"),
            Timeout = null
        };

        sut.Timeout.Should().BeNull();
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class HumanApprovalResponseTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var requestId = Guid.NewGuid();

        var sut = new HumanApprovalResponse
        {
            RequestId = requestId,
            Decision = HumanApprovalDecision.Approved
        };

        sut.RequestId.Should().Be(requestId);
        sut.Decision.Should().Be(HumanApprovalDecision.Approved);
    }

    [Fact]
    public void Construction_Defaults_ReviewerComments_IsNull()
    {
        var sut = new HumanApprovalResponse
        {
            RequestId = Guid.NewGuid(),
            Decision = HumanApprovalDecision.Approved
        };

        sut.ReviewerComments.Should().BeNull();
    }

    [Fact]
    public void Construction_Defaults_Modifications_IsNull()
    {
        var sut = new HumanApprovalResponse
        {
            RequestId = Guid.NewGuid(),
            Decision = HumanApprovalDecision.Approved
        };

        sut.Modifications.Should().BeNull();
    }

    [Fact]
    public void Construction_Defaults_ReviewerId_IsNull()
    {
        var sut = new HumanApprovalResponse
        {
            RequestId = Guid.NewGuid(),
            Decision = HumanApprovalDecision.Approved
        };

        sut.ReviewerId.Should().BeNull();
    }

    [Fact]
    public void Approved_Factory_ReturnsApprovedDecision()
    {
        var requestId = Guid.NewGuid();

        var sut = HumanApprovalResponse.Approved(requestId);

        sut.RequestId.Should().Be(requestId);
        sut.Decision.Should().Be(HumanApprovalDecision.Approved);
        sut.ReviewerId.Should().BeNull();
        sut.ReviewerComments.Should().BeNull();
    }

    [Fact]
    public void Approved_Factory_WithReviewerAndComments_SetsValues()
    {
        var requestId = Guid.NewGuid();

        var sut = HumanApprovalResponse.Approved(requestId, "admin", "Looks good");

        sut.ReviewerId.Should().Be("admin");
        sut.ReviewerComments.Should().Be("Looks good");
    }

    [Fact]
    public void Rejected_Factory_ReturnsRejectedDecision()
    {
        var requestId = Guid.NewGuid();

        var sut = HumanApprovalResponse.Rejected(requestId, "Too risky");

        sut.RequestId.Should().Be(requestId);
        sut.Decision.Should().Be(HumanApprovalDecision.Rejected);
        sut.ReviewerComments.Should().Be("Too risky");
    }

    [Fact]
    public void Rejected_Factory_WithReviewer_SetsReviewerId()
    {
        var requestId = Guid.NewGuid();

        var sut = HumanApprovalResponse.Rejected(requestId, "No", "supervisor");

        sut.ReviewerId.Should().Be("supervisor");
    }

    [Fact]
    public void TimedOut_Factory_ReturnsTimedOutDecision()
    {
        var requestId = Guid.NewGuid();

        var sut = HumanApprovalResponse.TimedOut(requestId);

        sut.RequestId.Should().Be(requestId);
        sut.Decision.Should().Be(HumanApprovalDecision.TimedOut);
        sut.ReviewerComments.Should().Contain("timed out");
    }

    [Fact]
    public void RespondedAt_DefaultsToRecentUtcNow()
    {
        var before = DateTime.UtcNow;

        var sut = HumanApprovalResponse.Approved(Guid.NewGuid());

        var after = DateTime.UtcNow;
        sut.RespondedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class SkillUsageContextTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var skill = new Skill { Name = "CodeReview", Description = "Reviews code" };
        var context = new ActionContext
        {
            AgentId = "agent-1",
            Environment = "testing",
            State = new Dictionary<string, object>()
        };

        var sut = new SkillUsageContext
        {
            Skill = skill,
            ActionContext = context,
            Goal = "Improve code quality"
        };

        sut.Skill.Name.Should().Be("CodeReview");
        sut.ActionContext.AgentId.Should().Be("agent-1");
        sut.Goal.Should().Be("Improve code quality");
    }

    [Fact]
    public void Construction_Defaults_InputParameters_IsEmpty()
    {
        var sut = new SkillUsageContext
        {
            Skill = new Skill { Name = "s", Description = "d" },
            ActionContext = new ActionContext
            {
                AgentId = "a", Environment = "e",
                State = new Dictionary<string, object>()
            },
            Goal = "g"
        };

        sut.InputParameters.Should().BeEmpty();
    }

    [Fact]
    public void Construction_Defaults_HistoricalSuccessRate_IsZero()
    {
        var sut = new SkillUsageContext
        {
            Skill = new Skill { Name = "s", Description = "d" },
            ActionContext = new ActionContext
            {
                AgentId = "a", Environment = "e",
                State = new Dictionary<string, object>()
            },
            Goal = "g"
        };

        sut.HistoricalSuccessRate.Should().Be(0.0);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class PlanContextTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var plan = new Plan
        {
            Goal = "Deploy update",
            Steps = new[]
            {
                new PlanStep
                {
                    Action = "build",
                    Parameters = new Dictionary<string, object>(),
                    ExpectedOutcome = "Build succeeds"
                }
            }
        };
        var context = new ActionContext
        {
            AgentId = "agent-1",
            Environment = "production",
            State = new Dictionary<string, object>()
        };

        var sut = new PlanContext
        {
            Plan = plan,
            ActionContext = context
        };

        sut.Plan.Goal.Should().Be("Deploy update");
        sut.ActionContext.Environment.Should().Be("production");
    }

    [Fact]
    public void Construction_Defaults_EstimatedRisk_IsHalf()
    {
        var sut = new PlanContext
        {
            Plan = new Plan { Goal = "g", Steps = Array.Empty<PlanStep>() },
            ActionContext = new ActionContext
            {
                AgentId = "a", Environment = "e",
                State = new Dictionary<string, object>()
            }
        };

        sut.EstimatedRisk.Should().Be(0.5);
    }

    [Fact]
    public void Construction_Defaults_ExpectedBenefits_IsEmpty()
    {
        var sut = new PlanContext
        {
            Plan = new Plan { Goal = "g", Steps = Array.Empty<PlanStep>() },
            ActionContext = new ActionContext
            {
                AgentId = "a", Environment = "e",
                State = new Dictionary<string, object>()
            }
        };

        sut.ExpectedBenefits.Should().BeEmpty();
    }

    [Fact]
    public void Construction_Defaults_PotentialConsequences_IsEmpty()
    {
        var sut = new PlanContext
        {
            Plan = new Plan { Goal = "g", Steps = Array.Empty<PlanStep>() },
            ActionContext = new ActionContext
            {
                AgentId = "a", Environment = "e",
                State = new Dictionary<string, object>()
            }
        };

        sut.PotentialConsequences.Should().BeEmpty();
    }

    [Fact]
    public void Construction_WithOptionalProperties_SetsValues()
    {
        var sut = new PlanContext
        {
            Plan = new Plan { Goal = "g", Steps = Array.Empty<PlanStep>() },
            ActionContext = new ActionContext
            {
                AgentId = "a", Environment = "e",
                State = new Dictionary<string, object>()
            },
            EstimatedRisk = 0.9,
            ExpectedBenefits = new[] { "Faster processing" },
            PotentialConsequences = new[] { "Downtime" }
        };

        sut.EstimatedRisk.Should().Be(0.9);
        sut.ExpectedBenefits.Should().HaveCount(1);
        sut.PotentialConsequences.Should().HaveCount(1);
    }
}
