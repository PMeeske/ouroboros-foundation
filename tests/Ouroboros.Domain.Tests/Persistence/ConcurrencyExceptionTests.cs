using Ouroboros.Domain.Persistence;

namespace Ouroboros.Tests.Persistence;

[Trait("Category", "Unit")]
public class ConcurrencyExceptionTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var ex = new ConcurrencyException("main", 5, 7);

        ex.BranchId.Should().Be("main");
        ex.ExpectedVersion.Should().Be(5);
        ex.ActualVersion.Should().Be(7);
    }

    [Fact]
    public void Message_ContainsBranchAndVersions()
    {
        var ex = new ConcurrencyException("feature-branch", 3, 10);

        ex.Message.Should().Contain("feature-branch");
        ex.Message.Should().Contain("3");
        ex.Message.Should().Contain("10");
    }

    [Fact]
    public void IsException_InheritFromException()
    {
        var ex = new ConcurrencyException("main", 0, 1);

        ex.Should().BeAssignableTo<Exception>();
    }

    [Theory]
    [InlineData("main", 0, 1)]
    [InlineData("develop", -1, 0)]
    [InlineData("feature/test", 100, 200)]
    public void Construction_VariousInputs_SetsCorrectly(string branchId, long expected, long actual)
    {
        var ex = new ConcurrencyException(branchId, expected, actual);

        ex.BranchId.Should().Be(branchId);
        ex.ExpectedVersion.Should().Be(expected);
        ex.ActualVersion.Should().Be(actual);
    }

    [Fact]
    public void CanBeCaughtAsException()
    {
        Action act = () => throw new ConcurrencyException("main", 0, 1);

        act.Should().Throw<ConcurrencyException>()
            .Which.BranchId.Should().Be("main");
    }
}
