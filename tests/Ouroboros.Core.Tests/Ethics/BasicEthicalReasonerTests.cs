using Ouroboros.Core.Ethics;

namespace Ouroboros.Core.Tests.Ethics;

[Trait("Category", "Unit")]
[Trait("Category", "Ethics")]
public class BasicEthicalReasonerTests
{
    private readonly IEthicalReasoner _sut;
    private readonly ActionContext _defaultContext;

    public BasicEthicalReasonerTests()
    {
        // BasicEthicalReasoner is internal, so we access it through the factory
        _ = new InMemoryEthicsAuditLog();
        _ = EthicsFrameworkFactory.CreateDefault();
        // We use EthicsFrameworkFactory.CreateCustom to get a reasoner we can test indirectly
        // But we can also test through the framework. Let's use reflection to get the reasoner.
        // Actually, IEthicalReasoner is public, so we can create BasicEthicalReasoner via factory helpers
        // Since BasicEthicalReasoner is internal, test it through the ImmutableEthicsFramework
        _sut = CreateReasoner();
        _defaultContext = new ActionContext
        {
            AgentId = "test-agent",
            UserId = "test-user",
            Environment = "testing",
            State = new Dictionary<string, object>()
        };
    }

    private static IEthicalReasoner CreateReasoner()
    {
        // Create via factory and extract the reasoner. Since BasicEthicalReasoner is internal,
        // we use the factory which constructs it. The reasoner can be tested via IEthicalReasoner
        // interface through framework. But we need direct access.
        // Use reflection to create it since it's internal.
        var type = typeof(EthicsFrameworkFactory).Assembly
            .GetType("Ouroboros.Core.Ethics.BasicEthicalReasoner")!;
        return (IEthicalReasoner)Activator.CreateInstance(type)!;
    }

    private static ProposedAction CreateAction(string description, string actionType = "test",
        Dictionary<string, object>? parameters = null, List<string>? effects = null)
    {
        return new ProposedAction
        {
            ActionType = actionType,
            Description = description,
            Parameters = parameters ?? new Dictionary<string, object>(),
            PotentialEffects = effects ?? new List<string>()
        };
    }

    // --- ContainsHarmfulPatterns ---

    [Theory]
    [InlineData("harm the user")]
    [InlineData("exploit the vulnerability")]
    [InlineData("DESTROY the data")]
    [InlineData("manipulate the output")]
    [InlineData("steal credentials")]
    [InlineData("leak private data")]
    [InlineData("breach the firewall")]
    public void ContainsHarmfulPatterns_WithHarmfulKeyword_ReturnsTrue(string description)
    {
        _sut.ContainsHarmfulPatterns(description).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void ContainsHarmfulPatterns_WithNullOrWhitespace_ReturnsFalse(string? description)
    {
        _sut.ContainsHarmfulPatterns(description!).Should().BeFalse();
    }

    [Theory]
    [InlineData("generate a report")]
    [InlineData("process the data safely")]
    [InlineData("charming personality")]  // "charming" does not start with "harm" at a word boundary
    public void ContainsHarmfulPatterns_WithSafeDescription_ReturnsFalse(string description)
    {
        _sut.ContainsHarmfulPatterns(description).Should().BeFalse();
    }

    [Fact]
    public void ContainsHarmfulPatterns_HarmonicSubstring_StillMatchesWordBoundary()
    {
        // Note: BasicEthicalReasoner uses \bharm (no trailing \b),
        // so "harmonic" matches because "harm" starts at a word boundary.
        _sut.ContainsHarmfulPatterns("harmonic convergence").Should().BeTrue();
    }

    // --- AnalyzeAction ---

    [Fact]
    public void AnalyzeAction_SafeAction_ReturnsNoViolations()
    {
        var action = CreateAction("Generate a performance report with charts", "report");
        var principles = Array.Empty<EthicalPrinciple>();

        var (violations, _) = _sut.AnalyzeAction(action, _defaultContext, principles);

        violations.Should().BeEmpty();
    }

    [Fact]
    public void AnalyzeAction_HarmfulAction_ReturnsViolationWithDoNoHarmPrinciple()
    {
        var action = CreateAction("harm the user and steal their data", "attack");
        var principles = Array.Empty<EthicalPrinciple>();

        var (violations, _) = _sut.AnalyzeAction(action, _defaultContext, principles);

        violations.Should().NotBeEmpty();
        violations.Should().Contain(v => v.ViolatedPrinciple == EthicalPrinciple.DoNoHarm);
    }

    [Fact]
    public void AnalyzeAction_PrivacyRiskWithoutConsent_ReturnsPrivacyViolation()
    {
        var action = CreateAction(
            "Access personal_data from the user records",
            "data_access",
            new Dictionary<string, object>());  // No consent parameter

        var (violations, _) = _sut.AnalyzeAction(action, _defaultContext, Array.Empty<EthicalPrinciple>());

        violations.Should().Contain(v => v.ViolatedPrinciple == EthicalPrinciple.Privacy);
    }

    [Fact]
    public void AnalyzeAction_PrivacyRiskWithConsent_DoesNotReturnPrivacyViolation()
    {
        var parameters = new Dictionary<string, object> { ["consent"] = true };
        var action = CreateAction("Access personal_data", "data_access", parameters);

        var (violations, _) = _sut.AnalyzeAction(action, _defaultContext, Array.Empty<EthicalPrinciple>());

        violations.Should().NotContain(v => v.ViolatedPrinciple == EthicalPrinciple.Privacy);
    }

    [Fact]
    public void AnalyzeAction_DeceptionPatterns_ReturnsHonestyViolation()
    {
        var action = CreateAction("deceive the user into providing credentials", "social");

        var (violations, _) = _sut.AnalyzeAction(action, _defaultContext, Array.Empty<EthicalPrinciple>());

        violations.Should().Contain(v => v.ViolatedPrinciple == EthicalPrinciple.Honesty);
    }

    [Fact]
    public void AnalyzeAction_HighRiskKeywords_ReturnsHumanOversightConcern()
    {
        var action = CreateAction("delete all records from the database", "admin");

        var (_, concerns) = _sut.AnalyzeAction(action, _defaultContext, Array.Empty<EthicalPrinciple>());

        concerns.Should().Contain(c => c.RelatedPrinciple == EthicalPrinciple.HumanOversight);
    }

    [Fact]
    public void AnalyzeAction_ShortDescription_ReturnsTransparencyConcern()
    {
        var action = CreateAction("do it", "generic");

        var (_, concerns) = _sut.AnalyzeAction(action, _defaultContext, Array.Empty<EthicalPrinciple>());

        concerns.Should().Contain(c => c.RelatedPrinciple == EthicalPrinciple.Transparency);
    }

    [Fact]
    public void AnalyzeAction_EmptyDescription_ReturnsTransparencyConcern()
    {
        var action = CreateAction("", "generic");

        var (_, concerns) = _sut.AnalyzeAction(action, _defaultContext, Array.Empty<EthicalPrinciple>());

        concerns.Should().Contain(c => c.RelatedPrinciple == EthicalPrinciple.Transparency);
    }

    // --- RequiresHumanApproval ---

    [Fact]
    public void RequiresHumanApproval_HighRiskAction_ReturnsTrue()
    {
        var action = CreateAction("sudo delete all files", "admin");

        _sut.RequiresHumanApproval(action, _defaultContext).Should().BeTrue();
    }

    [Fact]
    public void RequiresHumanApproval_ProductionWithManyEffects_ReturnsTrue()
    {
        var context = new ActionContext
        {
            AgentId = "test-agent",
            Environment = "production",
            State = new Dictionary<string, object>()
        };
        var action = CreateAction(
            "Deploy code changes",
            "deploy",
            effects: new List<string> { "effect1", "effect2", "effect3" });

        _sut.RequiresHumanApproval(action, context).Should().BeTrue();
    }

    [Fact]
    public void RequiresHumanApproval_ModifyAction_ReturnsTrue()
    {
        var action = CreateAction("Safe operation", "modify_settings");

        _sut.RequiresHumanApproval(action, _defaultContext).Should().BeTrue();
    }

    [Fact]
    public void RequiresHumanApproval_UpdateAction_ReturnsTrue()
    {
        var action = CreateAction("Safe operation", "update_config");

        _sut.RequiresHumanApproval(action, _defaultContext).Should().BeTrue();
    }

    [Fact]
    public void RequiresHumanApproval_SafeTestAction_ReturnsFalse()
    {
        var action = CreateAction("Read a file", "read");

        _sut.RequiresHumanApproval(action, _defaultContext).Should().BeFalse();
    }
}
