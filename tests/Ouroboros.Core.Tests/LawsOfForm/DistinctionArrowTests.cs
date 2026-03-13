using Ouroboros.Core.LawsOfForm;
using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class DistinctionArrowTests
{
    [Fact]
    public async Task Gate_MarkedPredicate_ReturnsInput()
    {
        var gate = DistinctionArrow.Gate<string>(_ => Form.Mark);

        var result = await gate("hello");

        result.Should().Be("hello");
    }

    [Fact]
    public async Task Gate_VoidPredicate_ReturnsDefault()
    {
        var gate = DistinctionArrow.Gate<string>(_ => Form.Void);

        var result = await gate("hello");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Branch_MarkedPredicate_CallsOnMarked()
    {
        var branch = DistinctionArrow.Branch<string, int>(
            _ => Form.Mark,
            s => s.Length,
            _ => -1);

        var result = await branch("hello");

        result.Should().Be(5);
    }

    [Fact]
    public async Task Branch_VoidPredicate_CallsOnVoid()
    {
        var branch = DistinctionArrow.Branch<string, int>(
            _ => Form.Void,
            s => s.Length,
            _ => -1);

        var result = await branch("hello");

        result.Should().Be(-1);
    }

    [Fact]
    public async Task Branch_NullInput_CallsOnVoid()
    {
        var branch = DistinctionArrow.Branch<string, int>(
            _ => Form.Mark,
            _ => 1,
            _ => -1);

        var result = await branch(null!);

        result.Should().Be(-1);
    }

    [Fact]
    public async Task AllMarked_AllTrue_ReturnsInput()
    {
        var arrow = DistinctionArrow.AllMarked<string>(
            _ => Form.Mark,
            _ => Form.Mark);

        var result = await arrow("test");

        result.Should().Be("test");
    }

    [Fact]
    public async Task AllMarked_OneFails_ReturnsDefault()
    {
        var arrow = DistinctionArrow.AllMarked<string>(
            _ => Form.Mark,
            _ => Form.Void);

        var result = await arrow("test");

        result.Should().BeNull();
    }

    [Fact]
    public async Task AnyMarked_OneTrue_ReturnsInput()
    {
        var arrow = DistinctionArrow.AnyMarked<string>(
            _ => Form.Void,
            _ => Form.Mark);

        var result = await arrow("test");

        result.Should().Be("test");
    }

    [Fact]
    public async Task AnyMarked_NoneTrue_ReturnsDefault()
    {
        var arrow = DistinctionArrow.AnyMarked<string>(
            _ => Form.Void,
            _ => Form.Void);

        var result = await arrow("test");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Evaluate_ExtractsAndCombines()
    {
        var arrow = DistinctionArrow.Evaluate<string>(
            _ => Form.Mark,
            (input, form) => $"{input}:{form}");

        var result = await arrow("test");

        result.Should().Contain("test");
    }

    [Fact]
    public async Task ReEntry_FixedPoint_ReturnsEarly()
    {
        // Self-reference that immediately reaches a fixed point (always returns Void)
        var arrow = DistinctionArrow.ReEntry<string>(
            (_, _) => Form.Void,
            maxDepth: 10);

        var result = await arrow("test");

        result.Should().Be(Form.Void);
    }

    [Fact]
    public async Task ReEntry_MaxDepthReached_ReturnsLastForm()
    {
        int callCount = 0;
        var arrow = DistinctionArrow.ReEntry<string>(
            (_, _) =>
            {
                callCount++;
                // Always alternate to prevent fixed point
                return callCount % 2 == 0 ? Form.Void : Form.Mark;
            },
            maxDepth: 5);

        var result = await arrow("test");

        callCount.Should().Be(5);
    }

    [Fact]
    public void LiftPredicate_TruePredicate_ReturnsMark()
    {
        var lifted = DistinctionArrow.LiftPredicate<int>(x => x > 0);

        var result = lifted(5);

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void LiftPredicate_FalsePredicate_ReturnsVoid()
    {
        var lifted = DistinctionArrow.LiftPredicate<int>(x => x > 0);

        var result = lifted(-1);

        result.Should().Be(Form.Void);
    }
}
