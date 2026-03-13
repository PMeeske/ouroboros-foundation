// <copyright file="SafeToolExecutorTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.LawsOfForm;
using ExecutionContext = Ouroboros.Core.LawsOfForm.ExecutionContext;
using LoF = Ouroboros.Core.LawsOfForm.Form;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for <see cref="SafeToolExecutor"/> which executes tools with safety gates
/// and full audit trails.
/// </summary>
[Trait("Category", "Unit")]
public class SafeToolExecutorTests
{
    private readonly Mock<IToolLookup> mockToolLookup;
    private readonly Mock<IRateLimiter> mockRateLimiter;
    private readonly Mock<IContentFilter> mockContentFilter;
    private readonly ExecutionContext testContext;

    public SafeToolExecutorTests()
    {
        this.mockToolLookup = new Mock<IToolLookup>();
        this.mockRateLimiter = new Mock<IRateLimiter>();
        this.mockContentFilter = new Mock<IContentFilter>();
        this.mockRateLimiter.Setup(r => r.IsAllowed(It.IsAny<ToolCall>())).Returns(true);

        var user = new UserInfo("testUser", new HashSet<string> { "tool.execute" });
        this.testContext = new ExecutionContext(user, this.mockRateLimiter.Object, this.mockContentFilter.Object);
    }

    private static ToolCall CreateToolCall(string name = "testTool", string args = "{}")
    {
        return new ToolCall(name, args);
    }

    // ──────────── Constructor ────────────

    [Fact]
    public void Constructor_NullToolLookup_ThrowsArgumentNullException()
    {
        Action act = () => new SafeToolExecutor(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("toolLookup");
    }

    [Fact]
    public void Constructor_ValidToolLookup_CreatesInstance()
    {
        var executor = new SafeToolExecutor(this.mockToolLookup.Object);

        executor.Should().NotBeNull();
    }

    // ──────────── AddCriterion ────────────

    [Fact]
    public void AddCriterion_NullName_ThrowsArgumentNullException()
    {
        var executor = new SafeToolExecutor(this.mockToolLookup.Object);

        Action act = () => executor.AddCriterion(null!, (call, ctx) => LoF.Mark);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCriterion_NullCriterion_ThrowsArgumentNullException()
    {
        var executor = new SafeToolExecutor(this.mockToolLookup.Object);

        Action act = () => executor.AddCriterion("test", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddCriterion_ReturnsSameExecutorForFluent()
    {
        var executor = new SafeToolExecutor(this.mockToolLookup.Object);

        var result = executor.AddCriterion("test", (call, ctx) => LoF.Mark);

        result.Should().BeSameAs(executor);
    }

    // ──────────── OnUncertain ────────────

    [Fact]
    public void OnUncertain_SimpleHandler_ReturnsSameExecutorForFluent()
    {
        var executor = new SafeToolExecutor(this.mockToolLookup.Object);

        var result = executor.OnUncertain(call => Task.FromResult(true));

        result.Should().BeSameAs(executor);
    }

    [Fact]
    public void OnUncertain_ContextHandler_ReturnsSameExecutorForFluent()
    {
        var executor = new SafeToolExecutor(this.mockToolLookup.Object);

        var result = executor.OnUncertain((call, ctx) => Task.FromResult(true));

        result.Should().BeSameAs(executor);
    }

    // ──────────── ExecuteWithAudit ────────────

    [Fact]
    public async Task ExecuteWithAudit_NullToolCall_ThrowsArgumentNullException()
    {
        var executor = new SafeToolExecutor(this.mockToolLookup.Object);

        Func<Task> act = () => executor.ExecuteWithAudit(null!, this.testContext);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteWithAudit_NullContext_ThrowsArgumentNullException()
    {
        var executor = new SafeToolExecutor(this.mockToolLookup.Object);
        var toolCall = CreateToolCall();

        Func<Task> act = () => executor.ExecuteWithAudit(toolCall, null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteWithAudit_AllCriteriaMark_ExecutesTool()
    {
        var mockTool = new Mock<IToolExecutor>();
        mockTool.Setup(t => t.InvokeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("output"));

        this.mockToolLookup.Setup(l => l.GetTool("testTool"))
            .Returns(Option<IToolExecutor>.Some(mockTool.Object));

        var executor = new SafeToolExecutor(this.mockToolLookup.Object)
            .AddCriterion("safety", (call, ctx) => LoF.Mark)
            .AddCriterion("auth", (call, ctx) => LoF.Mark);

        var toolCall = CreateToolCall();
        var result = await executor.ExecuteWithAudit(toolCall, this.testContext);

        result.State.Should().Be(LoF.Mark);
        result.Value.Should().NotBeNull();
        result.Value!.Output.Should().Be("output");
    }

    [Fact]
    public async Task ExecuteWithAudit_OneCriterionVoid_RejectsExecution()
    {
        var executor = new SafeToolExecutor(this.mockToolLookup.Object)
            .AddCriterion("safety", (call, ctx) => LoF.Mark)
            .AddCriterion("auth", (call, ctx) => LoF.Void);

        var toolCall = CreateToolCall();
        var result = await executor.ExecuteWithAudit(toolCall, this.testContext);

        result.State.Should().Be(LoF.Void);
        result.Reasoning.Should().Contain("auth");
    }

    [Fact]
    public async Task ExecuteWithAudit_CriterionThrowsException_TreatedAsUncertain()
    {
        var executor = new SafeToolExecutor(this.mockToolLookup.Object)
            .AddCriterion("throwing", (call, ctx) => throw new InvalidOperationException("Boom"));

        var toolCall = CreateToolCall();
        var result = await executor.ExecuteWithAudit(toolCall, this.testContext);

        // Exception is treated as Imaginary, and no uncertainty handler -> uncertain
        result.State.Should().Be(LoF.Imaginary);
    }

    [Fact]
    public async Task ExecuteWithAudit_UncertainWithNoHandler_ReturnsUncertain()
    {
        var executor = new SafeToolExecutor(this.mockToolLookup.Object)
            .AddCriterion("uncertain", (call, ctx) => LoF.Imaginary);

        var toolCall = CreateToolCall();
        var result = await executor.ExecuteWithAudit(toolCall, this.testContext);

        result.State.Should().Be(LoF.Imaginary);
        result.Reasoning.Should().Contain("uncertain");
    }

    [Fact]
    public async Task ExecuteWithAudit_UncertainWithApprovalHandler_ExecutesTool()
    {
        var mockTool = new Mock<IToolExecutor>();
        mockTool.Setup(t => t.InvokeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("approved-output"));

        this.mockToolLookup.Setup(l => l.GetTool("testTool"))
            .Returns(Option<IToolExecutor>.Some(mockTool.Object));

        var executor = new SafeToolExecutor(this.mockToolLookup.Object)
            .AddCriterion("uncertain", (call, ctx) => LoF.Imaginary)
            .OnUncertain(call => Task.FromResult(true));

        var toolCall = CreateToolCall();
        var result = await executor.ExecuteWithAudit(toolCall, this.testContext);

        result.State.Should().Be(LoF.Mark);
        result.Value!.Output.Should().Be("approved-output");
    }

    [Fact]
    public async Task ExecuteWithAudit_UncertainWithRejectionHandler_RejectsExecution()
    {
        var executor = new SafeToolExecutor(this.mockToolLookup.Object)
            .AddCriterion("uncertain", (call, ctx) => LoF.Imaginary)
            .OnUncertain(call => Task.FromResult(false));

        var toolCall = CreateToolCall();
        var result = await executor.ExecuteWithAudit(toolCall, this.testContext);

        result.State.Should().Be(LoF.Void);
        result.Reasoning.Should().Contain("Human review declined");
    }

    [Fact]
    public async Task ExecuteWithAudit_UncertaintyHandlerThrows_ReturnsUncertain()
    {
        var executor = new SafeToolExecutor(this.mockToolLookup.Object)
            .AddCriterion("uncertain", (call, ctx) => LoF.Imaginary)
            .OnUncertain(call => throw new Exception("Handler failed"));

        var toolCall = CreateToolCall();
        var result = await executor.ExecuteWithAudit(toolCall, this.testContext);

        result.State.Should().Be(LoF.Imaginary);
    }

    [Fact]
    public async Task ExecuteWithAudit_ToolNotFound_RejectsExecution()
    {
        this.mockToolLookup.Setup(l => l.GetTool("missing"))
            .Returns(Option<IToolExecutor>.None());

        var executor = new SafeToolExecutor(this.mockToolLookup.Object)
            .AddCriterion("safety", (call, ctx) => LoF.Mark);

        var toolCall = CreateToolCall(name: "missing");
        var result = await executor.ExecuteWithAudit(toolCall, this.testContext);

        result.State.Should().Be(LoF.Void);
        result.Reasoning.Should().Contain("not found");
    }

    [Fact]
    public async Task ExecuteWithAudit_ToolExecutionThrows_RejectsExecution()
    {
        var mockTool = new Mock<IToolExecutor>();
        mockTool.Setup(t => t.InvokeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Tool crash"));

        this.mockToolLookup.Setup(l => l.GetTool("testTool"))
            .Returns(Option<IToolExecutor>.Some(mockTool.Object));

        var executor = new SafeToolExecutor(this.mockToolLookup.Object)
            .AddCriterion("safety", (call, ctx) => LoF.Mark);

        var toolCall = CreateToolCall();
        var result = await executor.ExecuteWithAudit(toolCall, this.testContext);

        result.State.Should().Be(LoF.Void);
        result.Reasoning.Should().Contain("Tool execution failed");
    }

    [Fact]
    public async Task ExecuteWithAudit_NoCriteria_ExecutesTool()
    {
        var mockTool = new Mock<IToolExecutor>();
        mockTool.Setup(t => t.InvokeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("output"));

        this.mockToolLookup.Setup(l => l.GetTool("testTool"))
            .Returns(Option<IToolExecutor>.Some(mockTool.Object));

        var executor = new SafeToolExecutor(this.mockToolLookup.Object);

        var toolCall = CreateToolCall();
        var result = await executor.ExecuteWithAudit(toolCall, this.testContext);

        // FormExtensions.All with empty array returns Mark
        result.State.Should().Be(LoF.Mark);
    }

    [Fact]
    public async Task ExecuteWithAudit_RecordsExecutionInRateLimiter()
    {
        var mockTool = new Mock<IToolExecutor>();
        mockTool.Setup(t => t.InvokeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("output"));

        this.mockToolLookup.Setup(l => l.GetTool("testTool"))
            .Returns(Option<IToolExecutor>.Some(mockTool.Object));

        var executor = new SafeToolExecutor(this.mockToolLookup.Object)
            .AddCriterion("safety", (call, ctx) => LoF.Mark);

        var toolCall = CreateToolCall();
        await executor.ExecuteWithAudit(toolCall, this.testContext);

        this.mockRateLimiter.Verify(r => r.Record(toolCall), Times.Once);
    }

    [Fact]
    public async Task ExecuteWithAudit_CollectsEvidenceFromAllCriteria()
    {
        var mockTool = new Mock<IToolExecutor>();
        mockTool.Setup(t => t.InvokeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("output"));

        this.mockToolLookup.Setup(l => l.GetTool("testTool"))
            .Returns(Option<IToolExecutor>.Some(mockTool.Object));

        var executor = new SafeToolExecutor(this.mockToolLookup.Object)
            .AddCriterion("crit1", (call, ctx) => LoF.Mark)
            .AddCriterion("crit2", (call, ctx) => LoF.Mark);

        var toolCall = CreateToolCall();
        var result = await executor.ExecuteWithAudit(toolCall, this.testContext);

        result.Evidence.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task ExecuteWithAudit_ToolReturnsFailure_StillApproved()
    {
        var mockTool = new Mock<IToolExecutor>();
        mockTool.Setup(t => t.InvokeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Failure("tool error"));

        this.mockToolLookup.Setup(l => l.GetTool("testTool"))
            .Returns(Option<IToolExecutor>.Some(mockTool.Object));

        var executor = new SafeToolExecutor(this.mockToolLookup.Object)
            .AddCriterion("safety", (call, ctx) => LoF.Mark);

        var toolCall = CreateToolCall();
        var result = await executor.ExecuteWithAudit(toolCall, this.testContext);

        // Safety criteria passed, tool executed (even if result was failure)
        result.State.Should().Be(LoF.Mark);
        result.Value!.Status.Should().Be(ExecutionStatus.Failed);
    }

    [Fact]
    public async Task ExecuteWithAudit_OnUncertainWithContext_PassesContext()
    {
        ExecutionContext? capturedContext = null;

        var mockTool = new Mock<IToolExecutor>();
        mockTool.Setup(t => t.InvokeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("output"));

        this.mockToolLookup.Setup(l => l.GetTool("testTool"))
            .Returns(Option<IToolExecutor>.Some(mockTool.Object));

        var executor = new SafeToolExecutor(this.mockToolLookup.Object)
            .AddCriterion("uncertain", (call, ctx) => LoF.Imaginary)
            .OnUncertain((call, ctx) =>
            {
                capturedContext = ctx;
                return Task.FromResult(true);
            });

        var toolCall = CreateToolCall();
        await executor.ExecuteWithAudit(toolCall, this.testContext);

        capturedContext.Should().BeSameAs(this.testContext);
    }
}
