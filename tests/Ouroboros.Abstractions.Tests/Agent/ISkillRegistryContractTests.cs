// <copyright file="ISkillRegistryContractTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Abstractions.Tests.Agent;

/// <summary>
/// Contract tests demonstrating an ISkillRegistry interface concept that could be implemented in the Ouroboros.Abstractions.Agent namespace.
/// These tests verify that such an interface can be implemented standalone
/// without Engine dependencies, using test-local mock definitions.
/// </summary>
[Trait("Category", "Unit")]
public class ISkillRegistryContractTests
{
    [Fact]
    public void SkillMetadata_CanBeInstantiated()
    {
        // Arrange & Act
        var metadata = new SkillMetadata(
            Name: "TestSkill",
            Description: "A test skill",
            Category: "Testing",
            Tags: new[] { "test", "sample" });

        // Assert
        metadata.Name.Should().Be("TestSkill");
        metadata.Description.Should().Be("A test skill");
        metadata.Category.Should().Be("Testing");
        metadata.Tags.Should().Contain("test");
    }

    [Fact]
    public async Task FakeSkillRegistry_CanBeImplemented()
    {
        // Arrange
        var registry = new FakeSkillRegistry();
        var skill = new SkillMetadata("Skill1", "Description", "Category", new[] { "tag1" });

        // Act
        await registry.RegisterSkillAsync(skill, CancellationToken.None);
        var allSkills = await registry.GetAllSkillsAsync(CancellationToken.None);
        var found = await registry.GetSkillAsync("Skill1", CancellationToken.None);

        // Assert
        allSkills.Should().NotBeEmpty();
        allSkills.Should().Contain(s => s.Name == "Skill1");
        found.Should().NotBeNull();
        found!.Name.Should().Be("Skill1");
    }

    [Fact]
    public async Task ISkillRegistry_CanBeReferencedFromAbstractionsAgent()
    {
        // This test demonstrates a skill registry interface concept that could be implemented
        // in the Ouroboros.Abstractions.Agent namespace without requiring Core or Engine dependencies
        
        // Arrange
        ISkillRegistry registry = new FakeSkillRegistry();
        var skill = new SkillMetadata("TestSkill", "Test", "Test", Array.Empty<string>());

        // Act
        await registry.RegisterSkillAsync(skill, CancellationToken.None);
        var result = await registry.GetSkillAsync("TestSkill", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void ISkillRegistry_InterfaceExists()
    {
        // Verify the test-local interface type exists (demonstrating the concept)
        var interfaceType = typeof(ISkillRegistry);
        
        interfaceType.Should().NotBeNull();
        interfaceType.IsInterface.Should().BeTrue();
    }

    [Fact]
    public async Task SkillRegistry_UnregisterSkill_ShouldWork()
    {
        // Arrange
        var registry = new FakeSkillRegistry();
        var skill = new SkillMetadata("SkillToRemove", "Description", "Category", Array.Empty<string>());

        // Act
        await registry.RegisterSkillAsync(skill, CancellationToken.None);
        var beforeUnregister = await registry.GetSkillAsync("SkillToRemove", CancellationToken.None);
        var unregistered = await registry.UnregisterSkillAsync("SkillToRemove", CancellationToken.None);
        var afterUnregister = await registry.GetSkillAsync("SkillToRemove", CancellationToken.None);

        // Assert
        beforeUnregister.Should().NotBeNull();
        unregistered.Should().BeTrue();
        afterUnregister.Should().BeNull();
    }

    [Fact]
    public async Task SkillRegistry_FindSkillsByTag_ShouldWork()
    {
        // Arrange
        var registry = new FakeSkillRegistry();
        var skill1 = new SkillMetadata("Skill1", "Test", "Cat", new[] { "tag1", "common" });
        var skill2 = new SkillMetadata("Skill2", "Test", "Cat", new[] { "tag2", "common" });
        var skill3 = new SkillMetadata("Skill3", "Test", "Cat", new[] { "tag3" });

        // Act
        await registry.RegisterSkillAsync(skill1, CancellationToken.None);
        await registry.RegisterSkillAsync(skill2, CancellationToken.None);
        await registry.RegisterSkillAsync(skill3, CancellationToken.None);
        var commonSkills = await registry.FindSkillsByTagAsync("common", CancellationToken.None);

        // Assert
        commonSkills.Should().HaveCount(2);
        commonSkills.Should().Contain(s => s.Name == "Skill1");
        commonSkills.Should().Contain(s => s.Name == "Skill2");
    }
}

/// <summary>
/// Metadata about a skill.
/// </summary>
/// <param name="Name">The skill name.</param>
/// <param name="Description">The skill description.</param>
/// <param name="Category">The skill category.</param>
/// <param name="Tags">Tags associated with the skill.</param>
public sealed record SkillMetadata(
    string Name,
    string Description,
    string Category,
    IReadOnlyList<string> Tags);

/// <summary>
/// Interface for managing skills.
/// </summary>
public interface ISkillRegistry
{
    /// <summary>
    /// Registers a skill.
    /// </summary>
    /// <param name="skill">The skill metadata.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the operation.</returns>
    Task RegisterSkillAsync(SkillMetadata skill, CancellationToken cancellationToken);

    /// <summary>
    /// Unregisters a skill.
    /// </summary>
    /// <param name="skillName">The skill name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the skill was unregistered, false otherwise.</returns>
    Task<bool> UnregisterSkillAsync(string skillName, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a skill by name.
    /// </summary>
    /// <param name="skillName">The skill name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The skill metadata, or null if not found.</returns>
    Task<SkillMetadata?> GetSkillAsync(string skillName, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all registered skills.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all skills.</returns>
    Task<IReadOnlyList<SkillMetadata>> GetAllSkillsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Finds skills by tag.
    /// </summary>
    /// <param name="tag">The tag to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of matching skills.</returns>
    Task<IReadOnlyList<SkillMetadata>> FindSkillsByTagAsync(string tag, CancellationToken cancellationToken);
}

/// <summary>
/// Fake implementation for testing purposes.
/// </summary>
internal sealed class FakeSkillRegistry : ISkillRegistry
{
    private readonly Dictionary<string, SkillMetadata> _skills = new();

    public Task RegisterSkillAsync(SkillMetadata skill, CancellationToken cancellationToken)
    {
        _skills[skill.Name] = skill;
        return Task.CompletedTask;
    }

    public Task<bool> UnregisterSkillAsync(string skillName, CancellationToken cancellationToken)
    {
        return Task.FromResult(_skills.Remove(skillName));
    }

    public Task<SkillMetadata?> GetSkillAsync(string skillName, CancellationToken cancellationToken)
    {
        _skills.TryGetValue(skillName, out var skill);
        return Task.FromResult(skill);
    }

    public Task<IReadOnlyList<SkillMetadata>> GetAllSkillsAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<SkillMetadata>>(_skills.Values.ToList());
    }

    public Task<IReadOnlyList<SkillMetadata>> FindSkillsByTagAsync(string tag, CancellationToken cancellationToken)
    {
        var matches = _skills.Values
            .Where(s => s.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            .ToList();
        return Task.FromResult<IReadOnlyList<SkillMetadata>>(matches);
    }
}
