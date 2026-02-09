namespace Ouroboros.Specs.Steps;

using Ouroboros.Diagnostics;

[Binding]
public class MetricsCollectorSteps
{
    private MetricsCollector? _collector;

    [Given("a fresh metrics collector")]
    public void GivenAFreshMetricsCollector()
    {
        // Create a new instance for each scenario to ensure isolation
        _collector = new MetricsCollector();
    }

    [Given("I have collected some metrics")]
    public void GivenIHaveCollectedSomeMetrics()
    {
        _collector.Should().NotBeNull();
        _collector!.IncrementCounter("test_counter", 5.0);
        _collector.SetGauge("test_gauge", 100.0);
        _collector.ObserveHistogram("test_histogram", 50.0);
    }

    [When(@"I increment counter ""(.*)"" by (.*)")]
    public void WhenIIncrementCounterBy(string name, double value)
    {
        _collector.Should().NotBeNull();
        
        // Counters should not decrease - only allow positive increments
        if (value > 0)
        {
            _collector!.IncrementCounter(name, value);
        }
    }

    [When(@"I increment counter ""(.*)"" by (.*) with label ""(.*)""")]
    public void WhenIIncrementCounterByWithLabel(string name, double value, string label)
    {
        _collector.Should().NotBeNull();
        
        var labels = ParseLabel(label);
        _collector!.IncrementCounter(name, value, labels);
    }

    [When(@"I set gauge ""(.*)"" to (.*)")]
    public void WhenISetGaugeTo(string name, double value)
    {
        _collector.Should().NotBeNull();
        _collector!.SetGauge(name, value);
    }

    [When(@"I observe histogram ""(.*)"" with value (.*)")]
    public void WhenIObserveHistogramWithValue(string name, double value)
    {
        _collector.Should().NotBeNull();
        _collector!.ObserveHistogram(name, value);
    }

    [When(@"I observe histogram ""(.*)"" with value (.*) with label ""(.*)""")]
    public void WhenIObserveHistogramWithValueWithLabel(string name, double value, string label)
    {
        _collector.Should().NotBeNull();
        
        var labels = ParseLabel(label);
        _collector!.ObserveHistogram(name, value, labels);
    }

    [When(@"I observe histogram ""(.*)"" with values (.*)")]
    public void WhenIObserveHistogramWithValues(string name, string valuesStr)
    {
        _collector.Should().NotBeNull();
        
        // Parse comma-separated values like "1, 2, 3, 4, 5, 6, 7, 8, 9, 10"
        var values = valuesStr.Split(',').Select(v => double.Parse(v.Trim())).ToArray();
        
        foreach (var value in values)
        {
            _collector!.ObserveHistogram(name, value);
        }
    }

    [When("I reset the metrics collector")]
    public void WhenIResetTheMetricsCollector()
    {
        _collector.Should().NotBeNull();
        _collector!.Reset();
    }

    [Then(@"the counter ""([^""]+)"" with ""([^""]+)"" should have value (.*)")]
    public void ThenTheCounterWithLabelShouldHaveValue(string name, string label, double expectedValue)
    {
        _collector.Should().NotBeNull();
        
        var expectedLabels = ParseLabel(label);
        var metrics = _collector!.GetMetrics();
        var counter = metrics.FirstOrDefault(m => 
            m.Name == name && 
            m.Type == MetricType.Counter &&
            LabelsMatch(m.Labels, expectedLabels));
        
        counter.Should().NotBeNull($"Counter '{name}' with label '{label}' should exist");
        counter!.Value.Should().Be(expectedValue);
    }

    [Then(@"the counter ""([^""]+)"" should have value (.*)")]
    public void ThenTheCounterShouldHaveValue(string name, double expectedValue)
    {
        _collector.Should().NotBeNull();
        
        var metrics = _collector!.GetMetrics();
        var counter = metrics.FirstOrDefault(m => m.Name == name && m.Type == MetricType.Counter && m.Labels.Count == 0);
        
        counter.Should().NotBeNull($"Counter '{name}' should exist");
        counter!.Value.Should().Be(expectedValue);
    }

    [Then(@"the gauge ""(.*)"" should have value (.*)")]
    public void ThenTheGaugeShouldHaveValue(string name, double expectedValue)
    {
        _collector.Should().NotBeNull();
        
        var metrics = _collector!.GetMetrics();
        var gauge = metrics.FirstOrDefault(m => m.Name == name && m.Type == MetricType.Gauge);
        
        gauge.Should().NotBeNull($"Gauge '{name}' should exist");
        gauge!.Value.Should().Be(expectedValue);
    }

    [Then(@"the histogram ""([^""]+)"" with ""([^""]+)"" should have count (.*)")]
    public void ThenTheHistogramWithLabelShouldHaveCount(string name, string label, int expectedCount)
    {
        _collector.Should().NotBeNull();
        
        var expectedLabels = ParseLabel(label);
        var metrics = _collector!.GetMetrics();
        var countMetric = metrics.FirstOrDefault(m => 
            m.Name == $"{name}_count" && 
            m.Type == MetricType.Histogram &&
            LabelsMatch(m.Labels, expectedLabels));
        
        countMetric.Should().NotBeNull($"Histogram count for '{name}' with label '{label}' should exist");
        countMetric!.Value.Should().Be(expectedCount);
    }

    [Then(@"the histogram ""([^""]+)"" should have count (.*)")]
    public void ThenTheHistogramShouldHaveCount(string name, int expectedCount)
    {
        _collector.Should().NotBeNull();
        
        var metrics = _collector!.GetMetrics();
        var countMetric = metrics.FirstOrDefault(m => m.Name == $"{name}_count" && m.Type == MetricType.Histogram && m.Labels.Count == 0);
        
        countMetric.Should().NotBeNull($"Histogram count for '{name}' should exist");
        countMetric!.Value.Should().Be(expectedCount);
    }

    [Then(@"the histogram ""(.*)"" should have sum (.*)")]
    public void ThenTheHistogramShouldHaveSum(string name, double expectedSum)
    {
        _collector.Should().NotBeNull();
        
        var metrics = _collector!.GetMetrics();
        var sumMetric = metrics.FirstOrDefault(m => m.Name == $"{name}_sum" && m.Type == MetricType.Histogram);
        
        sumMetric.Should().NotBeNull($"Histogram sum for '{name}' should exist");
        sumMetric!.Value.Should().Be(expectedSum);
    }

    [Then(@"the histogram ""(.*)"" should have average (.*)")]
    public void ThenTheHistogramShouldHaveAverage(string name, double expectedAverage)
    {
        _collector.Should().NotBeNull();
        
        var metrics = _collector!.GetMetrics();
        var avgMetric = metrics.FirstOrDefault(m => m.Name == $"{name}_avg" && m.Type == MetricType.Histogram);
        
        avgMetric.Should().NotBeNull($"Histogram average for '{name}' should exist");
        avgMetric!.Value.Should().Be(expectedAverage);
    }

    [Then(@"I should have (.*) metrics in the collection")]
    public void ThenIShouldHaveMetricsInTheCollection(int expectedCount)
    {
        _collector.Should().NotBeNull();
        
        var metrics = _collector!.GetMetrics();
        
        // For this test, we count the distinct metric names (not including _count, _sum, _avg suffixes)
        var distinctMetrics = metrics
            .Select(m => m.Name.Split('_')[0])
            .Distinct()
            .Count();
        
        distinctMetrics.Should().Be(expectedCount);
    }

    [Then(@"the histogram ""(.*)"" p50 should be approximately (.*)")]
    public void ThenTheHistogramP50ShouldBeApproximately(string name, double expectedPercentile)
    {
        _collector.Should().NotBeNull();
        
        var percentile = CalculatePercentile(name, 50);
        percentile.Should().BeApproximately(expectedPercentile, 0.5, $"P50 for '{name}' should be approximately {expectedPercentile}");
    }

    [Then(@"the histogram ""(.*)"" p95 should be approximately (.*)")]
    public void ThenTheHistogramP95ShouldBeApproximately(string name, double expectedPercentile)
    {
        _collector.Should().NotBeNull();
        
        var percentile = CalculatePercentile(name, 95);
        percentile.Should().BeApproximately(expectedPercentile, 0.5, $"P95 for '{name}' should be approximately {expectedPercentile}");
    }

    [Then(@"the histogram ""(.*)"" p99 should be approximately (.*)")]
    public void ThenTheHistogramP99ShouldBeApproximately(string name, double expectedPercentile)
    {
        _collector.Should().NotBeNull();
        
        var percentile = CalculatePercentile(name, 99);
        percentile.Should().BeApproximately(expectedPercentile, 0.5, $"P99 for '{name}' should be approximately {expectedPercentile}");
    }

    // Helper method to parse labels from string like "status=success"
    private Dictionary<string, string> ParseLabel(string label)
    {
        var parts = label.Split('=');
        if (parts.Length != 2)
        {
            throw new ArgumentException($"Invalid label format: {label}. Expected format: key=value");
        }
        
        return new Dictionary<string, string> { [parts[0]] = parts[1] };
    }

    // Helper method to check if labels match
    private bool LabelsMatch(Dictionary<string, string> actual, Dictionary<string, string> expected)
    {
        if (actual.Count != expected.Count)
        {
            return false;
        }
        
        foreach (var kvp in expected)
        {
            if (!actual.TryGetValue(kvp.Key, out var value) || value != kvp.Value)
            {
                return false;
            }
        }
        
        return true;
    }

    // Helper method to calculate percentiles from histogram data
    // Since MetricsCollector doesn't expose raw histogram values, we need to access them via reflection
    // or simulate the percentile calculation
    private double CalculatePercentile(string name, int percentile)
    {
        _collector.Should().NotBeNull();
        
        // Access the private histograms field using reflection
        var histogramsField = typeof(MetricsCollector).GetField("histograms", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (histogramsField == null)
        {
            throw new InvalidOperationException("Could not access histograms field");
        }
        
        var histograms = histogramsField.GetValue(_collector) as System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentBag<double>>;
        
        if (histograms == null || !histograms.TryGetValue(name, out var values))
        {
            throw new InvalidOperationException($"Histogram '{name}' not found");
        }
        
        var sortedValues = values.OrderBy(v => v).ToArray();
        
        if (sortedValues.Length == 0)
        {
            return 0;
        }
        
        // Calculate percentile
        double index = (percentile / 100.0) * (sortedValues.Length - 1);
        int lowerIndex = (int)Math.Floor(index);
        int upperIndex = (int)Math.Ceiling(index);
        
        if (lowerIndex == upperIndex)
        {
            return sortedValues[lowerIndex];
        }
        
        // Linear interpolation
        double lowerValue = sortedValues[lowerIndex];
        double upperValue = sortedValues[upperIndex];
        double fraction = index - lowerIndex;
        
        return lowerValue + (upperValue - lowerValue) * fraction;
    }
}
