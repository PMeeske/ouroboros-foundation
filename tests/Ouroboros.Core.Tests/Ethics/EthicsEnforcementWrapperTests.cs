using Ouroboros.Core.Ethics;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class EthicsEnforcementWrapperTests
{
    private readonly Mock<IActionExecutor<string, int>> _mockExecutor;
    private readonly Mock<IEthicsFramework> _mockFramework;
    private readonly ActionContext _context;
    private readonly Func<string, ProposedAction> _converter;

    public EthicsEnforcementWrapperTests()
    {
        _mockExecutor = new Mock<IActionExecutor<string, int>>();
        _mockFramework = new Mock<IEthicsFramework>();
        _context = new ActionContext
        {
            AgentId = "test-agent",
            Environment = "testing",
            State = new Dictionary<string, object>()
        };
        _converter = action => new ProposedAction
        {
            ActionType = "test",
            Description = action,
            Parameters = new Dictionary<string, object>(),
            PotentialEffects = new List<string>()
        };
    }

    private EthicsEnforcementWrapper<string, int> CreateSut()
    {
        return new EthicsEnforcementWrapper<string, int>(
            _mockExecutor.Object, _mockFramework.Object, _converter, _context);
    }

    [Fact]
    public void Constructor_NullInnerExecutor_ThrowsArgumentNullException()
    {
        var act = () => new EthicsEnforcementWrapper<string, int>(
            null!, _mockFramework.Object, _converter, _context);

        act.Should().Throw<ArgumentNullException>().WithParameterName("innerExecutor");
    }

    [Fact]
    public void Constructor_NullEthicsFramework_ThrowsArgumentNullException()
    {
        var act = () => new EthicsEnforcementWrapper<string, int>(
            _mockExecutor.Object, null!, _converter, _context);

        act.Should().Throw<ArgumentNullException>().WithParameterName("ethicsFramework");
    }

    [Fact]
    public void Constructor_NullActionConverter_ThrowsArgumentNullException()
    {
        var act = () => new EthicsEnforcementWrapper<string, int>(
            _mockExecutor.Object, _mockFramework.Object, null!, _context);

        act.Should().Throw<ArgumentNullException>().WithParameterName("actionConverter");
    }

    [Fact]
    public void Constructor_NullContext_ThrowsArgumentNullException()
    {
        var act = () => new EthicsEnforcementWrapper<string, int>(
            _mockExecutor.Object, _mockFramework.Object, _converter, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("context");
    }

    [Fact]
    public async Task ExecuteAsync_NullAction_ThrowsArgumentNullException()
    {
        var sut = CreateSut();

        var act = () => sut.ExecuteAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("action");
    }

    [Fact]
    public async Task ExecuteAsync_PermittedAction_DelegatesToInnerExecutor()
    {
        var clearance = EthicalClearance.Permitted("Safe action");
        _mockFramework
            .Setup(f => f.EvaluateActionAsync(It.IsAny<ProposedAction>(), _context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EthicalClearance, string>.Success(clearance));
        _mockExecutor
            .Setup(e => e.ExecuteAsync("test-action", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int, string>.Success(42));

        var sut = CreateSut();
        var result = await sut.ExecuteAsync("test-action");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        _mockExecutor.Verify(e => e.ExecuteAsync("test-action", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DeniedAction_ReturnsFailureWithoutExecuting()
    {
        var violations = new List<EthicalViolation>
        {
            new() { ViolatedPrinciple = EthicalPrinciple.DoNoHarm, Description = "Harmful", Severity = ViolationSeverity.High, Evidence = "test", AffectedParties = new List<string> { "Users" } }
        };
        var clearance = EthicalClearance.Denied("Action is harmful", violations);
        _mockFramework
            .Setup(f => f.EvaluateActionAsync(It.IsAny<ProposedAction>(), _context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EthicalClearance, string>.Success(clearance));

        var sut = CreateSut();
        var result = await sut.ExecuteAsync("harmful-action");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("ethical violations");
        _mockExecutor.Verify(e => e.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_RequiresApproval_ReturnsFailureWithoutExecuting()
    {
        var clearance = EthicalClearance.RequiresApproval("Needs human review");
        _mockFramework
            .Setup(f => f.EvaluateActionAsync(It.IsAny<ProposedAction>(), _context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EthicalClearance, string>.Success(clearance));

        var sut = CreateSut();
        var result = await sut.ExecuteAsync("risky-action");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("human approval");
        _mockExecutor.Verify(e => e.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_EvaluationFailure_ReturnsFailure()
    {
        _mockFramework
            .Setup(f => f.EvaluateActionAsync(It.IsAny<ProposedAction>(), _context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EthicalClearance, string>.Failure("Framework error"));

        var sut = CreateSut();
        var result = await sut.ExecuteAsync("action");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Ethics evaluation failed");
    }

    [Fact]
    public async Task ExecuteAsync_InnerExecutorThrows_ReturnsFailure()
    {
        var clearance = EthicalClearance.Permitted("OK");
        _mockFramework
            .Setup(f => f.EvaluateActionAsync(It.IsAny<ProposedAction>(), _context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EthicalClearance, string>.Success(clearance));
        _mockExecutor
            .Setup(e => e.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Internal error"));

        var sut = CreateSut();
        var result = await sut.ExecuteAsync("action");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Execution failed");
    }

    [Fact]
    public async Task ExecuteAsync_PermittedWithConcerns_ExecutesSuccessfully()
    {
        var concerns = new List<EthicalConcern>
        {
            new() { RelatedPrinciple = EthicalPrinciple.Transparency, Description = "Low transparency", Level = ConcernLevel.Low, RecommendedAction = "Provide more details" }
        };
        var clearance = new EthicalClearance
        {
            IsPermitted = true,
            Level = EthicalClearanceLevel.PermittedWithConcerns,
            RelevantPrinciples = Array.Empty<EthicalPrinciple>(),
            Violations = Array.Empty<EthicalViolation>(),
            Concerns = concerns,
            Reasoning = "Permitted but has concerns"
        };
        _mockFramework
            .Setup(f => f.EvaluateActionAsync(It.IsAny<ProposedAction>(), _context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<EthicalClearance, string>.Success(clearance));
        _mockExecutor
            .Setup(e => e.ExecuteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int, string>.Success(100));

        var sut = CreateSut();
        var result = await sut.ExecuteAsync("action");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(100);
    }
}
