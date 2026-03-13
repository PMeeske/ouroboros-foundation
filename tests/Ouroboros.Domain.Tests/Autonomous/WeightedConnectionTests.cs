namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class WeightedConnectionTests
{
    // ═══════════════════════════════════════════════════════════════
    // Construction
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void Constructor_SetsProperties()
    {
        var conn = new WeightedConnection("a", "b", 0.5, 0.02);

        conn.SourceNeuronId.Should().Be("a");
        conn.TargetNeuronId.Should().Be("b");
        conn.Weight.Should().Be(0.5);
        conn.PlasticityRate.Should().Be(0.02);
    }

    [Fact]
    public void Constructor_DefaultWeight_IsOne()
    {
        var conn = new WeightedConnection("a", "b");
        conn.Weight.Should().Be(1.0);
    }

    [Fact]
    public void Constructor_DefaultPlasticityRate_IsPointZeroOne()
    {
        var conn = new WeightedConnection("a", "b");
        conn.PlasticityRate.Should().Be(0.01);
    }

    [Theory]
    [InlineData(1.5, 1.0)]
    [InlineData(-1.5, -1.0)]
    [InlineData(0.0, 0.0)]
    [InlineData(0.75, 0.75)]
    [InlineData(-0.75, -0.75)]
    public void Constructor_ClampsWeight(double input, double expected)
    {
        var conn = new WeightedConnection("a", "b", input);
        conn.Weight.Should().Be(expected);
    }

    [Fact]
    public void Constructor_InitialActivationCount_IsZero()
    {
        var conn = new WeightedConnection("a", "b");
        conn.ActivationCount.Should().Be(0);
    }

    [Fact]
    public void IsFrozen_DefaultIsFalse()
    {
        var conn = new WeightedConnection("a", "b");
        conn.IsFrozen.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════
    // HebbianUpdate
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void HebbianUpdate_BothActive_Strengthens()
    {
        var conn = new WeightedConnection("a", "b", 0.5, 0.1);
        double before = conn.Weight;

        conn.HebbianUpdate(sourceActive: true, targetActive: true);

        conn.Weight.Should().BeGreaterThan(before);
    }

    [Fact]
    public void HebbianUpdate_SourceActiveTargetInactive_Weakens()
    {
        var conn = new WeightedConnection("a", "b", 0.5, 0.1);
        double before = conn.Weight;

        conn.HebbianUpdate(sourceActive: true, targetActive: false);

        conn.Weight.Should().BeLessThan(before);
    }

    [Fact]
    public void HebbianUpdate_NeitherActive_NoChange()
    {
        var conn = new WeightedConnection("a", "b", 0.5, 0.1);

        conn.HebbianUpdate(sourceActive: false, targetActive: false);

        conn.Weight.Should().Be(0.5);
    }

    [Fact]
    public void HebbianUpdate_SourceInactiveTargetActive_NoChange()
    {
        var conn = new WeightedConnection("a", "b", 0.5, 0.1);

        conn.HebbianUpdate(sourceActive: false, targetActive: true);

        conn.Weight.Should().Be(0.5);
    }

    [Fact]
    public void HebbianUpdate_Frozen_NoChange()
    {
        var conn = new WeightedConnection("a", "b", 0.5, 0.1) { IsFrozen = true };

        conn.HebbianUpdate(sourceActive: true, targetActive: true);

        conn.Weight.Should().Be(0.5);
    }

    [Fact]
    public void HebbianUpdate_WeightStaysClamped()
    {
        var conn = new WeightedConnection("a", "b", 0.99, 0.5);

        for (int i = 0; i < 100; i++)
        {
            conn.HebbianUpdate(sourceActive: true, targetActive: true);
        }

        conn.Weight.Should().BeLessThanOrEqualTo(1.0);
        conn.Weight.Should().BeGreaterThanOrEqualTo(-1.0);
    }

    // ═══════════════════════════════════════════════════════════════
    // RecordActivation
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void RecordActivation_IncrementsCount()
    {
        var conn = new WeightedConnection("a", "b");

        conn.RecordActivation();
        conn.RecordActivation();

        conn.ActivationCount.Should().Be(2);
    }

    [Fact]
    public void RecordActivation_UpdatesLastActivation()
    {
        var conn = new WeightedConnection("a", "b");
        var before = DateTimeOffset.UtcNow;

        conn.RecordActivation();

        conn.LastActivation.Should().BeOnOrAfter(before);
    }
}
