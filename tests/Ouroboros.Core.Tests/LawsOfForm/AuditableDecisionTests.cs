// <copyright file="AuditableDecisionTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for the <see cref="AuditableDecision{T}"/> record.
/// </summary>
[Trait("Category", "Unit")]
public class AuditableDecisionTests
{
    private static Evidence CreateEvidence(string name = "test", string description = "desc") =>
        new(name, Form.Mark, description);

    // --- Constructor ---

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var result = Result<int, string>.Success(42);
        var certainty = Form.Mark;
        var reasoning = "good reason";
        var evidence = new List<Evidence> { CreateEvidence() };
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var metadata = new Dictionary<string, string> { ["key"] = "value" };

        var decision = new AuditableDecision<int>(
            result, certainty, reasoning, evidence, timestamp, metadata, confidencePhase: 0.75);

        decision.Result.Should().Be(result);
        decision.Certainty.Should().Be(certainty);
        decision.Reasoning.Should().Be(reasoning);
        decision.EvidenceTrail.Should().BeEquivalentTo(evidence);
        decision.Timestamp.Should().Be(timestamp);
        decision.Metadata.Should().ContainKey("key").WhoseValue.Should().Be("value");
        decision.ConfidencePhase.Should().Be(0.75);
    }

    [Fact]
    public void Constructor_DefaultTimestamp_UsesUtcNow()
    {
        var before = DateTime.UtcNow;
        var decision = new AuditableDecision<int>(
            Result<int, string>.Success(1), Form.Mark, "r", Array.Empty<Evidence>());
        var after = DateTime.UtcNow;

        decision.Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Constructor_DefaultMetadata_IsEmptyDictionary()
    {
        var decision = new AuditableDecision<int>(
            Result<int, string>.Success(1), Form.Mark, "r", Array.Empty<Evidence>());

        decision.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ConfidencePhaseBelow0_Throws()
    {
        var act = () => new AuditableDecision<int>(
            Result<int, string>.Success(1), Form.Mark, "r", Array.Empty<Evidence>(),
            confidencePhase: -0.1);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("confidencePhase");
    }

    [Fact]
    public void Constructor_ConfidencePhaseAbove1_Throws()
    {
        var act = () => new AuditableDecision<int>(
            Result<int, string>.Success(1), Form.Mark, "r", Array.Empty<Evidence>(),
            confidencePhase: 1.1);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("confidencePhase");
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void Constructor_ConfidencePhaseBoundaryValues_Accepted(double phase)
    {
        var decision = new AuditableDecision<int>(
            Result<int, string>.Success(1), Form.Mark, "r", Array.Empty<Evidence>(),
            confidencePhase: phase);

        decision.ConfidencePhase.Should().Be(phase);
    }

    // --- Alias Properties ---

    [Fact]
    public void State_ReturnsAliasForCertainty()
    {
        var decision = AuditableDecision<int>.Approve(42, "ok");

        decision.State.Should().Be(decision.Certainty);
    }

    [Fact]
    public void Evidence_ReturnsAliasForEvidenceTrail()
    {
        var ev = CreateEvidence();
        var decision = AuditableDecision<int>.Approve(42, "ok", ev);

        decision.Evidence.Should().BeSameAs(decision.EvidenceTrail);
    }

    // --- Value Property ---

    [Fact]
    public void Value_SuccessResult_ReturnsValue()
    {
        var decision = AuditableDecision<int>.Approve(42, "ok");

        decision.Value.Should().Be(42);
    }

    [Fact]
    public void Value_FailureResult_ReturnsDefault()
    {
        var decision = AuditableDecision<int>.Reject("err", "bad");

        decision.Value.Should().Be(default(int));
    }

    // --- RequiresHumanReview ---

    [Fact]
    public void RequiresHumanReview_ImaginaryCertainty_ReturnsTrue()
    {
        var decision = AuditableDecision<int>.Uncertain("err", "reason");

        decision.RequiresHumanReview.Should().BeTrue();
    }

    [Fact]
    public void RequiresHumanReview_MarkCertainty_ReturnsFalse()
    {
        var decision = AuditableDecision<int>.Approve(1, "ok");

        decision.RequiresHumanReview.Should().BeFalse();
    }

    [Fact]
    public void RequiresHumanReview_VoidCertainty_ReturnsFalse()
    {
        var decision = AuditableDecision<int>.Reject("err", "bad");

        decision.RequiresHumanReview.Should().BeFalse();
    }

    // --- ComplianceStatus ---

    [Fact]
    public void ComplianceStatus_Mark_ReturnsApproved()
    {
        var decision = AuditableDecision<int>.Approve(1, "ok");

        decision.ComplianceStatus.Should().Be("APPROVED");
    }

    [Fact]
    public void ComplianceStatus_Void_ReturnsRejected()
    {
        var decision = AuditableDecision<int>.Reject("err", "bad");

        decision.ComplianceStatus.Should().Be("REJECTED");
    }

    [Fact]
    public void ComplianceStatus_ImaginaryWithoutConfidence_ReturnsInconclusive()
    {
        var decision = AuditableDecision<int>.Uncertain("err", "reason");

        decision.ComplianceStatus.Should().Be("INCONCLUSIVE");
    }

    [Fact]
    public void ComplianceStatus_ImaginaryWithConfidence_ReturnsInconclusiveWithPercentage()
    {
        var decision = AuditableDecision<int>.Inconclusive(0.75, "reason");

        decision.ComplianceStatus.Should().Be("INCONCLUSIVE (75%)");
    }

    // --- Factory Methods ---

    [Fact]
    public void Approve_Params_CreatesMarkDecisionWithSuccessResult()
    {
        var ev = CreateEvidence();
        var decision = AuditableDecision<string>.Approve("ok", "reasoning", ev);

        decision.Certainty.Should().Be(Form.Mark);
        decision.Result.IsSuccess.Should().BeTrue();
        decision.Result.Value.Should().Be("ok");
        decision.Reasoning.Should().Be("reasoning");
        decision.EvidenceTrail.Should().HaveCount(1);
    }

    [Fact]
    public void Approve_List_CreatesMarkDecisionWithSuccessResult()
    {
        IReadOnlyList<Evidence> evidence = new List<Evidence> { CreateEvidence() };
        var decision = AuditableDecision<string>.Approve("ok", "reasoning", evidence);

        decision.Certainty.Should().Be(Form.Mark);
        decision.Result.IsSuccess.Should().BeTrue();
        decision.EvidenceTrail.Should().HaveCount(1);
    }

    [Fact]
    public void Reject_ErrorAndReasoning_CreatesVoidDecisionWithFailureResult()
    {
        var ev = CreateEvidence();
        var decision = AuditableDecision<int>.Reject("error msg", "reasoning", ev);

        decision.Certainty.Should().Be(Form.Void);
        decision.Result.IsSuccess.Should().BeFalse();
        decision.Result.Error.Should().Be("error msg");
        decision.Reasoning.Should().Be("reasoning");
    }

    [Fact]
    public void Reject_WithList_CreatesVoidDecision()
    {
        IReadOnlyList<Evidence> evidence = new List<Evidence> { CreateEvidence() };
        var decision = AuditableDecision<int>.Reject("error", "reasoning", evidence);

        decision.Certainty.Should().Be(Form.Void);
        decision.Result.Error.Should().Be("error");
    }

    [Fact]
    public void Reject_ReasoningOnly_UsesReasoningAsError()
    {
        IReadOnlyList<Evidence> evidence = new List<Evidence> { CreateEvidence() };
        var decision = AuditableDecision<int>.Reject("the reason", evidence);

        decision.Certainty.Should().Be(Form.Void);
        decision.Result.Error.Should().Be("the reason");
        decision.Reasoning.Should().Be("the reason");
    }

    [Fact]
    public void Uncertain_CreatesImaginaryDecision()
    {
        var ev = CreateEvidence();
        var decision = AuditableDecision<int>.Uncertain("uncertainty", "reasoning", ev);

        decision.Certainty.Should().Be(Form.Imaginary);
        decision.Result.IsSuccess.Should().BeFalse();
        decision.Result.Error.Should().Be("uncertainty");
    }

    [Fact]
    public void Inconclusive_Params_CreatesImaginaryDecisionWithConfidence()
    {
        var ev = CreateEvidence();
        var decision = AuditableDecision<int>.Inconclusive(0.5, "reasoning", ev);

        decision.Certainty.Should().Be(Form.Imaginary);
        decision.ConfidencePhase.Should().Be(0.5);
        decision.Result.Error.Should().Be("Inconclusive decision");
    }

    [Fact]
    public void Inconclusive_List_CreatesImaginaryDecisionWithConfidence()
    {
        IReadOnlyList<Evidence> evidence = new List<Evidence> { CreateEvidence() };
        var decision = AuditableDecision<int>.Inconclusive(0.8, "reasoning", evidence);

        decision.Certainty.Should().Be(Form.Imaginary);
        decision.ConfidencePhase.Should().Be(0.8);
    }

    // --- WithEvidence ---

    [Fact]
    public void WithEvidence_AddsToTrail()
    {
        var original = AuditableDecision<int>.Approve(1, "ok", CreateEvidence("first"));
        var newEvidence = CreateEvidence("second");

        var updated = original.WithEvidence(newEvidence);

        updated.EvidenceTrail.Should().HaveCount(2);
        updated.EvidenceTrail[1].CriterionName.Should().Be("second");
    }

    [Fact]
    public void WithEvidence_DoesNotMutateOriginal()
    {
        var original = AuditableDecision<int>.Approve(1, "ok", CreateEvidence());

        original.WithEvidence(CreateEvidence("extra"));

        original.EvidenceTrail.Should().HaveCount(1);
    }

    // --- WithMetadata ---

    [Fact]
    public void WithMetadata_AddsKeyValue()
    {
        var original = AuditableDecision<int>.Approve(1, "ok");

        var updated = original.WithMetadata("env", "test");

        updated.Metadata.Should().ContainKey("env").WhoseValue.Should().Be("test");
    }

    [Fact]
    public void WithMetadata_DoesNotMutateOriginal()
    {
        var original = AuditableDecision<int>.Approve(1, "ok");

        original.WithMetadata("env", "test");

        original.Metadata.Should().BeEmpty();
    }

    // --- ToAuditEntry ---

    [Fact]
    public void ToAuditEntry_ContainsDecisionInfo()
    {
        var ev = new Evidence("safety", Form.Mark, "all clear");
        var decision = AuditableDecision<int>.Approve(1, "safe action", ev);

        var entry = decision.ToAuditEntry();

        entry.Should().Contain("Decision:");
        entry.Should().Contain("Result: Success");
        entry.Should().Contain("Reasoning: safe action");
        entry.Should().Contain("safety");
        entry.Should().Contain("all clear");
    }

    [Fact]
    public void ToAuditEntry_FailureResult_ContainsFailure()
    {
        var decision = AuditableDecision<int>.Reject("err", "bad action");

        var entry = decision.ToAuditEntry();

        entry.Should().Contain("Result: Failure");
    }

    [Fact]
    public void ToAuditEntry_WithMetadata_ContainsMetadata()
    {
        var decision = AuditableDecision<int>.Approve(1, "ok")
            .WithMetadata("env", "prod");

        var entry = decision.ToAuditEntry();

        entry.Should().Contain("Metadata:");
        entry.Should().Contain("env: prod");
    }

    // --- Match ---

    [Fact]
    public void Match_MarkCertainty_InvokesCertainHandler()
    {
        var decision = AuditableDecision<int>.Approve(42, "ok");

        var result = decision.Match(
            onCertain: v => $"certain:{v}",
            onRejected: e => $"rejected:{e}",
            onUncertain: e => $"uncertain:{e}");

        result.Should().Be("certain:42");
    }

    [Fact]
    public void Match_VoidCertainty_InvokesRejectedHandler()
    {
        var decision = AuditableDecision<int>.Reject("err", "bad");

        var result = decision.Match(
            onCertain: v => $"certain:{v}",
            onRejected: e => $"rejected:{e}",
            onUncertain: e => $"uncertain:{e}");

        result.Should().Be("rejected:err");
    }

    [Fact]
    public void Match_ImaginaryCertainty_InvokesUncertainHandler()
    {
        var decision = AuditableDecision<int>.Uncertain("maybe", "unsure");

        var result = decision.Match(
            onCertain: v => $"certain:{v}",
            onRejected: e => $"rejected:{e}",
            onUncertain: e => $"uncertain:{e}");

        result.Should().Be("uncertain:maybe");
    }

    // --- Record Equality ---

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var evidence = new List<Evidence> { CreateEvidence() };
        var timestamp = DateTime.UtcNow;
        var result = Result<int, string>.Success(1);

        var d1 = new AuditableDecision<int>(result, Form.Mark, "r", evidence, timestamp);
        var d2 = new AuditableDecision<int>(result, Form.Mark, "r", evidence, timestamp);

        d1.Should().Be(d2);
    }
}
