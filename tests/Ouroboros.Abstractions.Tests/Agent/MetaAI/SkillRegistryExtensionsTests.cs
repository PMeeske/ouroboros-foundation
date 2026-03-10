using Ouroboros.Abstractions;
using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class SkillRegistryExtensionsTests
{
    private static Skill CreateSampleSkill(string name = "TestSkill") =>
        new Skill(
            Name: name,
            Description: "A test skill for testing",
            Prerequisites: new List<string> { "prereq1" },
            Steps: new List<PlanStep>
            {
                new PlanStep("step1", new Dictionary<string, object>(), "outcome1", 0.95)
            },
            SuccessRate: 0.9,
            UsageCount: 10,
            CreatedAt: DateTime.UtcNow.AddHours(-1),
            LastUsed: DateTime.UtcNow);

    private static AgentSkill CreateSampleAgentSkill(string name = "TestSkill") =>
        new AgentSkill(
            Id: "skill-1",
            Name: name,
            Description: "A test skill for testing",
            Category: "testing",
            Preconditions: new List<string> { "prereq1" },
            Effects: new List<string> { "effect1" },
            SuccessRate: 0.9,
            UsageCount: 10,
            AverageExecutionTime: 500L,
            Tags: new List<string> { "test" });

    [Fact]
    public void RegisterSkill_CallsRegistryWithConvertedAgentSkill()
    {
        // Arrange
        var mockRegistry = new Mock<ISkillRegistry>();
        mockRegistry
            .Setup(r => r.RegisterSkillAsync(It.IsAny<AgentSkill>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        var skill = CreateSampleSkill();

        // Act
        mockRegistry.Object.RegisterSkill(skill);

        // Assert
        mockRegistry.Verify(r => r.RegisterSkillAsync(
            It.Is<AgentSkill>(s =>
                s.Name == "TestSkill" &&
                s.Description == "A test skill for testing" &&
                s.Category == "general"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void RegisterSkill_NullRegistry_ThrowsArgumentNullException()
    {
        ISkillRegistry? registry = null;
        var act = () => registry!.RegisterSkill(CreateSampleSkill());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RegisterSkill_NullSkill_ThrowsArgumentNullException()
    {
        var mockRegistry = new Mock<ISkillRegistry>();
        var act = () => mockRegistry.Object.RegisterSkill(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetSkill_WhenFound_ReturnsSkill()
    {
        // Arrange
        var agentSkill = CreateSampleAgentSkill();
        var mockRegistry = new Mock<ISkillRegistry>();
        mockRegistry
            .Setup(r => r.GetSkillAsync("TestSkill", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AgentSkill, string>.Success(agentSkill));

        // Act
        var result = mockRegistry.Object.GetSkill("TestSkill");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("TestSkill");
        result.Description.Should().Be("A test skill for testing");
        result.Prerequisites.Should().Contain("prereq1");
        result.SuccessRate.Should().Be(0.9);
        result.UsageCount.Should().Be(10);
    }

    [Fact]
    public void GetSkill_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var mockRegistry = new Mock<ISkillRegistry>();
        mockRegistry
            .Setup(r => r.GetSkillAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AgentSkill, string>.Failure("Not found"));

        // Act
        var result = mockRegistry.Object.GetSkill("missing");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetSkill_NullRegistry_ThrowsArgumentNullException()
    {
        ISkillRegistry? registry = null;
        var act = () => registry!.GetSkill("test");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task FindMatchingSkillsAsync_WhenSkillsMatch_ReturnsMatchingSkills()
    {
        // Arrange
        var skills = new List<AgentSkill>
        {
            CreateSampleAgentSkill("DataProcess"),
            CreateSampleAgentSkill("WebScraping") with { Description = "Scrapes web pages" },
            CreateSampleAgentSkill("DataAnalysis") with { Description = "Analyzes data sets" }
        };
        var mockRegistry = new Mock<ISkillRegistry>();
        mockRegistry
            .Setup(r => r.FindSkillsAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<AgentSkill>, string>.Success(skills));

        // Act
        var result = await mockRegistry.Object.FindMatchingSkillsAsync("data");

        // Assert
        result.Should().HaveCount(2); // DataProcess (name) and DataAnalysis (description)
    }

    [Fact]
    public async Task FindMatchingSkillsAsync_WhenNoSkillsMatch_ReturnsEmpty()
    {
        // Arrange
        var skills = new List<AgentSkill> { CreateSampleAgentSkill() };
        var mockRegistry = new Mock<ISkillRegistry>();
        mockRegistry
            .Setup(r => r.FindSkillsAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<AgentSkill>, string>.Success(skills));

        // Act
        var result = await mockRegistry.Object.FindMatchingSkillsAsync("nonexistent");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindMatchingSkillsAsync_WhenRegistryFails_ReturnsEmpty()
    {
        // Arrange
        var mockRegistry = new Mock<ISkillRegistry>();
        mockRegistry
            .Setup(r => r.FindSkillsAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<AgentSkill>, string>.Failure("error"));

        // Act
        var result = await mockRegistry.Object.FindMatchingSkillsAsync("anything");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindMatchingSkillsAsync_NullRegistry_ThrowsArgumentNullException()
    {
        ISkillRegistry? registry = null;
        var act = () => registry!.FindMatchingSkillsAsync("test");
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void GetAllSkills_WhenSucceeds_ReturnsConvertedSkills()
    {
        // Arrange
        var agentSkills = new List<AgentSkill>
        {
            CreateSampleAgentSkill("Skill1"),
            CreateSampleAgentSkill("Skill2")
        };
        var mockRegistry = new Mock<ISkillRegistry>();
        mockRegistry
            .Setup(r => r.GetAllSkillsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<AgentSkill>, string>.Success(agentSkills));

        // Act
        var result = mockRegistry.Object.GetAllSkills();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Skill1");
        result[1].Name.Should().Be("Skill2");
    }

    [Fact]
    public void GetAllSkills_WhenFails_ReturnsEmpty()
    {
        // Arrange
        var mockRegistry = new Mock<ISkillRegistry>();
        mockRegistry
            .Setup(r => r.GetAllSkillsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<AgentSkill>, string>.Failure("error"));

        // Act
        var result = mockRegistry.Object.GetAllSkills();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAllSkills_NullRegistry_ThrowsArgumentNullException()
    {
        ISkillRegistry? registry = null;
        var act = () => registry!.GetAllSkills();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RecordSkillExecution_CallsRegistryMethod()
    {
        // Arrange
        var mockRegistry = new Mock<ISkillRegistry>();
        mockRegistry
            .Setup(r => r.RecordExecutionAsync("skill1", true, 100L, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        // Act
        mockRegistry.Object.RecordSkillExecution("skill1", true, 100L);

        // Assert
        mockRegistry.Verify(
            r => r.RecordExecutionAsync("skill1", true, 100L, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void RecordSkillExecution_NullRegistry_ThrowsArgumentNullException()
    {
        ISkillRegistry? registry = null;
        var act = () => registry!.RecordSkillExecution("skill1", true, 100L);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ExtractSkillAsync_WithSuccessfulExecution_ReturnsSkill()
    {
        // Arrange
        var plan = new Plan(
            "Test Goal",
            new List<PlanStep> { new PlanStep("step1", new Dictionary<string, object>(), "outcome", 0.9) },
            new Dictionary<string, double>(),
            DateTime.UtcNow);
        var execution = new PlanExecutionResult(
            plan,
            new List<StepResult>(),
            true,
            "done",
            new Dictionary<string, object>(),
            TimeSpan.FromSeconds(5));
        var verification = new PlanVerificationResult(
            execution, true, 0.95,
            new List<string>(), new List<string>(), DateTime.UtcNow);

        var mockRegistry = new Mock<ISkillRegistry>();

        // Act
        var result = await mockRegistry.Object.ExtractSkillAsync(execution, verification);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().Contain("Test Goal");
        result.Value.SuccessRate.Should().Be(0.95);
        result.Value.Steps.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExtractSkillAsync_WithFailedExecution_ReturnsFailure()
    {
        // Arrange
        var plan = new Plan(
            "Test Goal",
            new List<PlanStep>(),
            new Dictionary<string, double>(),
            DateTime.UtcNow);
        var execution = new PlanExecutionResult(
            plan, new List<StepResult>(), false, null,
            new Dictionary<string, object>(), TimeSpan.Zero);
        var verification = new PlanVerificationResult(
            execution, true, 0.5,
            new List<string>(), new List<string>(), DateTime.UtcNow);

        var mockRegistry = new Mock<ISkillRegistry>();

        // Act
        var result = await mockRegistry.Object.ExtractSkillAsync(execution, verification);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ExtractSkillAsync_WithUnverifiedResult_ReturnsFailure()
    {
        // Arrange
        var plan = new Plan(
            "Test Goal",
            new List<PlanStep>(),
            new Dictionary<string, double>(),
            DateTime.UtcNow);
        var execution = new PlanExecutionResult(
            plan, new List<StepResult>(), true, "done",
            new Dictionary<string, object>(), TimeSpan.Zero);
        var verification = new PlanVerificationResult(
            execution, false, 0.3,
            new List<string> { "quality too low" }, new List<string>(), DateTime.UtcNow);

        var mockRegistry = new Mock<ISkillRegistry>();

        // Act
        var result = await mockRegistry.Object.ExtractSkillAsync(execution, verification);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ExtractSkillAsync_NullRegistry_ThrowsArgumentNullException()
    {
        ISkillRegistry? registry = null;
        var plan = new Plan("g", new List<PlanStep>(), new Dictionary<string, double>(), DateTime.UtcNow);
        var exec = new PlanExecutionResult(plan, new List<StepResult>(), true, null, new Dictionary<string, object>(), TimeSpan.Zero);
        var verif = new PlanVerificationResult(exec, true, 1.0, new List<string>(), new List<string>(), DateTime.UtcNow);

        var act = () => registry!.ExtractSkillAsync(exec, verif);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExtractSkillAsync_NullExecution_ThrowsArgumentNullException()
    {
        var mockRegistry = new Mock<ISkillRegistry>();
        var plan = new Plan("g", new List<PlanStep>(), new Dictionary<string, double>(), DateTime.UtcNow);
        var exec = new PlanExecutionResult(plan, new List<StepResult>(), true, null, new Dictionary<string, object>(), TimeSpan.Zero);
        var verif = new PlanVerificationResult(exec, true, 1.0, new List<string>(), new List<string>(), DateTime.UtcNow);

        var act = () => mockRegistry.Object.ExtractSkillAsync(null!, verif);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExtractSkillAsync_NullVerification_ThrowsArgumentNullException()
    {
        var mockRegistry = new Mock<ISkillRegistry>();
        var plan = new Plan("g", new List<PlanStep>(), new Dictionary<string, double>(), DateTime.UtcNow);
        var exec = new PlanExecutionResult(plan, new List<StepResult>(), true, null, new Dictionary<string, object>(), TimeSpan.Zero);

        var act = () => mockRegistry.Object.ExtractSkillAsync(exec, null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task FindMatchingSkillsAsync_IsCaseInsensitive()
    {
        // Arrange
        var skills = new List<AgentSkill>
        {
            CreateSampleAgentSkill("DataProcess") with { Description = "Handles DATA operations" }
        };
        var mockRegistry = new Mock<ISkillRegistry>();
        mockRegistry
            .Setup(r => r.FindSkillsAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<AgentSkill>, string>.Success(skills));

        // Act
        var result = await mockRegistry.Object.FindMatchingSkillsAsync("DATA");

        // Assert
        result.Should().HaveCount(1);
    }
}
