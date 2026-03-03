namespace Ouroboros.Tests.Domain.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class ConnectionTopologyTests
{
    private readonly ConnectionTopology _sut = new();

    [Fact]
    public void SetConnection_StoresWeightCorrectly()
    {
        // Act
        _sut.SetConnection("A", "B", 0.75);

        // Assert
        _sut.GetWeight("A", "B").Should().Be(0.75);
    }

    [Fact]
    public void GetWeight_NoConnection_ReturnsDefaultExcitatory()
    {
        // Act
        double weight = _sut.GetWeight("X", "Y");

        // Assert
        weight.Should().Be(1.0);
    }

    [Fact]
    public void SetConnection_OverwritesExistingConnection()
    {
        // Arrange
        _sut.SetConnection("A", "B", 0.5);

        // Act
        _sut.SetConnection("A", "B", 0.9);

        // Assert
        _sut.GetWeight("A", "B").Should().Be(0.9);
    }

    [Fact]
    public void GetConnection_Exists_ReturnsConnection()
    {
        // Arrange
        _sut.SetConnection("A", "B", 0.6);

        // Act
        WeightedConnection? conn = _sut.GetConnection("A", "B");

        // Assert
        conn.Should().NotBeNull();
        conn!.SourceNeuronId.Should().Be("A");
        conn.TargetNeuronId.Should().Be("B");
        conn.Weight.Should().Be(0.6);
    }

    [Fact]
    public void GetConnection_NotExists_ReturnsNull()
    {
        // Act
        WeightedConnection? conn = _sut.GetConnection("X", "Y");

        // Assert
        conn.Should().BeNull();
    }

    [Fact]
    public void GetOutgoingConnections_ReturnsOnlySourceConnections()
    {
        // Arrange
        _sut.SetConnection("A", "B", 0.5);
        _sut.SetConnection("A", "C", 0.7);
        _sut.SetConnection("B", "C", 0.3);

        // Act
        var outgoing = _sut.GetOutgoingConnections("A").ToList();

        // Assert
        outgoing.Should().HaveCount(2);
        outgoing.Select(c => c.TargetNeuronId).Should().Contain("B").And.Contain("C");
    }

    [Fact]
    public void GetOutgoingConnections_NoConnections_ReturnsEmpty()
    {
        // Act
        var outgoing = _sut.GetOutgoingConnections("NONE").ToList();

        // Assert
        outgoing.Should().BeEmpty();
    }

    [Fact]
    public void GetIncomingConnections_ReturnsOnlyTargetConnections()
    {
        // Arrange
        _sut.SetConnection("A", "C", 0.5);
        _sut.SetConnection("B", "C", 0.7);
        _sut.SetConnection("A", "B", 0.3);

        // Act
        var incoming = _sut.GetIncomingConnections("C").ToList();

        // Assert
        incoming.Should().HaveCount(2);
        incoming.Select(c => c.SourceNeuronId).Should().Contain("A").And.Contain("B");
    }

    [Fact]
    public void AddInhibition_SetsNegativeWeight()
    {
        // Act
        _sut.AddInhibition("A", "B");

        // Assert
        _sut.GetWeight("A", "B").Should().Be(-0.5);
    }

    [Fact]
    public void AddInhibition_ClampsToNegativeRange()
    {
        // Act
        _sut.AddInhibition("A", "B", -2.0);

        // Assert
        _sut.GetWeight("A", "B").Should().Be(-1.0);
    }

    [Fact]
    public void AddInhibition_ClampsPositiveToZero()
    {
        // Act
        _sut.AddInhibition("A", "B", 0.5);

        // Assert
        _sut.GetWeight("A", "B").Should().Be(0.0);
    }

    [Fact]
    public void ApplyHebbianUpdate_ExistingConnection_UpdatesWeight()
    {
        // Arrange
        _sut.SetConnection("A", "B", 0.5, plasticityRate: 0.1);
        double originalWeight = _sut.GetWeight("A", "B");

        // Act
        _sut.ApplyHebbianUpdate("A", "B", sourceActive: true, targetActive: true);

        // Assert
        _sut.GetWeight("A", "B").Should().BeGreaterThan(originalWeight);
    }

    [Fact]
    public void ApplyHebbianUpdate_NonExistentConnection_DoesNothing()
    {
        // Act & Assert - should not throw
        _sut.ApplyHebbianUpdate("X", "Y", sourceActive: true, targetActive: true);

        // Still returns default
        _sut.GetWeight("X", "Y").Should().Be(1.0);
    }

    [Fact]
    public void GetWeightSnapshot_ReturnsImmutableCopy()
    {
        // Arrange
        _sut.SetConnection("A", "B", 0.5);
        _sut.SetConnection("C", "D", -0.3);

        // Act
        var snapshot = _sut.GetWeightSnapshot();

        // Assert
        snapshot.Should().HaveCount(2);
        snapshot[("A", "B")].Should().Be(0.5);
        snapshot[("C", "D")].Should().Be(-0.3);
    }

    [Fact]
    public void ComputeNetInput_SumsWeightedActivations()
    {
        // Arrange
        _sut.SetConnection("A", "C", 0.8);
        _sut.SetConnection("B", "C", 0.5);

        // Act
        double netInput = _sut.ComputeNetInput("C", id => id switch
        {
            "A" => 1.0,
            "B" => 0.5,
            _ => 0.0
        });

        // Assert
        // Expected: 0.8 * 1.0 + 0.5 * 0.5 = 1.05
        netInput.Should().BeApproximately(1.05, 0.001);
    }

    [Fact]
    public void ComputeNetInput_NoIncoming_ReturnsZero()
    {
        // Act
        double netInput = _sut.ComputeNetInput("X", _ => 1.0);

        // Assert
        netInput.Should().Be(0.0);
    }

    [Fact]
    public void ComputeNetInput_InhibitoryConnections_ReducesInput()
    {
        // Arrange
        _sut.SetConnection("A", "C", 0.8);
        _sut.AddInhibition("B", "C", -0.6);

        // Act
        double netInput = _sut.ComputeNetInput("C", _ => 1.0);

        // Assert
        // Expected: 0.8 * 1.0 + (-0.6) * 1.0 = 0.2
        netInput.Should().BeApproximately(0.2, 0.001);
    }

    [Fact]
    public void SetConnection_CustomPlasticityRate_IsPreserved()
    {
        // Act
        _sut.SetConnection("A", "B", 0.5, plasticityRate: 0.05);

        // Assert
        var conn = _sut.GetConnection("A", "B");
        conn.Should().NotBeNull();
        conn!.PlasticityRate.Should().Be(0.05);
    }
}
