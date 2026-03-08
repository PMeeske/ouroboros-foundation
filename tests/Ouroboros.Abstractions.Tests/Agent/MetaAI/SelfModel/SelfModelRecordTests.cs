using Ouroboros.Agent.MetaAI.SelfModel;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI.SelfModel;

[Trait("Category", "Unit")]
public class SelfModelRecordTests
{
    [Fact]
    public void AnomalyDetection_AllPropertiesSet()
    {
        // Arrange
        var causes = new List<string> { "memory leak", "high load" };
        var detected = DateTime.UtcNow;

        // Act
        var anomaly = new AnomalyDetection(
            "cpu_usage", 95.0, 40.0, 55.0, true, "critical", detected, causes);

        // Assert
        anomaly.MetricName.Should().Be("cpu_usage");
        anomaly.ObservedValue.Should().Be(95.0);
        anomaly.ExpectedValue.Should().Be(40.0);
        anomaly.Deviation.Should().Be(55.0);
        anomaly.IsAnomaly.Should().BeTrue();
        anomaly.Severity.Should().Be("critical");
        anomaly.DetectedAt.Should().Be(detected);
        anomaly.PossibleCauses.Should().HaveCount(2);
    }

    [Fact]
    public void AnomalyDetection_NotAnomaly_IsAnomalyFalse()
    {
        // Act
        var anomaly = new AnomalyDetection(
            "latency", 50.0, 48.0, 2.0, false, "low",
            DateTime.UtcNow, new List<string>());

        // Assert
        anomaly.IsAnomaly.Should().BeFalse();
        anomaly.PossibleCauses.Should().BeEmpty();
    }

    [Fact]
    public void Forecast_AllPropertiesSet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var predictionTime = DateTime.UtcNow;
        var targetTime = DateTime.UtcNow.AddHours(1);
        var metadata = new Dictionary<string, object> { ["model"] = "linear" };

        // Act
        var forecast = new Forecast(
            id, "Predict CPU load", "cpu_load", 75.0, 0.85,
            predictionTime, targetTime, ForecastStatus.Pending, null, metadata);

        // Assert
        forecast.Id.Should().Be(id);
        forecast.Description.Should().Be("Predict CPU load");
        forecast.MetricName.Should().Be("cpu_load");
        forecast.PredictedValue.Should().Be(75.0);
        forecast.Confidence.Should().Be(0.85);
        forecast.PredictionTime.Should().Be(predictionTime);
        forecast.TargetTime.Should().Be(targetTime);
        forecast.Status.Should().Be(ForecastStatus.Pending);
        forecast.ActualValue.Should().BeNull();
        forecast.Metadata.Should().ContainKey("model");
    }

    [Fact]
    public void Forecast_VerifiedWithActualValue_StatusIsVerified()
    {
        // Arrange
        var now = DateTime.UtcNow;

        // Act
        var forecast = new Forecast(
            Guid.NewGuid(), "desc", "metric", 100.0, 0.9,
            now, now, ForecastStatus.Verified, 98.5,
            new Dictionary<string, object>());

        // Assert
        forecast.Status.Should().Be(ForecastStatus.Verified);
        forecast.ActualValue.Should().Be(98.5);
    }

    [Fact]
    public void ForecastCalibration_AllPropertiesSet()
    {
        // Arrange
        var metricAccuracies = new Dictionary<string, double>
        {
            ["cpu_load"] = 0.92, ["memory"] = 0.88
        };

        // Act
        var calibration = new ForecastCalibration(
            100, 80, 20, 0.85, 0.90, 0.05, 0.03, metricAccuracies);

        // Assert
        calibration.TotalForecasts.Should().Be(100);
        calibration.VerifiedForecasts.Should().Be(80);
        calibration.FailedForecasts.Should().Be(20);
        calibration.AverageConfidence.Should().Be(0.85);
        calibration.AverageAccuracy.Should().Be(0.90);
        calibration.BrierScore.Should().Be(0.05);
        calibration.CalibrationError.Should().Be(0.03);
        calibration.MetricAccuracies.Should().HaveCount(2);
    }

    [Fact]
    public void ForecastStatus_AllValuesAreDefined()
    {
        // Act
        var values = Enum.GetValues<ForecastStatus>();

        // Assert
        values.Should().Contain(ForecastStatus.Pending);
        values.Should().Contain(ForecastStatus.Verified);
        values.Should().Contain(ForecastStatus.Failed);
        values.Should().Contain(ForecastStatus.Cancelled);
    }
}
