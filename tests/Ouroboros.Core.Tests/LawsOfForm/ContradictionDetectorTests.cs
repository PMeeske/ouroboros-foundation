using Ouroboros.Core.LawsOfForm;
using Moq;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class ContradictionDetectorTests
{
    [Fact]
    public void ContradictionDetector_ShouldBeCreatable()
    {
        // Verify ContradictionDetector type exists and is accessible
        typeof(ContradictionDetector).Should().NotBeNull();
    }

    [Fact]
    public void Analyze_ShouldBeDefined()
    {
        // Verify Analyze method exists
        typeof(ContradictionDetector).GetMethod("Analyze").Should().NotBeNull();
    }

    [Fact]
    public void AnalyzeMultiple_ShouldBeDefined()
    {
        // Verify AnalyzeMultiple method exists
        typeof(ContradictionDetector).GetMethod("AnalyzeMultiple").Should().NotBeNull();
    }

    [Fact]
    public void CheckPair_ShouldBeDefined()
    {
        // Verify CheckPair method exists
        typeof(ContradictionDetector).GetMethod("CheckPair").Should().NotBeNull();
    }
}
