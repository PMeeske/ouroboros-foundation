using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class DistinctionEventTypeTests
{
    [Theory]
    [InlineData(DistinctionEventType.DistinctionDrawn, 0)]
    [InlineData(DistinctionEventType.Crossed, 1)]
    [InlineData(DistinctionEventType.Condensed, 2)]
    [InlineData(DistinctionEventType.Cancelled, 3)]
    [InlineData(DistinctionEventType.ReEntryCreated, 4)]
    [InlineData(DistinctionEventType.Collapsed, 5)]
    [InlineData(DistinctionEventType.PatternMatched, 6)]
    [InlineData(DistinctionEventType.InferenceDerived, 7)]
    public void EnumValues_HaveExpectedOrdinals(DistinctionEventType value, int expected)
    {
        ((int)value).Should().Be(expected);
    }

    [Fact]
    public void AllValues_AreDefined()
    {
        var values = Enum.GetValues<DistinctionEventType>();

        values.Should().HaveCount(8);
    }

    [Fact]
    public void Enum_IsOfTypeInt()
    {
        Enum.GetUnderlyingType(typeof(DistinctionEventType)).Should().Be(typeof(int));
    }
}
