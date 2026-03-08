namespace Ouroboros.Tests.Domain.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class WeightedConnectionTests
{
    [Fact]
    public void Constructor_DefaultWeight_IsOne()
    {
        // Act
        var conn = new WeightedConnection("src", "tgt");

        // Assert
        conn.Weight.Should().Be(1.0);
        conn.SourceNeuronId.Should().Be("src");
        conn.TargetNeuronId.Should().Be("tgt");
        conn.PlasticityRate.Should().Be(0.01);
    }

    [Fact]
    public void Constructor_ClampsWeightAboveOne()
    {
        // Act
        var conn = new WeightedConnection("src", "tgt", initialWeight: 5.0);

        // Assert
        conn.Weight.Should().Be(1.0);
    }

    [Fact]
    public void Constructor_ClampsWeightBelowNegativeOne()
    {
        // Act
        var conn = new WeightedConnection("src", "tgt", initialWeight: -5.0);

        // Assert
        conn.Weight.Should().Be(-1.0);
    }

    [Fact]
    public void Constructor_AcceptsNegativeWeight()
    {
        // Act
        var conn = new WeightedConnection("src", "tgt", initialWeight: -0.5);

        // Assert
        conn.Weight.Should().Be(-0.5);
    }

    [Fact]
    public void HebbianUpdate_BothActive_StrengthensConnection()
    {
        // Arrange
        var conn = new WeightedConnection("src", "tgt", initialWeight: 0.5, plasticityRate: 0.1);
        var originalWeight = conn.Weight;

        // Act
        conn.HebbianUpdate(sourceActive: true, targetActive: true);

        // Assert
        conn.Weight.Should().BeGreaterThan(originalWeight);
    }

    [Fact]
    public void HebbianUpdate_SourceOnlyActive_WeakensConnection()
    {
        // Arrange
        var conn = new WeightedConnection("src", "tgt", initialWeight: 0.5, plasticityRate: 0.1);
        var originalWeight = conn.Weight;

        // Act
        conn.HebbianUpdate(sourceActive: true, targetActive: false);

        // Assert
        conn.Weight.Should().BeLessThan(originalWeight);
    }

    [Fact]
    public void HebbianUpdate_NeitherActive_NoChange()
    {
        // Arrange
        var conn = new WeightedConnection("src", "tgt", initialWeight: 0.5);
        var originalWeight = conn.Weight;

        // Act
        conn.HebbianUpdate(sourceActive: false, targetActive: false);

        // Assert
        conn.Weight.Should().Be(originalWeight);
    }

    [Fact]
    public void HebbianUpdate_Frozen_NoChange()
    {
        // Arrange
        var conn = new WeightedConnection("src", "tgt", initialWeight: 0.5, plasticityRate: 0.1);
        conn.IsFrozen = true;
        var originalWeight = conn.Weight;

        // Act
        conn.HebbianUpdate(sourceActive: true, targetActive: true);

        // Assert
        conn.Weight.Should().Be(originalWeight);
    }

    [Fact]
    public void HebbianUpdate_WeightStaysClamped()
    {
        // Arrange
        var conn = new WeightedConnection("src", "tgt", initialWeight: 0.99, plasticityRate: 0.5);

        // Act - many co-activations
        for (int i = 0; i < 100; i++)
        {
            conn.HebbianUpdate(sourceActive: true, targetActive: true);
        }

        // Assert
        conn.Weight.Should().BeLessThanOrEqualTo(1.0);
        conn.Weight.Should().BeGreaterThanOrEqualTo(-1.0);
    }

    [Fact]
    public void RecordActivation_IncrementsCount()
    {
        // Arrange
        var conn = new WeightedConnection("src", "tgt");

        // Act
        conn.RecordActivation();
        conn.RecordActivation();
        conn.RecordActivation();

        // Assert
        conn.ActivationCount.Should().Be(3);
    }

    [Fact]
    public void RecordActivation_UpdatesLastActivation()
    {
        // Arrange
        var conn = new WeightedConnection("src", "tgt");
        var before = DateTimeOffset.UtcNow;

        // Act
        conn.RecordActivation();

        // Assert
        conn.LastActivation.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void ActivationCount_InitiallyZero()
    {
        // Act
        var conn = new WeightedConnection("src", "tgt");

        // Assert
        conn.ActivationCount.Should().Be(0);
    }

    [Fact]
    public void IsFrozen_DefaultsFalse()
    {
        // Act
        var conn = new WeightedConnection("src", "tgt");

        // Assert
        conn.IsFrozen.Should().BeFalse();
    }
}
