using Ouroboros.Agent.MetaAI.Affect;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI.Affect;

/// <summary>
/// Additional tests for Affect records covering record equality, with-expressions,
/// and edge cases not covered by AffectRecordTests.
/// </summary>
[Trait("Category", "Unit")]
public class AffectRecordEqualityTests
{
    [Fact]
    public void AffectConfig_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new AffectConfig();
        var b = new AffectConfig();

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void AffectConfig_RecordEquality_DifferentValues_AreNotEqual()
    {
        // Arrange
        var a = new AffectConfig(StressThreshold: 0.5);
        var b = new AffectConfig(StressThreshold: 0.9);

        // Assert
        a.Should().NotBe(b);
    }

    [Fact]
    public void AffectConfig_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new AffectConfig();

        // Act
        var modified = original with { StressThreshold = 0.3 };

        // Assert
        modified.StressThreshold.Should().Be(0.3);
        modified.ConfidenceDecayRate.Should().Be(original.ConfidenceDecayRate);
        modified.CuriosityBoostFactor.Should().Be(original.CuriosityBoostFactor);
    }

    [Fact]
    public void AffectiveState_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ts = new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        var metadata = new Dictionary<string, object>();

        var a = new AffectiveState(id, 0.5, 0.3, 0.9, 0.4, 0.6, ts, metadata);
        var b = new AffectiveState(id, 0.5, 0.3, 0.9, 0.4, 0.6, ts, metadata);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void AffectiveState_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new AffectiveState(
            Guid.NewGuid(), 0.5, 0.3, 0.8, 0.4, 0.6,
            DateTime.UtcNow, new Dictionary<string, object>());

        // Act
        var modified = original with { Stress = 0.9, Valence = 0.1 };

        // Assert
        modified.Stress.Should().Be(0.9);
        modified.Valence.Should().Be(0.1);
        modified.Confidence.Should().Be(original.Confidence);
        modified.Id.Should().Be(original.Id);
    }

    [Fact]
    public void ValenceSignal_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var ts = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var a = new ValenceSignal("source", 0.5, SignalType.Stress, ts, null);
        var b = new ValenceSignal("source", 0.5, SignalType.Stress, ts, null);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void HomeostasisRule_RecordEquality_SameId_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ts = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new HomeostasisRule(id, "Rule", "Desc", SignalType.Stress, 0.0, 0.7, 0.3,
            HomeostasisAction.Throttle, 1.0, true, ts);
        var b = new HomeostasisRule(id, "Rule", "Desc", SignalType.Stress, 0.0, 0.7, 0.3,
            HomeostasisAction.Throttle, 1.0, true, ts);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void CorrectionResult_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new CorrectionResult(
            Guid.NewGuid(), HomeostasisAction.Alert, false,
            "Failed", 0.85, 0.85, DateTime.UtcNow);

        // Act
        var modified = original with { Success = true, Message = "Corrected", ValueAfter = 0.45 };

        // Assert
        modified.Success.Should().BeTrue();
        modified.Message.Should().Be("Corrected");
        modified.ValueAfter.Should().Be(0.45);
        modified.ViolationId.Should().Be(original.ViolationId);
    }

    [Fact]
    public void StressDetectionResult_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new StressDetectionResult(
            0.5, 1.0, 0.3, false, new List<double> { 1.0 },
            "Normal", DateTime.UtcNow);

        // Act
        var modified = original with { IsAnomalous = true, StressLevel = 0.95 };

        // Assert
        modified.IsAnomalous.Should().BeTrue();
        modified.StressLevel.Should().Be(0.95);
        modified.Frequency.Should().Be(original.Frequency);
    }

    [Fact]
    public void PolicyHealthSummary_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var violations = new Dictionary<SignalType, int> { [SignalType.Stress] = 10 };

        var a = new PolicyHealthSummary(5, 4, 10, 2, 8, 7, 0.875, violations);
        var b = new PolicyHealthSummary(5, 4, 10, 2, 8, 7, 0.875, violations);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void PolicyViolation_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new PolicyViolation(
            Guid.NewGuid(), "Rule1", SignalType.Confidence,
            0.2, 0.5, 1.0, "below_lower_bound",
            HomeostasisAction.Boost, 0.3, DateTime.UtcNow);

        // Act
        var modified = original with { Severity = 0.9 };

        // Assert
        modified.Severity.Should().Be(0.9);
        modified.RuleName.Should().Be("Rule1");
    }

    [Fact]
    public void SignalType_HasExpectedEnumValues()
    {
        // Act
        var values = Enum.GetValues<SignalType>();

        // Assert
        values.Should().HaveCountGreaterThanOrEqualTo(5);
    }

    [Fact]
    public void HomeostasisAction_EnumValues_HaveExpectedIntValues()
    {
        // Assert
        ((int)HomeostasisAction.Log).Should().Be(0);
        ((int)HomeostasisAction.Alert).Should().Be(1);
        ((int)HomeostasisAction.Throttle).Should().Be(2);
        ((int)HomeostasisAction.Boost).Should().Be(3);
        ((int)HomeostasisAction.Pause).Should().Be(4);
        ((int)HomeostasisAction.Reset).Should().Be(5);
        ((int)HomeostasisAction.Custom).Should().Be(6);
    }
}
