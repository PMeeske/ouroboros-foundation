using Ouroboros.Agent.MetaAI.Affect;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI.Affect;

[Trait("Category", "Unit")]
public class AffectRecordTests
{
    [Fact]
    public void AffectConfig_DefaultValues_AreCorrect()
    {
        // Act
        var config = new AffectConfig();

        // Assert
        config.StressThreshold.Should().Be(0.7);
        config.ConfidenceDecayRate.Should().Be(0.01);
        config.CuriosityBoostFactor.Should().Be(0.2);
        config.SignalHistorySize.Should().Be(1000);
        config.FourierWindowSize.Should().Be(64);
    }

    [Fact]
    public void AffectConfig_CustomValues_SetCorrectly()
    {
        // Act
        var config = new AffectConfig(
            StressThreshold: 0.5,
            ConfidenceDecayRate: 0.05,
            CuriosityBoostFactor: 0.3,
            SignalHistorySize: 500,
            FourierWindowSize: 128);

        // Assert
        config.StressThreshold.Should().Be(0.5);
        config.ConfidenceDecayRate.Should().Be(0.05);
        config.CuriosityBoostFactor.Should().Be(0.3);
        config.SignalHistorySize.Should().Be(500);
        config.FourierWindowSize.Should().Be(128);
    }

    [Fact]
    public void AffectiveState_AllPropertiesSet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var metadata = new Dictionary<string, object> { ["mood"] = "focused" };

        // Act
        var state = new AffectiveState(
            id, 0.8, 0.3, 0.9, 0.5, 0.6, timestamp, metadata);

        // Assert
        state.Id.Should().Be(id);
        state.Valence.Should().Be(0.8);
        state.Stress.Should().Be(0.3);
        state.Confidence.Should().Be(0.9);
        state.Curiosity.Should().Be(0.5);
        state.Arousal.Should().Be(0.6);
        state.Timestamp.Should().Be(timestamp);
        state.Metadata.Should().ContainKey("mood");
    }

    [Fact]
    public void ValenceSignal_AllPropertiesSet()
    {
        // Act
        var signal = new ValenceSignal(
            "sensor-1", 0.75, SignalType.Confidence,
            DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Assert
        signal.Source.Should().Be("sensor-1");
        signal.Value.Should().Be(0.75);
        signal.Type.Should().Be(SignalType.Confidence);
        signal.Duration.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ValenceSignal_NullDuration_IsAllowed()
    {
        // Act
        var signal = new ValenceSignal(
            "source", 0.5, SignalType.Stress, DateTime.UtcNow, null);

        // Assert
        signal.Duration.Should().BeNull();
    }

    [Fact]
    public void SignalType_AllValuesAreDefined()
    {
        // Act
        var values = Enum.GetValues<SignalType>();

        // Assert
        values.Should().Contain(SignalType.Stress);
        values.Should().Contain(SignalType.Confidence);
        values.Should().Contain(SignalType.Curiosity);
        values.Should().Contain(SignalType.Valence);
        values.Should().Contain(SignalType.Arousal);
    }

    [Fact]
    public void HomeostasisRule_AllPropertiesSet()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var rule = new HomeostasisRule(
            id, "StressLimit", "Keep stress below threshold",
            SignalType.Stress, 0.0, 0.7, 0.3,
            HomeostasisAction.Throttle, 1.0, true, DateTime.UtcNow);

        // Assert
        rule.Id.Should().Be(id);
        rule.Name.Should().Be("StressLimit");
        rule.TargetSignal.Should().Be(SignalType.Stress);
        rule.LowerBound.Should().Be(0.0);
        rule.UpperBound.Should().Be(0.7);
        rule.TargetValue.Should().Be(0.3);
        rule.Action.Should().Be(HomeostasisAction.Throttle);
        rule.Priority.Should().Be(1.0);
        rule.IsActive.Should().BeTrue();
    }

    [Fact]
    public void HomeostasisAction_AllValuesAreDefined()
    {
        // Act
        var values = Enum.GetValues<HomeostasisAction>();

        // Assert
        values.Should().Contain(HomeostasisAction.Log);
        values.Should().Contain(HomeostasisAction.Alert);
        values.Should().Contain(HomeostasisAction.Throttle);
        values.Should().Contain(HomeostasisAction.Boost);
        values.Should().Contain(HomeostasisAction.Pause);
        values.Should().Contain(HomeostasisAction.Reset);
        values.Should().Contain(HomeostasisAction.Custom);
    }

    [Fact]
    public void CorrectionResult_AllPropertiesSet()
    {
        // Arrange
        var violationId = Guid.NewGuid();

        // Act
        var result = new CorrectionResult(
            violationId, HomeostasisAction.Throttle, true,
            "Throttled successfully", 0.85, 0.45, DateTime.UtcNow);

        // Assert
        result.ViolationId.Should().Be(violationId);
        result.ActionTaken.Should().Be(HomeostasisAction.Throttle);
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Throttled successfully");
        result.ValueBefore.Should().Be(0.85);
        result.ValueAfter.Should().Be(0.45);
    }

    [Fact]
    public void StressDetectionResult_AllPropertiesSet()
    {
        // Act
        var result = new StressDetectionResult(
            0.9, 2.5, 0.8, true,
            new List<double> { 1.0, 2.5, 5.0 },
            "High stress detected", DateTime.UtcNow);

        // Assert
        result.StressLevel.Should().Be(0.9);
        result.Frequency.Should().Be(2.5);
        result.Amplitude.Should().Be(0.8);
        result.IsAnomalous.Should().BeTrue();
        result.SpectralPeaks.Should().HaveCount(3);
        result.Analysis.Should().Be("High stress detected");
    }

    [Fact]
    public void PolicyHealthSummary_AllPropertiesSet()
    {
        // Act
        var summary = new PolicyHealthSummary(
            TotalRules: 10,
            ActiveRules: 8,
            TotalViolations: 50,
            RecentViolations: 5,
            TotalCorrections: 45,
            SuccessfulCorrections: 40,
            CorrectionSuccessRate: 0.89,
            ViolationsBySignal: new Dictionary<SignalType, int>
            {
                [SignalType.Stress] = 30,
                [SignalType.Confidence] = 20
            });

        // Assert
        summary.TotalRules.Should().Be(10);
        summary.ActiveRules.Should().Be(8);
        summary.TotalViolations.Should().Be(50);
        summary.CorrectionSuccessRate.Should().Be(0.89);
        summary.ViolationsBySignal.Should().ContainKey(SignalType.Stress);
    }

    [Fact]
    public void PolicyViolation_AllPropertiesSet()
    {
        // Arrange
        var ruleId = Guid.NewGuid();

        // Act
        var violation = new PolicyViolation(
            ruleId, "StressRule", SignalType.Stress,
            0.85, 0.0, 0.7, "upper_bound_exceeded",
            HomeostasisAction.Throttle, 0.95, DateTime.UtcNow);

        // Assert
        violation.RuleId.Should().Be(ruleId);
        violation.RuleName.Should().Be("StressRule");
        violation.Signal.Should().Be(SignalType.Stress);
        violation.ObservedValue.Should().Be(0.85);
        violation.ViolationType.Should().Be("upper_bound_exceeded");
        violation.Severity.Should().Be(0.95);
    }
}
