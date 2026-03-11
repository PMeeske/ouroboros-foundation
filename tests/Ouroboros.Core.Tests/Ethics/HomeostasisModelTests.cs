using Ouroboros.Core.Ethics;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class EthicalTensionTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var sut = new EthicalTension
        {
            Id = "tension-1",
            Description = "Autonomy vs Safety conflict",
            TraditionsInvolved = new[] { "Kantian", "Utilitarian" },
            Intensity = 0.75
        };

        sut.Id.Should().Be("tension-1");
        sut.Description.Should().Be("Autonomy vs Safety conflict");
        sut.TraditionsInvolved.Should().HaveCount(2);
        sut.Intensity.Should().Be(0.75);
    }

    [Fact]
    public void Construction_Defaults_DetectedAt_IsRecentUtcNow()
    {
        var before = DateTime.UtcNow;

        var sut = new EthicalTension
        {
            Id = "t",
            Description = "d",
            TraditionsInvolved = Array.Empty<string>(),
            Intensity = 0.5
        };

        var after = DateTime.UtcNow;
        sut.DetectedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Construction_Defaults_IsResolvable_IsFalse()
    {
        var sut = new EthicalTension
        {
            Id = "t",
            Description = "d",
            TraditionsInvolved = Array.Empty<string>(),
            Intensity = 0.5
        };

        sut.IsResolvable.Should().BeFalse();
    }

    [Fact]
    public void Construction_WithIsResolvableTrue_SetsValue()
    {
        var sut = new EthicalTension
        {
            Id = "t",
            Description = "d",
            TraditionsInvolved = Array.Empty<string>(),
            Intensity = 0.5,
            IsResolvable = true
        };

        sut.IsResolvable.Should().BeTrue();
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var traditions = new[] { "Kantian" };

        var a = new EthicalTension
        {
            Id = "t1", Description = "d", TraditionsInvolved = traditions,
            Intensity = 0.5, DetectedAt = timestamp, IsResolvable = false
        };
        var b = new EthicalTension
        {
            Id = "t1", Description = "d", TraditionsInvolved = traditions,
            Intensity = 0.5, DetectedAt = timestamp, IsResolvable = false
        };

        a.Should().Be(b);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var original = new EthicalTension
        {
            Id = "t1",
            Description = "d",
            TraditionsInvolved = Array.Empty<string>(),
            Intensity = 0.3
        };

        var modified = original with { Intensity = 0.9 };

        modified.Intensity.Should().Be(0.9);
        original.Intensity.Should().Be(0.3);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class HomeostasisEventTests
{
    private static HomeostasisSnapshot CreateSnapshot(double balance = 0.8, bool stable = true) =>
        new()
        {
            OverallBalance = balance,
            ActiveTensions = Array.Empty<EthicalTension>(),
            TraditionWeights = new Dictionary<string, double>(),
            UnresolvedParadoxCount = 0,
            IsStable = stable
        };

    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var before = CreateSnapshot(0.9, true);
        var after = CreateSnapshot(0.6, false);

        var sut = new HomeostasisEvent
        {
            EventType = "TensionRegistered",
            Description = "New ethical tension detected",
            Before = before,
            After = after
        };

        sut.EventType.Should().Be("TensionRegistered");
        sut.Description.Should().Be("New ethical tension detected");
        sut.Before.OverallBalance.Should().Be(0.9);
        sut.After.OverallBalance.Should().Be(0.6);
    }

    [Fact]
    public void Construction_Defaults_OccurredAt_IsRecentUtcNow()
    {
        var beforeTime = DateTime.UtcNow;

        var sut = new HomeostasisEvent
        {
            EventType = "e",
            Description = "d",
            Before = CreateSnapshot(),
            After = CreateSnapshot()
        };

        var afterTime = DateTime.UtcNow;
        sut.OccurredAt.Should().BeOnOrAfter(beforeTime).And.BeOnOrBefore(afterTime);
    }

    [Fact]
    public void Construction_WithExplicitTimestamp_SetsValue()
    {
        var timestamp = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var sut = new HomeostasisEvent
        {
            EventType = "e",
            Description = "d",
            Before = CreateSnapshot(),
            After = CreateSnapshot(),
            OccurredAt = timestamp
        };

        sut.OccurredAt.Should().Be(timestamp);
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class HomeostasisSnapshotTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsValues()
    {
        var tensions = new[]
        {
            new EthicalTension
            {
                Id = "t1",
                Description = "tension",
                TraditionsInvolved = new[] { "Ahimsa", "Kantian" },
                Intensity = 0.6
            }
        };
        var weights = new Dictionary<string, double>
        {
            ["Ahimsa"] = 0.5,
            ["Kantian"] = 0.5
        };

        var sut = new HomeostasisSnapshot
        {
            OverallBalance = 0.7,
            ActiveTensions = tensions,
            TraditionWeights = weights,
            UnresolvedParadoxCount = 1,
            IsStable = false
        };

        sut.OverallBalance.Should().Be(0.7);
        sut.ActiveTensions.Should().HaveCount(1);
        sut.TraditionWeights.Should().HaveCount(2);
        sut.UnresolvedParadoxCount.Should().Be(1);
        sut.IsStable.Should().BeFalse();
    }

    [Fact]
    public void Construction_Defaults_Timestamp_IsRecentUtcNow()
    {
        var before = DateTime.UtcNow;

        var sut = new HomeostasisSnapshot
        {
            OverallBalance = 1.0,
            ActiveTensions = Array.Empty<EthicalTension>(),
            TraditionWeights = new Dictionary<string, double>(),
            UnresolvedParadoxCount = 0,
            IsStable = true
        };

        var after = DateTime.UtcNow;
        sut.Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Construction_FullyBalanced_HasExpectedState()
    {
        var sut = new HomeostasisSnapshot
        {
            OverallBalance = 1.0,
            ActiveTensions = Array.Empty<EthicalTension>(),
            TraditionWeights = new Dictionary<string, double> { ["Ubuntu"] = 1.0 },
            UnresolvedParadoxCount = 0,
            IsStable = true
        };

        sut.OverallBalance.Should().Be(1.0);
        sut.ActiveTensions.Should().BeEmpty();
        sut.UnresolvedParadoxCount.Should().Be(0);
        sut.IsStable.Should().BeTrue();
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var tensions = Array.Empty<EthicalTension>();
        var weights = new Dictionary<string, double>();
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new HomeostasisSnapshot
        {
            OverallBalance = 0.5, ActiveTensions = tensions,
            TraditionWeights = weights, UnresolvedParadoxCount = 2,
            IsStable = false, Timestamp = timestamp
        };
        var b = new HomeostasisSnapshot
        {
            OverallBalance = 0.5, ActiveTensions = tensions,
            TraditionWeights = weights, UnresolvedParadoxCount = 2,
            IsStable = false, Timestamp = timestamp
        };

        a.Should().Be(b);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var original = new HomeostasisSnapshot
        {
            OverallBalance = 0.5,
            ActiveTensions = Array.Empty<EthicalTension>(),
            TraditionWeights = new Dictionary<string, double>(),
            UnresolvedParadoxCount = 0,
            IsStable = false
        };

        var modified = original with { OverallBalance = 0.9, IsStable = true };

        modified.OverallBalance.Should().Be(0.9);
        modified.IsStable.Should().BeTrue();
        original.OverallBalance.Should().Be(0.5);
        original.IsStable.Should().BeFalse();
    }
}
