// <copyright file="FormReasoningEventArgsTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools.MeTTa;

using FluentAssertions;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using Ouroboros.Tools.MeTTa;
using Xunit;

/// <summary>
/// Unit tests for <see cref="FormReasoningEventArgs"/>.
/// </summary>
[Trait("Category", "Unit")]
public class FormReasoningEventArgsTests
{
    [Fact]
    public void Constructor_WithRequiredProperties_CreatesInstance()
    {
        // Act
        var args = new FormReasoningEventArgs
        {
            Operation = "test_operation",
        };

        // Assert
        args.Operation.Should().Be("test_operation");
    }

    [Fact]
    public void DefaultFormState_IsDefault()
    {
        // Act
        var args = new FormReasoningEventArgs
        {
            Operation = "test",
        };

        // Assert - Form is a struct, so default value is valid
        args.FormState.Should().NotBeNull();
    }

    [Fact]
    public void FormState_CanBeSetToMark()
    {
        // Act
        var args = new FormReasoningEventArgs
        {
            Operation = "draw",
            FormState = Form.Mark,
        };

        // Assert
        args.FormState.IsMarked().Should().BeTrue();
    }

    [Fact]
    public void FormState_CanBeSetToVoid()
    {
        // Act
        var args = new FormReasoningEventArgs
        {
            Operation = "cross",
            FormState = Form.Void,
        };

        // Assert
        args.FormState.IsVoid().Should().BeTrue();
    }

    [Fact]
    public void FormState_CanBeSetToImaginary()
    {
        // Act
        var args = new FormReasoningEventArgs
        {
            Operation = "reentry",
            FormState = Form.Imaginary,
        };

        // Assert
        args.FormState.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void Context_DefaultsToNull()
    {
        // Act
        var args = new FormReasoningEventArgs
        {
            Operation = "test",
        };

        // Assert
        args.Context.Should().BeNull();
    }

    [Fact]
    public void Context_CanBeSet()
    {
        // Act
        var args = new FormReasoningEventArgs
        {
            Operation = "test",
            Context = "my-context",
        };

        // Assert
        args.Context.Should().Be("my-context");
    }

    [Fact]
    public void RelatedAtoms_DefaultsToEmpty()
    {
        // Act
        var args = new FormReasoningEventArgs
        {
            Operation = "test",
        };

        // Assert
        args.RelatedAtoms.Should().BeEmpty();
    }

    [Fact]
    public void RelatedAtoms_CanBeSet()
    {
        // Arrange
        var atoms = new List<Atom> { Atom.Sym("test") };

        // Act
        var args = new FormReasoningEventArgs
        {
            Operation = "test",
            RelatedAtoms = atoms,
        };

        // Assert
        args.RelatedAtoms.Should().HaveCount(1);
    }

    [Fact]
    public void Trace_DefaultsToEmpty()
    {
        // Act
        var args = new FormReasoningEventArgs
        {
            Operation = "test",
        };

        // Assert
        args.Trace.Should().BeEmpty();
    }

    [Fact]
    public void Trace_CanBeSet()
    {
        // Act
        var args = new FormReasoningEventArgs
        {
            Operation = "test",
            Trace = new List<string> { "step 1", "step 2" },
        };

        // Assert
        args.Trace.Should().HaveCount(2);
        args.Trace.Should().Contain("step 1");
    }

    [Fact]
    public void Timestamp_DefaultsToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var args = new FormReasoningEventArgs
        {
            Operation = "test",
        };

        var after = DateTime.UtcNow;

        // Assert
        args.Timestamp.Should().BeOnOrAfter(before);
        args.Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void InheritsFromEventArgs()
    {
        // Arrange
        var args = new FormReasoningEventArgs
        {
            Operation = "test",
        };

        // Assert
        args.Should().BeAssignableTo<EventArgs>();
    }
}
