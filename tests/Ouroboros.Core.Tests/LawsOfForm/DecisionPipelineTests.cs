// <copyright file="DecisionPipelineTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;
using Ouroboros.Abstractions.Monads;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for <see cref="DecisionPipeline"/> which composes multiple auditable decisions.
/// </summary>
[Trait("Category", "Unit")]
public class DecisionPipelineTests
{
    // ──────────── Evaluate (AND logic) ────────────

    [Fact]
    public void Evaluate_NullCriteria_ThrowsArgumentNullException()
    {
        Action act = () => DecisionPipeline.Evaluate<string, string>(
            "input",
            null!,
            x => x);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("criteria");
    }

    [Fact]
    public void Evaluate_EmptyCriteria_ThrowsArgumentException()
    {
        Action act = () => DecisionPipeline.Evaluate<string, string>(
            "input",
            Array.Empty<Func<string, AuditableDecision<string>>>(),
            x => x);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("criteria");
    }

    [Fact]
    public void Evaluate_AllCriteriaPass_ReturnsApprovedWithTransformedOutput()
    {
        var criteria = new Func<int, AuditableDecision<int>>[]
        {
            x => AuditableDecision<int>.Approve(x, "Criterion passed"),
            x => AuditableDecision<int>.Approve(x, "Another passed"),
        };

        var result = DecisionPipeline.Evaluate(
            42,
            criteria,
            x => x.ToString());

        result.State.Should().Be(Form.Mark);
        result.Value.Should().Be("42");
        result.Reasoning.Should().Contain("All 2 criteria passed");
    }

    [Fact]
    public void Evaluate_OneCriterionFails_ReturnsRejected()
    {
        var criteria = new Func<string, AuditableDecision<string>>[]
        {
            x => AuditableDecision<string>.Approve(x, "Passed"),
            x => AuditableDecision<string>.Reject("Failed", "Bad input"),
        };

        var result = DecisionPipeline.Evaluate(
            "test",
            criteria,
            x => x.ToUpper());

        result.State.Should().Be(Form.Void);
        result.Reasoning.Should().Contain("Failed criteria");
        result.Reasoning.Should().Contain("Criterion 2");
    }

    [Fact]
    public void Evaluate_OneCriterionInconclusive_ReturnsInconclusive()
    {
        var criteria = new Func<string, AuditableDecision<string>>[]
        {
            x => AuditableDecision<string>.Approve(x, "Passed"),
            x => AuditableDecision<string>.Inconclusive(0.7, "Uncertain"),
        };

        var result = DecisionPipeline.Evaluate(
            "test",
            criteria,
            x => x.ToUpper());

        result.State.Should().Be(Form.Imaginary);
        result.Reasoning.Should().Contain("Inconclusive");
    }

    [Fact]
    public void Evaluate_InconclusiveUsesMaxPhase()
    {
        var criteria = new Func<int, AuditableDecision<int>>[]
        {
            x => AuditableDecision<int>.Inconclusive(0.3, "Low confidence"),
            x => AuditableDecision<int>.Inconclusive(0.9, "High confidence"),
        };

        var result = DecisionPipeline.Evaluate(
            1,
            criteria,
            x => x * 2);

        result.State.Should().Be(Form.Imaginary);
        result.ConfidencePhase.Should().Be(0.9);
    }

    [Fact]
    public void Evaluate_CombinesEvidenceFromAllCriteria()
    {
        var evidence1 = new Evidence("crit1", Form.Mark, "Passed");
        var evidence2 = new Evidence("crit2", Form.Mark, "Also passed");

        var criteria = new Func<string, AuditableDecision<string>>[]
        {
            x => AuditableDecision<string>.Approve(x, "OK", evidence1),
            x => AuditableDecision<string>.Approve(x, "OK", evidence2),
        };

        var result = DecisionPipeline.Evaluate("test", criteria, x => x);

        result.Evidence.Should().HaveCount(2);
    }

    [Fact]
    public void Evaluate_FailedAndInconclusive_VoidDominatesOverImaginary()
    {
        // When AND logic has both Void and Imaginary, Imaginary should dominate per And() logic
        var criteria = new Func<int, AuditableDecision<int>>[]
        {
            x => AuditableDecision<int>.Reject("Failed", "Bad"),
            x => AuditableDecision<int>.Inconclusive(0.5, "Uncertain"),
        };

        var result = DecisionPipeline.Evaluate(1, criteria, x => x);

        // Form.And with Imaginary returns Imaginary
        result.State.Should().Be(Form.Imaginary);
    }

    // ──────────── EvaluateAny (OR logic) ────────────

    [Fact]
    public void EvaluateAny_NullCriteria_ThrowsArgumentNullException()
    {
        Action act = () => DecisionPipeline.EvaluateAny<string>(
            "input",
            null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("criteria");
    }

    [Fact]
    public void EvaluateAny_EmptyCriteria_ThrowsArgumentException()
    {
        Action act = () => DecisionPipeline.EvaluateAny<string>(
            "input",
            Array.Empty<Func<string, AuditableDecision<string>>>());

        act.Should().Throw<ArgumentException>()
            .WithParameterName("criteria");
    }

    [Fact]
    public void EvaluateAny_AtLeastOnePasses_ReturnsApproved()
    {
        var criteria = new Func<string, AuditableDecision<string>>[]
        {
            x => AuditableDecision<string>.Reject("Failed", "Bad"),
            x => AuditableDecision<string>.Approve(x, "Passed"),
        };

        var result = DecisionPipeline.EvaluateAny("test", criteria);

        result.State.Should().Be(Form.Mark);
        result.Value.Should().Be("test");
        result.Reasoning.Should().Contain("Passed criteria");
    }

    [Fact]
    public void EvaluateAny_AllFail_ReturnsRejected()
    {
        var criteria = new Func<int, AuditableDecision<int>>[]
        {
            x => AuditableDecision<int>.Reject("Fail1", "Bad1"),
            x => AuditableDecision<int>.Reject("Fail2", "Bad2"),
        };

        var result = DecisionPipeline.EvaluateAny(42, criteria);

        result.State.Should().Be(Form.Void);
        result.Reasoning.Should().Contain("All 2 criteria failed");
    }

    [Fact]
    public void EvaluateAny_NonePassButSomeInconclusive_ReturnsInconclusive()
    {
        var criteria = new Func<int, AuditableDecision<int>>[]
        {
            x => AuditableDecision<int>.Reject("Failed", "Bad"),
            x => AuditableDecision<int>.Inconclusive(0.6, "Maybe"),
        };

        var result = DecisionPipeline.EvaluateAny(1, criteria);

        // OR logic: Void | Imaginary = Imaginary (since Mark dominates, but no Mark here)
        result.State.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void EvaluateAny_CombinesAllEvidence()
    {
        var criteria = new Func<string, AuditableDecision<string>>[]
        {
            x => AuditableDecision<string>.Approve(x, "OK", new Evidence("e1", Form.Mark, "A")),
            x => AuditableDecision<string>.Reject("Fail", "Bad", new Evidence("e2", Form.Void, "B")),
        };

        var result = DecisionPipeline.EvaluateAny("test", criteria);

        result.Evidence.Should().HaveCount(2);
    }

    // ──────────── Chain ────────────

    [Fact]
    public void Chain_AllStepsPass_ReturnsFinalValue()
    {
        var initial = AuditableDecision<int>.Approve(1, "Start");

        var result = DecisionPipeline.Chain(
            initial,
            x => AuditableDecision<int>.Approve(x + 1, "Step 1"),
            x => AuditableDecision<int>.Approve(x + 1, "Step 2"));

        result.State.Should().Be(Form.Mark);
        result.Value.Should().Be(3);
    }

    [Fact]
    public void Chain_FirstStepFails_StopsEarly()
    {
        var initial = AuditableDecision<int>.Reject("Failed", "Bad start");

        bool secondStepCalled = false;
        var result = DecisionPipeline.Chain(
            initial,
            x =>
            {
                secondStepCalled = true;
                return AuditableDecision<int>.Approve(x, "Should not run");
            });

        result.State.Should().Be(Form.Void);
        secondStepCalled.Should().BeFalse();
    }

    [Fact]
    public void Chain_MiddleStepInconclusive_ReturnsInconclusive()
    {
        var initial = AuditableDecision<int>.Approve(1, "Start");

        var result = DecisionPipeline.Chain(
            initial,
            x => AuditableDecision<int>.Inconclusive(0.5, "Uncertain"),
            x => AuditableDecision<int>.Approve(x, "Should not run"));

        result.State.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void Chain_NoSteps_ReturnsInitial()
    {
        var initial = AuditableDecision<int>.Approve(42, "Initial");

        var result = DecisionPipeline.Chain(initial);

        result.State.Should().Be(Form.Mark);
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Chain_CombinesEvidenceAcrossSteps()
    {
        var initial = AuditableDecision<int>.Approve(
            1, "Start", new Evidence("init", Form.Mark, "Initial"));

        var result = DecisionPipeline.Chain(
            initial,
            x => AuditableDecision<int>.Approve(
                x + 1, "Step", new Evidence("step1", Form.Mark, "Step one")));

        result.Evidence.Should().HaveCount(2);
    }

    [Fact]
    public void Chain_CombinesReasoningAcrossSteps()
    {
        var initial = AuditableDecision<int>.Approve(1, "First");

        var result = DecisionPipeline.Chain(
            initial,
            x => AuditableDecision<int>.Approve(x, "Second"));

        result.Reasoning.Should().Contain("First");
        result.Reasoning.Should().Contain("Second");
    }

    [Fact]
    public void Chain_StepRejects_ReturnsCombinedReasoningInRejection()
    {
        var initial = AuditableDecision<int>.Approve(1, "Start");

        var result = DecisionPipeline.Chain(
            initial,
            x => AuditableDecision<int>.Reject("Denied", "Step rejected"));

        result.State.Should().Be(Form.Void);
        result.Reasoning.Should().Contain("Start");
    }
}
