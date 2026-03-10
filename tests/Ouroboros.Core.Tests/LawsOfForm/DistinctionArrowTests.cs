// <copyright file="DistinctionArrowTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;
using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for <see cref="DistinctionArrow"/> which provides Kleisli arrows
/// for distinction-based reasoning.
/// </summary>
[Trait("Category", "Unit")]
public class DistinctionArrowTests
{
    // ──────────── Gate ────────────

    [Fact]
    public async Task Gate_PredicateReturnsMark_ReturnsInput()
    {
        Step<string, string?> gate = DistinctionArrow.Gate<string>(_ => Form.Mark);

        string? result = await gate("hello");

        result.Should().Be("hello");
    }

    [Fact]
    public async Task Gate_PredicateReturnsVoid_ReturnsNull()
    {
        Step<string, string?> gate = DistinctionArrow.Gate<string>(_ => Form.Void);

        string? result = await gate("hello");

        result.Should().BeNull();
    }

    [Fact]
    public async Task Gate_PredicateReturnsImaginary_ReturnsNull()
    {
        Step<string, string?> gate = DistinctionArrow.Gate<string>(_ => Form.Imaginary);

        string? result = await gate("hello");

        result.Should().BeNull();
    }

    // ──────────── Branch ────────────

    [Fact]
    public async Task Branch_PredicateReturnsMark_AppliesOnMarkedTransform()
    {
        Step<int, string> branch = DistinctionArrow.Branch<int, string>(
            _ => Form.Mark,
            x => $"marked:{x}",
            x => $"void:{x}");

        string result = await branch(42);

        result.Should().Be("marked:42");
    }

    [Fact]
    public async Task Branch_PredicateReturnsVoid_AppliesOnVoidTransform()
    {
        Step<int, string> branch = DistinctionArrow.Branch<int, string>(
            _ => Form.Void,
            x => $"marked:{x}",
            x => $"void:{x}");

        string result = await branch(42);

        result.Should().Be("void:42");
    }

    [Fact]
    public async Task Branch_PredicateReturnsImaginary_AppliesOnVoidTransform()
    {
        Step<int, string> branch = DistinctionArrow.Branch<int, string>(
            _ => Form.Imaginary,
            x => $"marked:{x}",
            x => $"void:{x}");

        string result = await branch(42);

        result.Should().Be("void:42");
    }

    [Fact]
    public async Task Branch_NullInput_AppliesOnVoidTransform()
    {
        Step<string, string> branch = DistinctionArrow.Branch<string, string>(
            _ => Form.Mark,
            x => "marked",
            x => "void");

        string result = await branch(null!);

        result.Should().Be("void");
    }

    // ──────────── AllMarked ────────────

    [Fact]
    public async Task AllMarked_AllPredicatesReturnMark_ReturnsInput()
    {
        Step<string, string?> arrow = DistinctionArrow.AllMarked<string>(
            _ => Form.Mark,
            _ => Form.Mark,
            _ => Form.Mark);

        string? result = await arrow("test");

        result.Should().Be("test");
    }

    [Fact]
    public async Task AllMarked_OnePredicateReturnsVoid_ReturnsNull()
    {
        Step<string, string?> arrow = DistinctionArrow.AllMarked<string>(
            _ => Form.Mark,
            _ => Form.Void,
            _ => Form.Mark);

        string? result = await arrow("test");

        result.Should().BeNull();
    }

    [Fact]
    public async Task AllMarked_OnePredicateReturnsImaginary_ReturnsNull()
    {
        Step<string, string?> arrow = DistinctionArrow.AllMarked<string>(
            _ => Form.Mark,
            _ => Form.Imaginary);

        string? result = await arrow("test");

        result.Should().BeNull();
    }

    [Fact]
    public async Task AllMarked_NoPredicates_ReturnsInput()
    {
        Step<string, string?> arrow = DistinctionArrow.AllMarked<string>();

        string? result = await arrow("test");

        result.Should().Be("test");
    }

    // ──────────── AnyMarked ────────────

    [Fact]
    public async Task AnyMarked_OnePredicateReturnsMark_ReturnsInput()
    {
        Step<string, string?> arrow = DistinctionArrow.AnyMarked<string>(
            _ => Form.Void,
            _ => Form.Mark,
            _ => Form.Void);

        string? result = await arrow("test");

        result.Should().Be("test");
    }

    [Fact]
    public async Task AnyMarked_AllPredicatesReturnVoid_ReturnsNull()
    {
        Step<string, string?> arrow = DistinctionArrow.AnyMarked<string>(
            _ => Form.Void,
            _ => Form.Void);

        string? result = await arrow("test");

        result.Should().BeNull();
    }

    [Fact]
    public async Task AnyMarked_NoPredicates_ReturnsNull()
    {
        Step<string, string?> arrow = DistinctionArrow.AnyMarked<string>();

        string? result = await arrow("test");

        result.Should().BeNull();
    }

    // ──────────── Evaluate ────────────

    [Fact]
    public async Task Evaluate_ExtractsAndCombinesForm()
    {
        Step<string, string> arrow = DistinctionArrow.Evaluate<string>(
            _ => Form.Mark,
            (input, form) => $"{input}:{form}");

        string result = await arrow("data");

        result.Should().Be("data:⌐");
    }

    [Fact]
    public async Task Evaluate_VoidForm_ExtractsCorrectly()
    {
        Step<string, string> arrow = DistinctionArrow.Evaluate<string>(
            _ => Form.Void,
            (input, form) => $"{input}:{form}");

        string result = await arrow("data");

        result.Should().Be("data:∅");
    }

    // ──────────── ReEntry ────────────

    [Fact]
    public async Task ReEntry_FixedPoint_StopsAtFixedPoint()
    {
        // Self-reference always returns Mark regardless of current => fixed point at Mark
        Step<string, Form> arrow = DistinctionArrow.ReEntry<string>(
            (input, current) => Form.Mark,
            maxDepth: 10);

        Form result = await arrow("input");

        // First iteration: current = Void, next = Mark (no fixed point yet)
        // Second iteration: current = Mark, next = Mark (fixed point!)
        result.Should().Be(Form.Mark);
    }

    [Fact]
    public async Task ReEntry_Oscillating_ReturnsLastComputedForm()
    {
        // Self-reference toggles: always returns opposite of current
        Step<string, Form> arrow = DistinctionArrow.ReEntry<string>(
            (input, current) => current.Not(),
            maxDepth: 5);

        Form result = await arrow("input");

        // The loop oscillates: Void->Mark->Void->Mark->Void
        // After 5 iterations, returns last computed form
        result.Should().BeOneOf(Form.Mark, Form.Void);
    }

    [Fact]
    public async Task ReEntry_ImaginarySelfReference_FixedPoint()
    {
        // Imaginary.Not() == Imaginary, so fixed point at first iteration
        Step<string, Form> arrow = DistinctionArrow.ReEntry<string>(
            (input, current) => Form.Imaginary,
            maxDepth: 10);

        Form result = await arrow("input");

        // current starts as Void, next = Imaginary (not fixed)
        // current = Imaginary, next = Imaginary (fixed!)
        result.Should().Be(Form.Imaginary);
    }

    [Fact]
    public async Task ReEntry_DefaultMaxDepth_IsReasonable()
    {
        int callCount = 0;
        Step<string, Form> arrow = DistinctionArrow.ReEntry<string>(
            (input, current) =>
            {
                callCount++;
                return current.Not(); // oscillates
            });

        await arrow("test");

        callCount.Should().BeLessOrEqualTo(10);
    }

    // ──────────── LiftPredicate ────────────

    [Fact]
    public void LiftPredicate_TrueCondition_ReturnsMark()
    {
        Func<int, Form> lifted = DistinctionArrow.LiftPredicate<int>(x => x > 0);

        Form result = lifted(42);

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void LiftPredicate_FalseCondition_ReturnsVoid()
    {
        Func<int, Form> lifted = DistinctionArrow.LiftPredicate<int>(x => x > 0);

        Form result = lifted(-1);

        result.Should().Be(Form.Void);
    }
}
