// <copyright file="UserInfoTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for the <see cref="UserInfo"/> record.
/// </summary>
[Trait("Category", "Unit")]
public class UserInfoTests
{
    // --- Constructor ---

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var permissions = new HashSet<string> { "read", "write", "admin" };

        var user = new UserInfo("user-123", permissions);

        user.UserId.Should().Be("user-123");
        user.Permissions.Should().BeEquivalentTo(new[] { "read", "write", "admin" });
    }

    [Fact]
    public void Constructor_EmptyPermissions_Accepted()
    {
        var user = new UserInfo("user-1", new HashSet<string>());

        user.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_SinglePermission_Accepted()
    {
        var user = new UserInfo("user-1", new HashSet<string> { "read" });

        user.Permissions.Should().HaveCount(1);
        user.Permissions.Should().Contain("read");
    }

    // --- HasPermission ---

    [Fact]
    public void HasPermission_ExistingPermission_ReturnsTrue()
    {
        var user = new UserInfo("user-1", new HashSet<string> { "read", "write" });

        user.HasPermission("read").Should().BeTrue();
        user.HasPermission("write").Should().BeTrue();
    }

    [Fact]
    public void HasPermission_NonExistingPermission_ReturnsFalse()
    {
        var user = new UserInfo("user-1", new HashSet<string> { "read" });

        user.HasPermission("write").Should().BeFalse();
        user.HasPermission("admin").Should().BeFalse();
    }

    [Fact]
    public void HasPermission_EmptyPermissions_ReturnsFalse()
    {
        var user = new UserInfo("user-1", new HashSet<string>());

        user.HasPermission("read").Should().BeFalse();
    }

    [Fact]
    public void HasPermission_IsCaseSensitive()
    {
        var user = new UserInfo("user-1", new HashSet<string> { "Read" });

        user.HasPermission("Read").Should().BeTrue();
        user.HasPermission("read").Should().BeFalse();
        user.HasPermission("READ").Should().BeFalse();
    }

    // --- Record Equality ---

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var perms = new HashSet<string> { "read" };

        var u1 = new UserInfo("user-1", perms);
        var u2 = new UserInfo("user-1", perms);

        u1.Should().Be(u2);
    }

    [Fact]
    public void RecordEquality_DifferentUserId_AreNotEqual()
    {
        var perms = new HashSet<string> { "read" };

        var u1 = new UserInfo("user-1", perms);
        var u2 = new UserInfo("user-2", perms);

        u1.Should().NotBe(u2);
    }

    [Fact]
    public void RecordEquality_DifferentPermissions_AreNotEqual()
    {
        var u1 = new UserInfo("user-1", new HashSet<string> { "read" });
        var u2 = new UserInfo("user-1", new HashSet<string> { "write" });

        u1.Should().NotBe(u2);
    }

    // --- With expression (record) ---

    [Fact]
    public void WithExpression_CanCreateModifiedCopy()
    {
        var original = new UserInfo("user-1", new HashSet<string> { "read" });

        var modified = original with { UserId = "user-2" };

        modified.UserId.Should().Be("user-2");
        modified.Permissions.Should().Contain("read");
        original.UserId.Should().Be("user-1");
    }

    // --- UserId edge cases ---

    [Fact]
    public void Constructor_EmptyUserId_Accepted()
    {
        var user = new UserInfo("", new HashSet<string>());

        user.UserId.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_SpecialCharactersInUserId_Accepted()
    {
        var user = new UserInfo("user@domain.com", new HashSet<string> { "read" });

        user.UserId.Should().Be("user@domain.com");
    }
}
