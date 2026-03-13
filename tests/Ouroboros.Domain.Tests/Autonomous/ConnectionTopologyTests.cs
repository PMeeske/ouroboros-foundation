namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class ConnectionTopologyTests
{
    private readonly ConnectionTopology _topology = new();

    // ═══════════════════════════════════════════════════════════════
    // SetConnection / GetWeight
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void SetConnection_NewConnection_CreatesIt()
    {
        _topology.SetConnection("a", "b", 0.5);

        _topology.GetWeight("a", "b").Should().Be(0.5);
    }

    [Fact]
    public void SetConnection_ExistingConnection_UpdatesWeight()
    {
        _topology.SetConnection("a", "b", 0.3);
        _topology.SetConnection("a", "b", 0.7);

        _topology.GetWeight("a", "b").Should().Be(0.7);
    }

    [Theory]
    [InlineData(1.5, 1.0)]
    [InlineData(-1.5, -1.0)]
    [InlineData(2.0, 1.0)]
    [InlineData(-2.0, -1.0)]
    public void SetConnection_WeightOutOfRange_Clamped(double input, double expected)
    {
        _topology.SetConnection("a", "b", input);
        _topology.GetWeight("a", "b").Should().Be(expected);
    }

    [Fact]
    public void GetWeight_NoConnection_ReturnsDefaultExcitatory()
    {
        _topology.GetWeight("x", "y").Should().Be(1.0);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetConnection
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void GetConnection_Exists_ReturnsConnection()
    {
        _topology.SetConnection("a", "b", 0.5);

        var conn = _topology.GetConnection("a", "b");
        conn.Should().NotBeNull();
        conn!.Weight.Should().Be(0.5);
    }

    [Fact]
    public void GetConnection_NotExists_ReturnsNull()
    {
        _topology.GetConnection("x", "y").Should().BeNull();
    }

    // ═══════════════════════════════════════════════════════════════
    // GetOutgoingConnections / GetIncomingConnections
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void GetOutgoingConnections_ReturnsCorrectConnections()
    {
        _topology.SetConnection("a", "b", 0.5);
        _topology.SetConnection("a", "c", 0.3);
        _topology.SetConnection("b", "c", 0.7);

        var outgoing = _topology.GetOutgoingConnections("a").ToList();
        outgoing.Should().HaveCount(2);
    }

    [Fact]
    public void GetIncomingConnections_ReturnsCorrectConnections()
    {
        _topology.SetConnection("a", "c", 0.5);
        _topology.SetConnection("b", "c", 0.3);

        var incoming = _topology.GetIncomingConnections("c").ToList();
        incoming.Should().HaveCount(2);
    }

    [Fact]
    public void GetOutgoingConnections_NoConnections_ReturnsEmpty()
    {
        _topology.GetOutgoingConnections("x").Should().BeEmpty();
    }

    // ═══════════════════════════════════════════════════════════════
    // ApplyHebbianUpdate
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void ApplyHebbianUpdate_BothActive_StrengthensConnection()
    {
        _topology.SetConnection("a", "b", 0.5);
        double before = _topology.GetWeight("a", "b");

        _topology.ApplyHebbianUpdate("a", "b", sourceActive: true, targetActive: true);

        _topology.GetWeight("a", "b").Should().BeGreaterThan(before);
    }

    [Fact]
    public void ApplyHebbianUpdate_SourceActiveTargetInactive_WeakensConnection()
    {
        _topology.SetConnection("a", "b", 0.5);
        double before = _topology.GetWeight("a", "b");

        _topology.ApplyHebbianUpdate("a", "b", sourceActive: true, targetActive: false);

        _topology.GetWeight("a", "b").Should().BeLessThan(before);
    }

    [Fact]
    public void ApplyHebbianUpdate_NoConnection_DoesNotThrow()
    {
        var act = () => _topology.ApplyHebbianUpdate("x", "y", true, true);
        act.Should().NotThrow();
    }

    // ═══════════════════════════════════════════════════════════════
    // AddInhibition
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void AddInhibition_Default_SetsNegativeWeight()
    {
        _topology.AddInhibition("a", "b");

        _topology.GetWeight("a", "b").Should().Be(-0.5);
    }

    [Fact]
    public void AddInhibition_CustomStrength_ClampsToRange()
    {
        _topology.AddInhibition("a", "b", -1.5);

        _topology.GetWeight("a", "b").Should().Be(-1.0);
    }

    [Fact]
    public void AddInhibition_PositiveStrength_ClampsToZero()
    {
        _topology.AddInhibition("a", "b", 0.5);

        _topology.GetWeight("a", "b").Should().Be(0.0);
    }

    // ═══════════════════════════════════════════════════════════════
    // ComputeNetInput
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void ComputeNetInput_SumsWeightedActivations()
    {
        _topology.SetConnection("a", "target", 0.5);
        _topology.SetConnection("b", "target", -0.3);

        double netInput = _topology.ComputeNetInput("target", id => id == "a" ? 1.0 : 0.5);

        // 0.5 * 1.0 + (-0.3) * 0.5 = 0.5 - 0.15 = 0.35
        netInput.Should().BeApproximately(0.35, 0.001);
    }

    [Fact]
    public void ComputeNetInput_NoIncomingConnections_ReturnsZero()
    {
        double result = _topology.ComputeNetInput("isolated", _ => 1.0);
        result.Should().Be(0.0);
    }

    // ═══════════════════════════════════════════════════════════════
    // GetWeightSnapshot
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void GetWeightSnapshot_ReturnsImmutableCopy()
    {
        _topology.SetConnection("a", "b", 0.5);
        _topology.SetConnection("b", "c", 0.7);

        var snapshot = _topology.GetWeightSnapshot();

        snapshot.Should().HaveCount(2);
        snapshot[("a", "b")].Should().Be(0.5);
        snapshot[("b", "c")].Should().Be(0.7);
    }
}
