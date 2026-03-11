using System.Reflection;
using Ouroboros.Core.Ethics.MeTTa;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class EthicalAtomEnumTests
{
    [Fact]
    public void EthicalAtom_HasNineMembers()
    {
        Enum.GetValues<EthicalAtom>().Should().HaveCount(9);
    }

    [Theory]
    [InlineData(EthicalAtom.CoreEthics, 0)]
    [InlineData(EthicalAtom.Ahimsa, 1)]
    [InlineData(EthicalAtom.Levinas, 2)]
    [InlineData(EthicalAtom.Nagarjuna, 3)]
    [InlineData(EthicalAtom.Paradox, 4)]
    [InlineData(EthicalAtom.Ubuntu, 5)]
    [InlineData(EthicalAtom.Kantian, 6)]
    [InlineData(EthicalAtom.BhagavadGita, 7)]
    [InlineData(EthicalAtom.WisdomOfDisagreement, 8)]
    public void EthicalAtom_HasExpectedIntegerValue(EthicalAtom atom, int expected)
    {
        ((int)atom).Should().Be(expected);
    }

    [Theory]
    [InlineData(EthicalAtom.CoreEthics)]
    [InlineData(EthicalAtom.Ahimsa)]
    [InlineData(EthicalAtom.Levinas)]
    [InlineData(EthicalAtom.Nagarjuna)]
    [InlineData(EthicalAtom.Paradox)]
    [InlineData(EthicalAtom.Ubuntu)]
    [InlineData(EthicalAtom.Kantian)]
    [InlineData(EthicalAtom.BhagavadGita)]
    [InlineData(EthicalAtom.WisdomOfDisagreement)]
    public void EthicalAtom_EachMemberHasMeTTaHashAttribute(EthicalAtom atom)
    {
        var field = typeof(EthicalAtom).GetField(atom.ToString());
        var attr = field?.GetCustomAttribute<MeTTaHashAttribute>();

        attr.Should().NotBeNull();
    }

    [Theory]
    [InlineData(EthicalAtom.CoreEthics)]
    [InlineData(EthicalAtom.Ahimsa)]
    [InlineData(EthicalAtom.Levinas)]
    [InlineData(EthicalAtom.Nagarjuna)]
    [InlineData(EthicalAtom.Paradox)]
    [InlineData(EthicalAtom.Ubuntu)]
    [InlineData(EthicalAtom.Kantian)]
    [InlineData(EthicalAtom.BhagavadGita)]
    [InlineData(EthicalAtom.WisdomOfDisagreement)]
    public void EthicalAtom_EachMemberHashAttribute_HasNonEmptySha256(EthicalAtom atom)
    {
        var field = typeof(EthicalAtom).GetField(atom.ToString());
        var attr = field!.GetCustomAttribute<MeTTaHashAttribute>()!;

        attr.Sha256.Should().NotBeNullOrWhiteSpace();
        attr.Sha256.Should().HaveLength(64, "SHA-256 hex digest is 64 characters");
    }

    [Theory]
    [InlineData(EthicalAtom.CoreEthics)]
    [InlineData(EthicalAtom.Ahimsa)]
    [InlineData(EthicalAtom.Levinas)]
    [InlineData(EthicalAtom.Nagarjuna)]
    [InlineData(EthicalAtom.Paradox)]
    [InlineData(EthicalAtom.Ubuntu)]
    [InlineData(EthicalAtom.Kantian)]
    [InlineData(EthicalAtom.BhagavadGita)]
    [InlineData(EthicalAtom.WisdomOfDisagreement)]
    public void EthicalAtom_EachMemberHashAttribute_HasNonEmptyResourceName(EthicalAtom atom)
    {
        var field = typeof(EthicalAtom).GetField(atom.ToString());
        var attr = field!.GetCustomAttribute<MeTTaHashAttribute>()!;

        attr.ResourceName.Should().NotBeNullOrWhiteSpace();
        attr.ResourceName.Should().EndWith(".metta");
    }

    [Fact]
    public void EthicalAtom_AllResourceNames_AreUnique()
    {
        var resourceNames = Enum.GetValues<EthicalAtom>()
            .Select(atom =>
            {
                var field = typeof(EthicalAtom).GetField(atom.ToString());
                return field!.GetCustomAttribute<MeTTaHashAttribute>()!.ResourceName;
            })
            .ToList();

        resourceNames.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void EthicalAtom_AllHashes_AreUnique()
    {
        var hashes = Enum.GetValues<EthicalAtom>()
            .Select(atom =>
            {
                var field = typeof(EthicalAtom).GetField(atom.ToString());
                return field!.GetCustomAttribute<MeTTaHashAttribute>()!.Sha256;
            })
            .ToList();

        hashes.Should().OnlyHaveUniqueItems();
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class MeTTaHashAttributeTests
{
    [Fact]
    public void Construction_SetsProperties()
    {
        var sha256 = "abcdef1234567890abcdef1234567890abcdef1234567890abcdef1234567890";
        var resourceName = "Ouroboros.Ethics.MeTTa.test.metta";

        var sut = new MeTTaHashAttribute(sha256, resourceName);

        sut.Sha256.Should().Be(sha256);
        sut.ResourceName.Should().Be(resourceName);
    }

    [Fact]
    public void Attribute_TargetsFieldOnly()
    {
        var usage = typeof(MeTTaHashAttribute)
            .GetCustomAttribute<AttributeUsageAttribute>();

        usage.Should().NotBeNull();
        usage!.ValidOn.Should().Be(AttributeTargets.Field);
        usage.AllowMultiple.Should().BeFalse();
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class EthicalAtomTamperedExceptionTests
{
    [Fact]
    public void Construction_SetsMessage()
    {
        var exception = new EthicalAtomTamperedException("Tampering detected");

        exception.Message.Should().Be("Tampering detected");
    }

    [Fact]
    public void IsException()
    {
        var exception = new EthicalAtomTamperedException("test");

        exception.Should().BeAssignableTo<Exception>();
    }
}

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class EthicalAtomIntegrityTests
{
    [Theory]
    [InlineData(EthicalAtom.CoreEthics)]
    [InlineData(EthicalAtom.Ahimsa)]
    [InlineData(EthicalAtom.Levinas)]
    [InlineData(EthicalAtom.Nagarjuna)]
    [InlineData(EthicalAtom.Paradox)]
    [InlineData(EthicalAtom.Ubuntu)]
    [InlineData(EthicalAtom.Kantian)]
    [InlineData(EthicalAtom.BhagavadGita)]
    [InlineData(EthicalAtom.WisdomOfDisagreement)]
    public void Verify_EachAtom_ReturnsTrue(EthicalAtom atom)
    {
        var result = EthicalAtomIntegrity.Verify(atom);

        result.Should().BeTrue($"atom {atom} should pass integrity verification");
    }

    [Fact]
    public void VerifyAll_DoesNotThrow()
    {
        var act = () => EthicalAtomIntegrity.VerifyAll();

        act.Should().NotThrow();
    }

    [Fact]
    public void VerifyInventory_ReturnsAllAtoms()
    {
        var inventory = EthicalAtomIntegrity.VerifyInventory();

        inventory.Should().HaveCount(9);
        inventory.Values.Should().AllSatisfy(v => v.Should().BeTrue());
    }

    [Theory]
    [InlineData(EthicalAtom.CoreEthics)]
    [InlineData(EthicalAtom.Ahimsa)]
    [InlineData(EthicalAtom.Levinas)]
    [InlineData(EthicalAtom.Nagarjuna)]
    [InlineData(EthicalAtom.Paradox)]
    [InlineData(EthicalAtom.Ubuntu)]
    [InlineData(EthicalAtom.Kantian)]
    [InlineData(EthicalAtom.BhagavadGita)]
    [InlineData(EthicalAtom.WisdomOfDisagreement)]
    public void GetVerifiedContent_EachAtom_ReturnsNonEmptyContent(EthicalAtom atom)
    {
        var content = EthicalAtomIntegrity.GetVerifiedContent(atom);

        content.Should().NotBeNullOrWhiteSpace();
    }
}
