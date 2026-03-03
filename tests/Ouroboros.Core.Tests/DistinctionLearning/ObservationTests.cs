using Ouroboros.Core.DistinctionLearning;
using DLObservation = Ouroboros.Core.DistinctionLearning.Observation;

namespace Ouroboros.Tests.DistinctionLearning;

[Trait("Category", "Unit")]
public sealed class ObservationTests
{
    [Fact]
    public void WithCertainPrior_SetsHighCertainty()
    {
        var sut = DLObservation.WithCertainPrior("observed pattern");

        sut.Content.Should().Be("observed pattern");
        sut.PriorCertainty.Should().Be(0.9);
    }

    [Fact]
    public void WithCertainPrior_SetsDefaultSource()
    {
        var sut = DLObservation.WithCertainPrior("test");

        sut.Context.Should().ContainKey("source");
        sut.Context["source"].Should().Be("default");
    }

    [Fact]
    public void WithCertainPrior_CustomContextKey()
    {
        var sut = DLObservation.WithCertainPrior("test", "sensor_1");

        sut.Context["source"].Should().Be("sensor_1");
    }

    [Fact]
    public void WithUncertainPrior_SetsLowCertainty()
    {
        var sut = DLObservation.WithUncertainPrior("uncertain observation");

        sut.Content.Should().Be("uncertain observation");
        sut.PriorCertainty.Should().Be(0.3);
    }

    [Fact]
    public void WithUncertainPrior_SetsDefaultSource()
    {
        var sut = DLObservation.WithUncertainPrior("test");

        sut.Context["source"].Should().Be("default");
    }

    [Fact]
    public void WithUncertainPrior_CustomContextKey()
    {
        var sut = DLObservation.WithUncertainPrior("test", "noisy_sensor");

        sut.Context["source"].Should().Be("noisy_sensor");
    }

    [Fact]
    public void DirectConstruction_SetsAllProperties()
    {
        var now = DateTime.UtcNow;
        var ctx = new Dictionary<string, object> { ["env"] = "test" };
        var sut = new DLObservation("content", now, 0.6, ctx);

        sut.Content.Should().Be("content");
        sut.Timestamp.Should().Be(now);
        sut.PriorCertainty.Should().Be(0.6);
        sut.Context["env"].Should().Be("test");
    }

    [Fact]
    public void WithCertainPrior_SetsTimestamp()
    {
        var before = DateTime.UtcNow;
        var sut = DLObservation.WithCertainPrior("test");
        var after = DateTime.UtcNow;

        sut.Timestamp.Should().BeOnOrAfter(before);
        sut.Timestamp.Should().BeOnOrBefore(after);
    }
}
