// <copyright file="ExecutionContextTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;
using ExecutionContext = Ouroboros.Core.LawsOfForm.ExecutionContext;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for the <see cref="ExecutionContext"/> record.
/// </summary>
[Trait("Category", "Unit")]
public class ExecutionContextTests
{
    private static UserInfo CreateUser() =>
        new("user1", new HashSet<string> { "read" });

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var user = CreateUser();
        var rateLimiter = new Mock<IRateLimiter>().Object;
        var contentFilter = new Mock<IContentFilter>().Object;
        var metadata = new Dictionary<string, object> { ["key"] = "value" };

        var ctx = new ExecutionContext(user, rateLimiter, contentFilter, "session-1", metadata);

        ctx.User.Should().Be(user);
        ctx.RateLimiter.Should().Be(rateLimiter);
        ctx.ContentFilter.Should().Be(contentFilter);
        ctx.SessionId.Should().Be("session-1");
        ctx.Metadata.Should().ContainKey("key");
    }

    [Fact]
    public void Constructor_DefaultSessionId_IsNull()
    {
        var ctx = new ExecutionContext(
            CreateUser(),
            new Mock<IRateLimiter>().Object,
            new Mock<IContentFilter>().Object);

        ctx.SessionId.Should().BeNull();
    }

    [Fact]
    public void Constructor_DefaultMetadata_IsEmptyDictionary()
    {
        var ctx = new ExecutionContext(
            CreateUser(),
            new Mock<IRateLimiter>().Object,
            new Mock<IContentFilter>().Object);

        ctx.Metadata.Should().BeEmpty();
    }
}

/// <summary>
/// Tests for the <see cref="ExecutionStatus"/> enum.
/// </summary>
[Trait("Category", "Unit")]
public class ExecutionStatusTests
{
    [Fact]
    public void ExecutionStatus_HasExpectedValues()
    {
        ((int)ExecutionStatus.Success).Should().Be(0);
        ((int)ExecutionStatus.Failed).Should().Be(1);
        ((int)ExecutionStatus.Blocked).Should().Be(2);
        ((int)ExecutionStatus.PendingApproval).Should().Be(3);
    }

    [Fact]
    public void ExecutionStatus_HasFourMembers()
    {
        Enum.GetValues<ExecutionStatus>().Should().HaveCount(4);
    }
}

/// <summary>
/// Tests for the <see cref="SafetyLevel"/> enum.
/// </summary>
[Trait("Category", "Unit")]
public class SafetyLevelTests
{
    [Fact]
    public void SafetyLevel_HasExpectedValues()
    {
        ((int)SafetyLevel.Safe).Should().Be(0);
        ((int)SafetyLevel.Uncertain).Should().Be(1);
        ((int)SafetyLevel.Unsafe).Should().Be(2);
    }

    [Fact]
    public void SafetyLevel_HasThreeMembers()
    {
        Enum.GetValues<SafetyLevel>().Should().HaveCount(3);
    }
}
