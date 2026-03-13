using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class AuditableDecisionTests
{
    [Fact]
    public void Approve_CreatesMarkCertainty()
    {
        var sut = AuditableDecision<string>.Approve("value", "looks good");

        sut.Certainty.Should().Be(Form.Mark);
        sut.Result.IsSuccess.Should().BeTrue();
        sut.Result.Value.Should().Be("value");
        sut.Reasoning.Should().Be("looks good");
        sut.RequiresHumanReview.Should().BeFalse();
        sut.ComplianceStatus.Should().Be("APPROVED");
    }

    [Fact]
    public void Approve_WithEvidence_IncludesEvidence()
    {
        var evidence = new Evidence("criterion", Form.Mark, "passed");
        var sut = AuditableDecision<string>.Approve("v", "reason", evidence);

        sut.EvidenceTrail.Should().HaveCount(1);
        sut.Evidence.Should().HaveCount(1);
    }

    [Fact]
    public void Reject_CreatesVoidCertainty()
    {
        var sut = AuditableDecision<string>.Reject("failed", "not safe");

        sut.Certainty.Should().Be(Form.Void);
        sut.Result.IsSuccess.Should().BeFalse();
        sut.Reasoning.Should().Be("not safe");
        sut.RequiresHumanReview.Should().BeFalse();
        sut.ComplianceStatus.Should().Be("REJECTED");
    }

    [Fact]
    public void Reject_WithReasoningOnly_CreatesVoidCertainty()
    {
        var evidence = new List<Evidence> { new("c", Form.Void, "d") };
        var sut = AuditableDecision<string>.Reject("rejected reason", evidence);

        sut.Certainty.Should().Be(Form.Void);
        sut.Result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Uncertain_CreatesImaginaryCertainty()
    {
        var sut = AuditableDecision<string>.Uncertain("unclear", "need more info");

        sut.Certainty.Should().Be(Form.Imaginary);
        sut.RequiresHumanReview.Should().BeTrue();
        sut.ComplianceStatus.Should().Be("INCONCLUSIVE");
    }

    [Fact]
    public void Inconclusive_CreatesImaginaryWithConfidencePhase()
    {
        var sut = AuditableDecision<string>.Inconclusive(0.6, "borderline case");

        sut.Certainty.Should().Be(Form.Imaginary);
        sut.ConfidencePhase.Should().Be(0.6);
        sut.RequiresHumanReview.Should().BeTrue();
        sut.ComplianceStatus.Should().Contain("INCONCLUSIVE");
        sut.ComplianceStatus.Should().Contain("60");
    }

    [Fact]
    public void Constructor_InvalidConfidencePhase_ThrowsArgumentOutOfRange()
    {
        var act = () => new AuditableDecision<string>(
            Ouroboros.Abstractions.Monads.Result<string, string>.Success("v"),
            Form.Imaginary,
            "reason",
            Array.Empty<Evidence>(),
            confidencePhase: 1.5);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Value_WhenSuccess_ReturnsValue()
    {
        var sut = AuditableDecision<string>.Approve("hello", "reason");

        sut.Value.Should().Be("hello");
    }

    [Fact]
    public void Value_WhenFailure_ReturnsDefault()
    {
        var sut = AuditableDecision<string>.Reject("error", "reason");

        sut.Value.Should().BeNull();
    }

    [Fact]
    public void State_IsAliasForCertainty()
    {
        var sut = AuditableDecision<string>.Approve("v", "r");

        sut.State.Should().Be(sut.Certainty);
    }

    [Fact]
    public void WithEvidence_AddsEvidenceToTrail()
    {
        var sut = AuditableDecision<string>.Approve("v", "r");
        var newEvidence = new Evidence("new", Form.Mark, "new evidence");

        var updated = sut.WithEvidence(newEvidence);

        updated.EvidenceTrail.Should().HaveCount(1);
        sut.EvidenceTrail.Should().BeEmpty();
    }

    [Fact]
    public void WithMetadata_AddsMetadata()
    {
        var sut = AuditableDecision<string>.Approve("v", "r");

        var updated = sut.WithMetadata("key", "value");

        updated.Metadata.Should().ContainKey("key");
        updated.Metadata["key"].Should().Be("value");
        sut.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void ToAuditEntry_ContainsDecisionInfo()
    {
        var evidence = new Evidence("check", Form.Mark, "passed");
        var sut = AuditableDecision<string>.Approve("v", "all good", evidence)
            .WithMetadata("env", "prod");

        var entry = sut.ToAuditEntry();

        entry.Should().Contain("Decision:");
        entry.Should().Contain("all good");
        entry.Should().Contain("Evidence Trail:");
        entry.Should().Contain("check");
        entry.Should().Contain("Metadata:");
        entry.Should().Contain("env");
    }

    [Fact]
    public void Match_OnApproved_CallsOnCertain()
    {
        var sut = AuditableDecision<string>.Approve("value", "reason");

        var result = sut.Match(
            onCertain: v => $"approved:{v}",
            onRejected: e => $"rejected:{e}",
            onUncertain: e => $"uncertain:{e}");

        result.Should().Be("approved:value");
    }

    [Fact]
    public void Match_OnRejected_CallsOnRejected()
    {
        var sut = AuditableDecision<string>.Reject("denied", "reason");

        var result = sut.Match(
            onCertain: v => "certain",
            onRejected: e => $"rejected:{e}",
            onUncertain: e => "uncertain");

        result.Should().Contain("rejected:");
    }

    [Fact]
    public void Match_OnUncertain_CallsOnUncertain()
    {
        var sut = AuditableDecision<string>.Uncertain("unknown", "reason");

        var result = sut.Match(
            onCertain: v => "certain",
            onRejected: e => "rejected",
            onUncertain: e => $"uncertain:{e}");

        result.Should().Contain("uncertain:");
    }

    [Fact]
    public void Timestamp_DefaultsToUtcNow()
    {
        var before = DateTime.UtcNow;
        var sut = AuditableDecision<string>.Approve("v", "r");
        var after = DateTime.UtcNow;

        sut.Timestamp.Should().BeOnOrAfter(before);
        sut.Timestamp.Should().BeOnOrBefore(after);
    }
}
