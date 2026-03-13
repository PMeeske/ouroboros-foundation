using Ouroboros.Abstractions;
using Ouroboros.Core.LawsOfForm;
using LoF = Ouroboros.Core.LawsOfForm.Form;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class AuditableDecisionTests
{
    private static Evidence MakeEvidence(string name = "test", LoF? eval = null, string desc = "desc")
        => new(name, eval ?? LoF.Mark, desc);

    // --- Approve ---

    [Fact]
    public void Approve_CreatesMarkDecision()
    {
        // Act
        var decision = AuditableDecision<string>.Approve("ok", "Passed checks", MakeEvidence());

        // Assert
        decision.Certainty.Should().Be(LoF.Mark);
        decision.Result.IsSuccess.Should().BeTrue();
        decision.Result.Value.Should().Be("ok");
        decision.Reasoning.Should().Be("Passed checks");
        decision.RequiresHumanReview.Should().BeFalse();
        decision.ComplianceStatus.Should().Be("APPROVED");
    }

    [Fact]
    public void Approve_WithList_CreatesMarkDecision()
    {
        // Arrange
        var evidence = new List<Evidence> { MakeEvidence("a"), MakeEvidence("b") };

        // Act
        var decision = AuditableDecision<int>.Approve(42, "Good", evidence);

        // Assert
        decision.Value.Should().Be(42);
        decision.EvidenceTrail.Should().HaveCount(2);
    }

    // --- Reject ---

    [Fact]
    public void Reject_CreatesVoidDecision()
    {
        // Act
        var decision = AuditableDecision<string>.Reject("denied", "Failed checks", MakeEvidence("rule1", LoF.Void));

        // Assert
        decision.Certainty.Should().Be(LoF.Void);
        decision.Result.IsFailure.Should().BeTrue();
        decision.Result.Error.Should().Be("denied");
        decision.RequiresHumanReview.Should().BeFalse();
        decision.ComplianceStatus.Should().Be("REJECTED");
    }

    [Fact]
    public void Reject_WithReasoningAndEvidence_CreatesVoidDecision()
    {
        // Arrange
        var evidence = new List<Evidence> { MakeEvidence() };

        // Act
        var decision = AuditableDecision<string>.Reject("reasoning", evidence);

        // Assert
        decision.Certainty.Should().Be(LoF.Void);
        decision.Result.IsFailure.Should().BeTrue();
    }

    // --- Uncertain ---

    [Fact]
    public void Uncertain_CreatesImaginaryDecision()
    {
        // Act
        var decision = AuditableDecision<string>.Uncertain("unclear", "Need more info", MakeEvidence("ambig", LoF.Imaginary));

        // Assert
        decision.Certainty.Should().Be(LoF.Imaginary);
        decision.Result.IsFailure.Should().BeTrue();
        decision.RequiresHumanReview.Should().BeTrue();
        decision.ComplianceStatus.Should().Be("INCONCLUSIVE");
    }

    // --- Inconclusive ---

    [Fact]
    public void Inconclusive_CreatesImaginaryWithConfidence()
    {
        // Act
        var decision = AuditableDecision<string>.Inconclusive(0.75, "Partially confident", MakeEvidence());

        // Assert
        decision.Certainty.Should().Be(LoF.Imaginary);
        decision.ConfidencePhase.Should().Be(0.75);
        decision.RequiresHumanReview.Should().BeTrue();
        decision.ComplianceStatus.Should().Contain("75");
    }

    [Fact]
    public void Inconclusive_WithList_CreatesImaginaryWithConfidence()
    {
        // Arrange
        var evidence = new List<Evidence> { MakeEvidence() };

        // Act
        var decision = AuditableDecision<int>.Inconclusive(0.5, "50-50", evidence);

        // Assert
        decision.ConfidencePhase.Should().Be(0.5);
    }

    // --- ConfidencePhase validation ---

    [Fact]
    public void Constructor_InvalidConfidencePhase_ThrowsArgumentOutOfRange()
    {
        // Act
        var act = () => new AuditableDecision<string>(
            Result<string, string>.Success("ok"),
            LoF.Imaginary,
            "test",
            new List<Evidence>(),
            confidencePhase: 1.5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_NegativeConfidencePhase_ThrowsArgumentOutOfRange()
    {
        // Act
        var act = () => new AuditableDecision<string>(
            Result<string, string>.Success("ok"),
            LoF.Mark,
            "test",
            new List<Evidence>(),
            confidencePhase: -0.1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // --- Value property ---

    [Fact]
    public void Value_OnSuccess_ReturnsValue()
    {
        var decision = AuditableDecision<int>.Approve(42, "ok", MakeEvidence());
        decision.Value.Should().Be(42);
    }

    [Fact]
    public void Value_OnFailure_ReturnsDefault()
    {
        var decision = AuditableDecision<int>.Reject("error", "bad", MakeEvidence());
        decision.Value.Should().Be(default(int));
    }

    // --- State alias ---

    [Fact]
    public void State_IsAliasForCertainty()
    {
        var decision = AuditableDecision<string>.Approve("ok", "reason", MakeEvidence());
        decision.State.Should().Be(decision.Certainty);
    }

    // --- Evidence alias ---

    [Fact]
    public void Evidence_IsAliasForEvidenceTrail()
    {
        var decision = AuditableDecision<string>.Approve("ok", "reason", MakeEvidence("e1"));
        decision.Evidence.Should().BeSameAs(decision.EvidenceTrail);
    }

    // --- WithEvidence ---

    [Fact]
    public void WithEvidence_AddsToTrail()
    {
        // Arrange
        var decision = AuditableDecision<string>.Approve("ok", "reason", MakeEvidence("first"));

        // Act
        var updated = decision.WithEvidence(MakeEvidence("second"));

        // Assert
        updated.EvidenceTrail.Should().HaveCount(2);
        decision.EvidenceTrail.Should().HaveCount(1, "original should be unchanged");
    }

    // --- WithMetadata ---

    [Fact]
    public void WithMetadata_AddsKeyValuePair()
    {
        // Arrange
        var decision = AuditableDecision<string>.Approve("ok", "reason", MakeEvidence());

        // Act
        var updated = decision.WithMetadata("key", "value");

        // Assert
        updated.Metadata.Should().ContainKey("key");
        updated.Metadata["key"].Should().Be("value");
    }

    // --- ToAuditEntry ---

    [Fact]
    public void ToAuditEntry_ContainsRelevantInfo()
    {
        // Arrange
        var decision = AuditableDecision<string>.Approve("ok", "Passed all checks", MakeEvidence("safety"))
            .WithMetadata("user", "admin");

        // Act
        string entry = decision.ToAuditEntry();

        // Assert
        entry.Should().Contain("Passed all checks");
        entry.Should().Contain("safety");
        entry.Should().Contain("Success");
        entry.Should().Contain("user");
        entry.Should().Contain("admin");
    }

    // --- Match ---

    [Fact]
    public void Match_Approved_CallsOnCertain()
    {
        // Arrange
        var decision = AuditableDecision<string>.Approve("value", "ok", MakeEvidence());

        // Act
        var result = decision.Match(
            onCertain: v => $"certain:{v}",
            onRejected: e => $"rejected:{e}",
            onUncertain: e => $"uncertain:{e}");

        // Assert
        result.Should().Be("certain:value");
    }

    [Fact]
    public void Match_Rejected_CallsOnRejected()
    {
        // Arrange
        var decision = AuditableDecision<string>.Reject("denied", "bad", MakeEvidence());

        // Act
        var result = decision.Match(
            onCertain: v => $"certain:{v}",
            onRejected: e => $"rejected:{e}",
            onUncertain: e => $"uncertain:{e}");

        // Assert
        result.Should().Be("rejected:denied");
    }

    [Fact]
    public void Match_Uncertain_CallsOnUncertain()
    {
        // Arrange
        var decision = AuditableDecision<string>.Uncertain("unclear", "need more data", MakeEvidence());

        // Act
        var result = decision.Match(
            onCertain: v => $"certain:{v}",
            onRejected: e => $"rejected:{e}",
            onUncertain: e => $"uncertain:{e}");

        // Assert
        result.Should().Be("uncertain:unclear");
    }

    // --- Timestamp ---

    [Fact]
    public void Timestamp_DefaultsToUtcNow()
    {
        var before = DateTime.UtcNow;
        var decision = AuditableDecision<string>.Approve("ok", "reason", MakeEvidence());
        var after = DateTime.UtcNow;

        decision.Timestamp.Should().BeOnOrAfter(before);
        decision.Timestamp.Should().BeOnOrBefore(after);
    }

    // --- Metadata defaults to empty ---

    [Fact]
    public void Metadata_DefaultsToEmpty()
    {
        var decision = AuditableDecision<string>.Approve("ok", "reason", MakeEvidence());
        decision.Metadata.Should().BeEmpty();
    }
}
