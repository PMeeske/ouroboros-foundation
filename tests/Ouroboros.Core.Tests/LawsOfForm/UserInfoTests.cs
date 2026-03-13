using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class UserInfoTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var permissions = new HashSet<string> { "read", "write" };
        var sut = new UserInfo("user-1", permissions);

        sut.UserId.Should().Be("user-1");
        sut.Permissions.Should().HaveCount(2);
    }

    [Fact]
    public void HasPermission_ExistingPermission_ReturnsTrue()
    {
        var sut = new UserInfo("user-1", new HashSet<string> { "admin", "read" });

        sut.HasPermission("admin").Should().BeTrue();
    }

    [Fact]
    public void HasPermission_MissingPermission_ReturnsFalse()
    {
        var sut = new UserInfo("user-1", new HashSet<string> { "read" });

        sut.HasPermission("admin").Should().BeFalse();
    }

    [Fact]
    public void HasPermission_EmptyPermissions_ReturnsFalse()
    {
        var sut = new UserInfo("user-1", new HashSet<string>());

        sut.HasPermission("anything").Should().BeFalse();
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var perms = new HashSet<string> { "read" };
        var a = new UserInfo("user-1", perms);
        var b = new UserInfo("user-1", perms);

        a.Should().Be(b);
    }
}
