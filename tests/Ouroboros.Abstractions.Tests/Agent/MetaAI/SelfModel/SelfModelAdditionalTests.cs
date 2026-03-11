using Ouroboros.Agent.MetaAI.SelfModel;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI.SelfModel;

[Trait("Category", "Unit")]
public class SelfModelAdditionalTests
{
    [Fact]
    public void AnomalyDetection_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var causes = new List<string>();
        var ts = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new AnomalyDetection("metric", 10.0, 5.0, 5.0, true, "high", ts, causes);
        var b = new AnomalyDetection("metric", 10.0, 5.0, 5.0, true, "high", ts, causes);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void AnomalyDetection_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new AnomalyDetection(
            "cpu", 90.0, 50.0, 40.0, true, "critical",
            DateTime.UtcNow, new List<string> { "overload" });

        // Act
        var modified = original with { IsAnomaly = false, Severity = "low" };

        // Assert
        modified.IsAnomaly.Should().BeFalse();
        modified.Severity.Should().Be("low");
        modified.MetricName.Should().Be("cpu");
    }

    [Fact]
    public void Forecast_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ts = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var metadata = new Dictionary<string, object>();

        var a = new Forecast(id, "desc", "metric", 10.0, 0.9, ts, ts, ForecastStatus.Pending, null, metadata);
        var b = new Forecast(id, "desc", "metric", 10.0, 0.9, ts, ts, ForecastStatus.Pending, null, metadata);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void Forecast_WithExpression_CanUpdateStatus()
    {
        // Arrange
        var original = new Forecast(
            Guid.NewGuid(), "desc", "metric", 100.0, 0.9,
            DateTime.UtcNow, DateTime.UtcNow.AddHours(1),
            ForecastStatus.Pending, null, new Dictionary<string, object>());

        // Act
        var modified = original with
        {
            Status = ForecastStatus.Verified,
            ActualValue = 98.0
        };

        // Assert
        modified.Status.Should().Be(ForecastStatus.Verified);
        modified.ActualValue.Should().Be(98.0);
        modified.PredictedValue.Should().Be(100.0);
    }

    [Fact]
    public void Forecast_FailedStatus_IsValid()
    {
        // Act
        var forecast = new Forecast(
            Guid.NewGuid(), "desc", "metric", 50.0, 0.5,
            DateTime.UtcNow, DateTime.UtcNow,
            ForecastStatus.Failed, null, new Dictionary<string, object>());

        // Assert
        forecast.Status.Should().Be(ForecastStatus.Failed);
    }

    [Fact]
    public void ForecastCalibration_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var accuracies = new Dictionary<string, double>();

        var a = new ForecastCalibration(10, 8, 2, 0.9, 0.85, 0.1, 0.05, accuracies);
        var b = new ForecastCalibration(10, 8, 2, 0.9, 0.85, 0.1, 0.05, accuracies);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void ForecastCalibration_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new ForecastCalibration(
            10, 8, 2, 0.9, 0.85, 0.1, 0.05, new Dictionary<string, double>());

        // Act
        var modified = original with { TotalForecasts = 20, VerifiedForecasts = 18 };

        // Assert
        modified.TotalForecasts.Should().Be(20);
        modified.VerifiedForecasts.Should().Be(18);
        modified.AverageConfidence.Should().Be(0.9);
    }

    [Fact]
    public void ForecastStatus_CancelledStatus_Exists()
    {
        // Assert
        Enum.IsDefined(ForecastStatus.Cancelled).Should().BeTrue();
    }
}
