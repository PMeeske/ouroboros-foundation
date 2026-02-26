namespace Ouroboros.Tests.Domain.Persistence;

using Ouroboros.Domain.Persistence;

[Trait("Category", "Unit")]
public class ConcurrencyExceptionTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Act
        var ex = new ConcurrencyException("branch-1", 5, 3);

        // Assert
        ex.BranchId.Should().Be("branch-1");
        ex.ExpectedVersion.Should().Be(5);
        ex.ActualVersion.Should().Be(3);
    }

    [Fact]
    public void Constructor_SetsMessageWithDetails()
    {
        // Act
        var ex = new ConcurrencyException("main", 10, 7);

        // Assert
        ex.Message.Should().Contain("main");
        ex.Message.Should().Contain("10");
        ex.Message.Should().Contain("7");
    }

    [Fact]
    public void InheritsFromException()
    {
        // Act
        var ex = new ConcurrencyException("branch", 1, 0);

        // Assert
        ex.Should().BeAssignableTo<Exception>();
    }
}
