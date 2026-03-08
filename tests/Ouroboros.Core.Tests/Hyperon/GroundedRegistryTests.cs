using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class GroundedRegistryTests
{
    [Fact]
    public void Register_AndGet_ReturnsOperation()
    {
        var registry = new GroundedRegistry();
        GroundedOperation op = (space, expr) => Enumerable.Empty<Atom>();

        registry.Register("test-op", op);

        var result = registry.Get("test-op");
        result.HasValue.Should().BeTrue();
    }

    [Fact]
    public void Get_NonExistentOp_ReturnsNone()
    {
        var registry = new GroundedRegistry();

        var result = registry.Get("nonexistent");

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Contains_RegisteredOp_ReturnsTrue()
    {
        var registry = new GroundedRegistry();
        registry.Register("op1", (s, e) => Enumerable.Empty<Atom>());

        registry.Contains("op1").Should().BeTrue();
    }

    [Fact]
    public void Contains_UnregisteredOp_ReturnsFalse()
    {
        var registry = new GroundedRegistry();

        registry.Contains("missing").Should().BeFalse();
    }

    [Fact]
    public void RegisteredNames_ReturnsAllRegistered()
    {
        var registry = new GroundedRegistry();
        registry.Register("op1", (s, e) => Enumerable.Empty<Atom>());
        registry.Register("op2", (s, e) => Enumerable.Empty<Atom>());

        registry.RegisteredNames.Should().Contain("op1");
        registry.RegisteredNames.Should().Contain("op2");
    }

    [Fact]
    public void CreateStandard_ContainsStandardOps()
    {
        var registry = GroundedRegistry.CreateStandard();

        registry.Contains("implies").Should().BeTrue();
        registry.Contains("equal").Should().BeTrue();
        registry.Contains("not").Should().BeTrue();
        registry.Contains("and").Should().BeTrue();
        registry.Contains("or").Should().BeTrue();
        registry.Contains("assert").Should().BeTrue();
        registry.Contains("retract").Should().BeTrue();
        registry.Contains("match").Should().BeTrue();
        registry.Contains("quote").Should().BeTrue();
    }

    [Fact]
    public void Register_OverwriteExisting_ReplacesOperation()
    {
        var registry = new GroundedRegistry();
        GroundedOperation op1 = (s, e) => new[] { Atom.Sym("v1") };
        GroundedOperation op2 = (s, e) => new[] { Atom.Sym("v2") };

        registry.Register("op", op1);
        registry.Register("op", op2);

        var result = registry.Get("op");
        result.HasValue.Should().BeTrue();
        var space = new AtomSpace();
        var expr = Atom.Expr(Atom.Sym("op"));
        result.Value!(space, expr).Should().Contain(Atom.Sym("v2"));
    }
}
