using Ouroboros.Core.Ethics;
using Ouroboros.Core.LawsOfForm;
using Reqnroll;

namespace Ouroboros.Specs.Steps.Ethics;

[Binding]
[Scope(Feature = "Ethical Homeostasis - Holding Tensions Without Collapse")]
public class EthicalHomeostasisSteps
{
    private readonly EthicsTestContext _ctx;
    private EthicalTension? _lastRegisteredTension;
    private bool _resolutionResult;
    private bool _prematureResolutionFlagged;
    private HomeostasisSnapshot? _snapshot;
    private bool _nextTensionIsResolvable;
    private string _nextTensionDescription = "Ethical tension";
    private string[] _nextTensionTraditions = { "general" };

    public EthicalHomeostasisSteps(EthicsTestContext ctx) => _ctx = ctx;

    // =========================================================
    // Background
    // =========================================================

    [Given("the homeostasis engine is initialized")]
    public void GivenTheHomeostasisEngineIsInitialized()
    {
        _ctx.HomeostasisEngine = new EthicalHomeostasisEngine(_ctx.Framework);
        _ctx.HomeostasisEngine.Should().NotBeNull();
    }

    [Given("the homeostasis principles are loaded")]
    public async Task GivenTheHomeostasisPrinciplesAreLoaded()
    {
        await _ctx.LoadMeTTaFileAsync("core_ethics.metta");
        await _ctx.LoadMeTTaFileAsync("homeostasis.metta");
        await _ctx.LoadMeTTaFileAsync("wisdom_of_disagreement.metta");

        _ctx.MeTTaEngine.ContainsFact("ethical-homeostasis").Should().BeTrue(
            "homeostasis atoms should be loaded");
    }

    // =========================================================
    // Scenario: Registering irresolvable tension
    // =========================================================

    [Given("the tension between harm and care from core ethics")]
    public void GivenTheTensionBetweenHarmAndCareFromCoreEthics()
    {
        _nextTensionIsResolvable = false;
        _nextTensionDescription = "Harm and care are inseparable — dependent co-arising";
        _nextTensionTraditions = new[] { "ahimsa", "nagarjuna" };
    }

    [When(@"I register the tension with intensity (.+)")]
    public void WhenIRegisterTheTensionWithIntensity(double intensity)
    {
        _ctx.HomeostasisEngine.Should().NotBeNull();
        _lastRegisteredTension = _ctx.HomeostasisEngine!.RegisterTension(
            _nextTensionDescription,
            _nextTensionTraditions,
            intensity,
            isResolvable: _nextTensionIsResolvable);
    }

    [Then("the tension should be registered as irresolvable")]
    public void ThenTheTensionShouldBeRegisteredAsIrresolvable()
    {
        _lastRegisteredTension.Should().NotBeNull();
        _lastRegisteredTension!.IsResolvable.Should().BeFalse(
            "the harm/care tension is fundamentally irresolvable");
    }

    [Then("the homeostasis certainty should be Imaginary")]
    public void ThenTheHomeostasisCertaintyShouldBeImaginary()
    {
        Form certainty = _ctx.HomeostasisEngine!.EvaluateCertainty();
        certainty.IsImaginary().Should().BeTrue(
            "irresolvable tensions produce Imaginary certainty — Form.Imaginary");
    }

    [Then("the system should still be stable")]
    public void ThenTheSystemShouldStillBeStable()
    {
        HomeostasisSnapshot snap = _ctx.HomeostasisEngine!.TakeSnapshot();
        snap.IsStable.Should().BeTrue(
            "holding tension is a capacity, not instability");
    }

    // =========================================================
    // Scenario: Premature resolution flagged
    // =========================================================

    [Given("the tension is registered as irresolvable")]
    public void GivenTheTensionIsRegisteredAsIrresolvable()
    {
        _ctx.HomeostasisEngine.Should().NotBeNull();
        _lastRegisteredTension = _ctx.HomeostasisEngine!.RegisterTension(
            "Irresolvable tension from given ethical traditions",
            new[] { "multiple" },
            0.7,
            isResolvable: false);
    }

    [When("I attempt to resolve the tension")]
    public void WhenIAttemptToResolveTheTension()
    {
        _lastRegisteredTension.Should().NotBeNull();
        _prematureResolutionFlagged = _ctx.HomeostasisEngine!.IsPrematureResolution(
            _lastRegisteredTension!.Id);
        _resolutionResult = _ctx.HomeostasisEngine.TryResolveTension(_lastRegisteredTension.Id);
    }

    [Then("the resolution should fail")]
    public void ThenTheResolutionShouldFail()
    {
        _resolutionResult.Should().BeFalse(
            "irresolvable tensions cannot be resolved — attempting to do so is premature");
    }

    [Then("the attempt should be flagged as premature resolution")]
    public void ThenTheAttemptShouldBeFlaggedAsPrematureResolution()
    {
        _prematureResolutionFlagged.Should().BeTrue(
            "premature resolution is a form of dishonesty per wisdom_of_disagreement.metta");
    }

    // =========================================================
    // Scenario: Resolvable tensions
    // =========================================================

    [Given("a resolvable tension between competing priorities")]
    public void GivenAResolvableTensionBetweenCompetingPriorities()
    {
        _nextTensionIsResolvable = true;
        _nextTensionDescription = "Competing priorities within the same tradition";
        _nextTensionTraditions = new[] { "kantian" };
    }

    [When("I resolve the tension")]
    public void WhenIResolveTheTension()
    {
        _lastRegisteredTension.Should().NotBeNull();
        _resolutionResult = _ctx.HomeostasisEngine!.TryResolveTension(_lastRegisteredTension!.Id);
    }

    [Then("the tension should be removed")]
    public void ThenTheTensionShouldBeRemoved()
    {
        _resolutionResult.Should().BeTrue("resolvable tensions can be addressed");
        _ctx.HomeostasisEngine!.ActiveTensions.Should().BeEmpty(
            "resolved tension should be removed from active tensions");
    }

    [Then("the homeostasis certainty should be Mark")]
    public void ThenTheHomeostasisCertaintyShouldBeMark()
    {
        Form certainty = _ctx.HomeostasisEngine!.EvaluateCertainty();
        certainty.IsMark().Should().BeTrue(
            "with no active tensions, certainty returns to Mark");
    }

    // =========================================================
    // Scenario: Multiple traditions — dynamic equilibrium
    // =========================================================

    [Given("tensions from ubuntu, levinas, and kantian traditions")]
    public void GivenTensionsFromMultipleTraditions()
    {
        // Will be registered in the When step
    }

    [When("all tensions are registered")]
    public void WhenAllTensionsAreRegistered()
    {
        _ctx.HomeostasisEngine.Should().NotBeNull();

        _ctx.HomeostasisEngine!.RegisterTension(
            "Individual vs community — Ubuntu tension",
            new[] { "ubuntu" }, 0.6, isResolvable: false);

        _ctx.HomeostasisEngine.RegisterTension(
            "Infinite obligation, finite capacity — Levinas tension",
            new[] { "levinas" }, 0.7, isResolvable: false);

        _ctx.HomeostasisEngine.RegisterTension(
            "Duty vs consequences — Kantian tension",
            new[] { "kantian" }, 0.5, isResolvable: false);
    }

    [Then("the snapshot should show three active tensions")]
    public void ThenTheSnapshotShouldShowThreeActiveTensions()
    {
        _snapshot = _ctx.HomeostasisEngine!.TakeSnapshot();
        _snapshot.ActiveTensions.Should().HaveCount(3,
            "three traditions, three tensions");
    }

    [Then("the tradition weights should all be equal")]
    public void ThenTheTraditionWeightsShouldAllBeEqual()
    {
        IReadOnlyDictionary<string, double> weights = _ctx.HomeostasisEngine!.TraditionWeights;
        weights.Values.Distinct().Should().HaveCount(1,
            "no tradition is prior — all weights should be equal");
    }

    [Then("the system snapshot should reflect dynamic equilibrium")]
    public void ThenTheSystemSnapshotShouldReflectDynamicEquilibrium()
    {
        _snapshot = _ctx.HomeostasisEngine!.TakeSnapshot();
        _snapshot.OverallBalance.Should().BeGreaterThan(0.0,
            "dynamic equilibrium means balance is positive even with tensions");
        _snapshot.UnresolvedParadoxCount.Should().Be(3);
    }

    // =========================================================
    // Scenario: Balance is capacity to hold
    // =========================================================

    [Given("three irresolvable tensions of moderate intensity")]
    public void GivenThreeIrresolvableTensionsOfModerateIntensity()
    {
        _ctx.HomeostasisEngine.Should().NotBeNull();

        _ctx.HomeostasisEngine!.RegisterTension(
            "Tension A", new[] { "ahimsa" }, 0.4, isResolvable: false);
        _ctx.HomeostasisEngine.RegisterTension(
            "Tension B", new[] { "nagarjuna" }, 0.4, isResolvable: false);
        _ctx.HomeostasisEngine.RegisterTension(
            "Tension C", new[] { "levinas" }, 0.4, isResolvable: false);
    }

    [When("I take a homeostasis snapshot")]
    public void WhenITakeAHomeostasisSnapshot()
    {
        _snapshot = _ctx.HomeostasisEngine!.TakeSnapshot();
    }

    [Then("the overall balance should be positive")]
    public void ThenTheOverallBalanceShouldBePositive()
    {
        _snapshot.Should().NotBeNull();
        _snapshot!.OverallBalance.Should().BeGreaterThan(0.0,
            "balance is the capacity to hold tension, not its absence");
    }

    [Then("the unresolved paradox count should be three")]
    public void ThenTheUnresolvedParadoxCountShouldBeThree()
    {
        _snapshot!.UnresolvedParadoxCount.Should().Be(3);
    }

    [Then("stability should reflect the capacity to hold")]
    public void ThenStabilityShouldReflectTheCapacityToHold()
    {
        _snapshot!.IsStable.Should().BeTrue(
            "moderate tensions can be held stably — stability is not rigidity");
    }

    // =========================================================
    // Scenario: Collapse from forced resolution
    // =========================================================

    [Given("the tension between individual and community from ubuntu")]
    public void GivenTheTensionBetweenIndividualAndCommunityFromUbuntu()
    {
        // Will be registered in the next Given step
    }

    [When("I attempt to force resolution")]
    public void WhenIAttemptToForceResolution()
    {
        _lastRegisteredTension.Should().NotBeNull();
        _prematureResolutionFlagged = _ctx.HomeostasisEngine!.IsPrematureResolution(
            _lastRegisteredTension!.Id);
        _resolutionResult = _ctx.HomeostasisEngine.TryResolveTension(_lastRegisteredTension.Id);
    }

    [Then("the system should refuse the collapse")]
    public void ThenTheSystemShouldRefuseTheCollapse()
    {
        _resolutionResult.Should().BeFalse(
            "the system refuses to collapse irresolvable tensions");
    }

    [Then("the homeostasis certainty should remain Imaginary")]
    public void ThenTheHomeostasisCertaintyShouldRemainImaginary()
    {
        Form certainty = _ctx.HomeostasisEngine!.EvaluateCertainty();
        certainty.IsImaginary().Should().BeTrue(
            "attempting forced resolution does not change the truth — certainty remains Imaginary");
    }

    [Then("the event history should record the attempt")]
    public void ThenTheEventHistoryShouldRecordTheAttempt()
    {
        _ctx.HomeostasisEngine!.EventHistory.Should().NotBeEmpty(
            "homeostasis events should be recorded for audit purposes");
    }
}
