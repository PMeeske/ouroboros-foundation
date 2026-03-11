using Ouroboros.Agent.MetaAI.Affect;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI.Affect;

[Trait("Category", "Unit")]
public class AffectAdditionalTests
{
    [Fact]
    public void HomeostasisRule_InactiveRule_IsActiveFalse()
    {
        // Act
        var rule = new HomeostasisRule(
            Guid.NewGuid(), "InactiveRule", "Disabled rule",
            SignalType.Stress, 0.0, 1.0, 0.5,
            HomeostasisAction.Log, 0.5, false, DateTime.UtcNow);

        // Assert
        rule.IsActive.Should().BeFalse();
    }

    [Fact]
    public void HomeostasisRule_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new HomeostasisRule(
            Guid.NewGuid(), "Rule", "Desc",
            SignalType.Stress, 0.0, 0.7, 0.3,
            HomeostasisAction.Throttle, 1.0, true, DateTime.UtcNow);

        // Act
        var modified = original with { IsActive = false, Priority = 0.1 };

        // Assert
        modified.IsActive.Should().BeFalse();
        modified.Priority.Should().Be(0.1);
        modified.Name.Should().Be("Rule");
    }

    [Fact]
    public void CorrectionResult_FailedCorrection_SuccessIsFalse()
    {
        // Act
        var result = new CorrectionResult(
            Guid.NewGuid(), HomeostasisAction.Reset, false,
            "Could not reset", 0.9, 0.9, DateTime.UtcNow);

        // Assert
        result.Success.Should().BeFalse();
        result.ValueBefore.Should().Be(result.ValueAfter);
    }

    [Fact]
    public void StressDetectionResult_NormalStress_IsNotAnomalous()
    {
        // Act
        var result = new StressDetectionResult(
            0.2, 1.0, 0.1, false,
            new List<double>(),
            "Normal operation", DateTime.UtcNow);

        // Assert
        result.IsAnomalous.Should().BeFalse();
        result.SpectralPeaks.Should().BeEmpty();
    }

    [Fact]
    public void PolicyHealthSummary_ZeroRules_EmptyViolations()
    {
        // Act
        var summary = new PolicyHealthSummary(
            0, 0, 0, 0, 0, 0, 0.0,
            new Dictionary<SignalType, int>());

        // Assert
        summary.TotalRules.Should().Be(0);
        summary.ViolationsBySignal.Should().BeEmpty();
    }

    [Fact]
    public void PolicyViolation_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ts = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new PolicyViolation(id, "rule", SignalType.Stress, 0.8, 0.0, 0.7, "upper", HomeostasisAction.Throttle, 0.9, ts);
        var b = new PolicyViolation(id, "rule", SignalType.Stress, 0.8, 0.0, 0.7, "upper", HomeostasisAction.Throttle, 0.9, ts);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void ValenceSignal_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new ValenceSignal(
            "source", 0.5, SignalType.Valence,
            DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Act
        var modified = original with { Value = 0.9, Type = SignalType.Arousal };

        // Assert
        modified.Value.Should().Be(0.9);
        modified.Type.Should().Be(SignalType.Arousal);
        modified.Source.Should().Be("source");
    }

    [Fact]
    public void StressDetectionResult_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var peaks = new List<double> { 1.0 };
        var ts = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new StressDetectionResult(0.5, 1.0, 0.3, false, peaks, "normal", ts);
        var b = new StressDetectionResult(0.5, 1.0, 0.3, false, peaks, "normal", ts);

        // Assert
        a.Should().Be(b);
    }
}
