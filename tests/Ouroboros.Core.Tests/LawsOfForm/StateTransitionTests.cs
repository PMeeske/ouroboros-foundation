// <copyright file="StateTransitionTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for the <see cref="StateTransition{TState}"/> record.
/// </summary>
[Trait("Category", "Unit")]
public class StateTransitionTests
{
    // --- Constructor ---

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var timestamp = new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);
        var form = Form.Mark;
        var state = Option<string>.Some("Active");
        var reason = "Activated";

        var transition = new StateTransition<string>(timestamp, form, state, reason);

        transition.Timestamp.Should().Be(timestamp);
        transition.Form.Should().Be(form);
        transition.State.HasValue.Should().BeTrue();
        transition.State.Value.Should().Be("Active");
        transition.Reason.Should().Be("Activated");
    }

    [Fact]
    public void Constructor_WithNoneState_SetsCorrectly()
    {
        var timestamp = DateTimeOffset.UtcNow;
        var state = Option<string>.None();

        var transition = new StateTransition<string>(timestamp, Form.Imaginary, state, "Indeterminate");

        transition.State.HasValue.Should().BeFalse();
        transition.Form.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void Constructor_WithVoidForm_SetsCorrectly()
    {
        var transition = new StateTransition<int>(
            DateTimeOffset.UtcNow,
            Form.Void,
            Option<int>.None(),
            "Impossible state");

        transition.Form.Should().Be(Form.Void);
    }

    // --- Record Equality ---

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var ts = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var state = Option<string>.Some("X");

        var t1 = new StateTransition<string>(ts, Form.Mark, state, "reason");
        var t2 = new StateTransition<string>(ts, Form.Mark, state, "reason");

        t1.Should().Be(t2);
    }

    [Fact]
    public void RecordEquality_DifferentForm_AreNotEqual()
    {
        var ts = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var state = Option<string>.Some("X");

        var t1 = new StateTransition<string>(ts, Form.Mark, state, "reason");
        var t2 = new StateTransition<string>(ts, Form.Void, state, "reason");

        t1.Should().NotBe(t2);
    }

    [Fact]
    public void RecordEquality_DifferentReason_AreNotEqual()
    {
        var ts = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var state = Option<string>.Some("X");

        var t1 = new StateTransition<string>(ts, Form.Mark, state, "reason1");
        var t2 = new StateTransition<string>(ts, Form.Mark, state, "reason2");

        t1.Should().NotBe(t2);
    }

    [Fact]
    public void RecordEquality_DifferentTimestamp_AreNotEqual()
    {
        var state = Option<string>.Some("X");

        var t1 = new StateTransition<string>(
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Form.Mark, state, "reason");
        var t2 = new StateTransition<string>(
            new DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero),
            Form.Mark, state, "reason");

        t1.Should().NotBe(t2);
    }

    // --- With expression (record) ---

    [Fact]
    public void WithExpression_CanCreateModifiedCopy()
    {
        var original = new StateTransition<string>(
            DateTimeOffset.UtcNow, Form.Mark, Option<string>.Some("A"), "initial");

        var modified = original with { Reason = "updated" };

        modified.Reason.Should().Be("updated");
        modified.Form.Should().Be(Form.Mark);
        original.Reason.Should().Be("initial");
    }

    // --- Different TState types ---

    [Fact]
    public void WorksWithIntStateType()
    {
        var transition = new StateTransition<int>(
            DateTimeOffset.UtcNow, Form.Mark, Option<int>.Some(42), "set to 42");

        transition.State.Value.Should().Be(42);
    }

    [Fact]
    public void WorksWithEnumStateType()
    {
        var transition = new StateTransition<DayOfWeek>(
            DateTimeOffset.UtcNow, Form.Mark, Option<DayOfWeek>.Some(DayOfWeek.Monday), "Monday");

        transition.State.Value.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void NoneState_WithDifferentTypes_WorksCorrectly()
    {
        var intTransition = new StateTransition<int>(
            DateTimeOffset.UtcNow, Form.Imaginary, Option<int>.None(), "uncertain");

        intTransition.State.HasValue.Should().BeFalse();
    }
}
