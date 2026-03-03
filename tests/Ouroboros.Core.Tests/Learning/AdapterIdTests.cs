using Ouroboros.Core.Learning;

namespace Ouroboros.Tests.Learning;

[Trait("Category", "Unit")]
public sealed class AdapterIdTests
{
    [Fact]
    public void NewId_CreatesUniqueId()
    {
        var id1 = AdapterId.NewId();
        var id2 = AdapterId.NewId();

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void NewId_HasNonEmptyGuid()
    {
        var sut = AdapterId.NewId();

        sut.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void FromString_ValidGuid_ReturnsSome()
    {
        var guid = Guid.NewGuid();
        var result = AdapterId.FromString(guid.ToString());

        result.HasValue.Should().BeTrue();
        result.Value!.Value.Should().Be(guid);
    }

    [Fact]
    public void FromString_InvalidString_ReturnsNone()
    {
        var result = AdapterId.FromString("not-a-guid");

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void FromString_EmptyString_ReturnsNone()
    {
        var result = AdapterId.FromString("");

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        var guid = Guid.NewGuid();
        var sut = new AdapterId(guid);

        sut.ToString().Should().Be(guid.ToString());
    }

    [Fact]
    public void Equality_SameGuid_AreEqual()
    {
        var guid = Guid.NewGuid();
        var id1 = new AdapterId(guid);
        var id2 = new AdapterId(guid);

        id1.Should().Be(id2);
    }

    [Fact]
    public void Equality_DifferentGuid_AreNotEqual()
    {
        var id1 = AdapterId.NewId();
        var id2 = AdapterId.NewId();

        id1.Should().NotBe(id2);
    }
}
