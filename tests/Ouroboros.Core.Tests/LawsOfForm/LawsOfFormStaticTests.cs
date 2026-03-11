// <copyright file="LawsOfFormStaticTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.SpencerBrown;
using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for the <see cref="Ouroboros.Core.SpencerBrown.LawsOfForm"/> static class.
/// Validates Spencer-Brown's Laws of Form operations on the generic Form{T} type.
/// </summary>
[Trait("Category", "Unit")]
public class LawsOfFormStaticTests
{
    // --- Mark ---

    [Fact]
    public void Mark_CreatesMarkedForm()
    {
        var form = Ouroboros.Core.SpencerBrown.LawsOfForm.Mark(42);

        form.IsMarked.Should().BeTrue();
        form.Value.Should().Be(42);
    }

    [Fact]
    public void Mark_StringValue_CreatesMarkedForm()
    {
        var form = Ouroboros.Core.SpencerBrown.LawsOfForm.Mark("hello");

        form.IsMarked.Should().BeTrue();
        form.Value.Should().Be("hello");
    }

    // --- Void ---

    [Fact]
    public void Void_CreatesVoidForm()
    {
        var form = Ouroboros.Core.SpencerBrown.LawsOfForm.Void<int>();

        form.IsVoid.Should().BeTrue();
        form.IsMarked.Should().BeFalse();
    }

    // --- Cross ---

    [Fact]
    public void Cross_MarkedForm_CrossesBoundary()
    {
        var marked = Form<int>.Mark(10);

        var crossed = Ouroboros.Core.SpencerBrown.LawsOfForm.Cross(marked);

        crossed.IsMarked.Should().BeTrue();
        crossed.Depth.Should().Be(2); // Mark creates depth 1, cross adds 1
    }

    [Fact]
    public void Cross_VoidForm_CreatesMark()
    {
        var v = Form<int>.Void();

        var crossed = Ouroboros.Core.SpencerBrown.LawsOfForm.Cross(v);

        crossed.IsMarked.Should().BeTrue();
    }

    // --- Call (Law of Calling / Condensation) ---

    [Fact]
    public void Call_MarkedForm_ReturnsMarkAtDepth1()
    {
        var form = Form<int>.Mark(5);
        var crossed = form.Cross(); // depth 2

        var called = Ouroboros.Core.SpencerBrown.LawsOfForm.Call(crossed);

        called.IsMarked.Should().BeTrue();
        called.Depth.Should().Be(1);
    }

    [Fact]
    public void Call_VoidForm_ReturnsVoid()
    {
        var v = Form<int>.Void();

        var called = Ouroboros.Core.SpencerBrown.LawsOfForm.Call(v);

        called.IsVoid.Should().BeTrue();
    }

    [Fact]
    public void Call_Idempotent_CallingMarkedIsStillMarked()
    {
        var form = Form<string>.Mark("x");

        var called = Ouroboros.Core.SpencerBrown.LawsOfForm.Call(form);

        called.IsMarked.Should().BeTrue();
        called.Value.Should().Be("x");
    }

    // --- Recross (Law of Crossing / Cancellation) ---

    [Fact]
    public void Recross_DoubleCrossed_ReturnsOriginalDepth()
    {
        var form = Form<int>.Mark(10);
        var doubleCrossed = form.Cross().Cross(); // depth 3

        var recrossed = Ouroboros.Core.SpencerBrown.LawsOfForm.Recross(doubleCrossed);

        recrossed.Depth.Should().Be(1); // 3 - 2 = 1
    }

    [Fact]
    public void Recross_SingleDepth_ReturnsVoid()
    {
        var form = Form<int>.Mark(10); // depth 1

        var recrossed = Ouroboros.Core.SpencerBrown.LawsOfForm.Recross(form);

        recrossed.IsVoid.Should().BeTrue();
    }

    [Fact]
    public void Recross_VoidForm_ReturnsVoid()
    {
        var v = Form<int>.Void();

        var recrossed = Ouroboros.Core.SpencerBrown.LawsOfForm.Recross(v);

        recrossed.IsVoid.Should().BeTrue();
    }

    // --- MarkArrow ---

    [Fact]
    public async Task MarkArrow_ProducesMarkedForm()
    {
        var arrow = Ouroboros.Core.SpencerBrown.LawsOfForm.MarkArrow<int>();

        var result = await arrow(42);

        result.IsMarked.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    // --- CrossArrow ---

    [Fact]
    public async Task CrossArrow_CrossesForm()
    {
        var arrow = Ouroboros.Core.SpencerBrown.LawsOfForm.CrossArrow<int>();
        var input = Form<int>.Mark(5);

        var result = await arrow(input);

        result.IsMarked.Should().BeTrue();
        result.Depth.Should().Be(2);
    }

    // --- Product ---

    [Fact]
    public void Product_BothMarked_ReturnsMarkedTuple()
    {
        var f1 = Form<int>.Mark(1);
        var f2 = Form<string>.Mark("a");

        var product = Ouroboros.Core.SpencerBrown.LawsOfForm.Product(f1, f2);

        product.IsMarked.Should().BeTrue();
        product.Value.Should().Be((1, "a"));
    }

    [Fact]
    public void Product_FirstVoid_ReturnsVoid()
    {
        var f1 = Form<int>.Void();
        var f2 = Form<string>.Mark("a");

        var product = Ouroboros.Core.SpencerBrown.LawsOfForm.Product(f1, f2);

        product.IsVoid.Should().BeTrue();
    }

    [Fact]
    public void Product_SecondVoid_ReturnsVoid()
    {
        var f1 = Form<int>.Mark(1);
        var f2 = Form<string>.Void();

        var product = Ouroboros.Core.SpencerBrown.LawsOfForm.Product(f1, f2);

        product.IsVoid.Should().BeTrue();
    }

    [Fact]
    public void Product_BothVoid_ReturnsVoid()
    {
        var f1 = Form<int>.Void();
        var f2 = Form<string>.Void();

        var product = Ouroboros.Core.SpencerBrown.LawsOfForm.Product(f1, f2);

        product.IsVoid.Should().BeTrue();
    }

    // --- CrossProduct ---

    [Fact]
    public async Task CrossProduct_BothStepsMarked_ReturnsMarkedProductForm()
    {
        Step<int, Form<int>> s1 = input => Task.FromResult(Form<int>.Mark(input));
        Step<int, Form<int>> s2 = input => Task.FromResult(Form<int>.Mark(input * 2));

        var crossProduct = Ouroboros.Core.SpencerBrown.LawsOfForm.CrossProduct(s1, s2);
        var result = await crossProduct(5);

        result.IsMarked.Should().BeTrue();
        result.Value.Should().Be((5, 10));
    }

    [Fact]
    public async Task CrossProduct_OneStepVoid_ReturnsVoid()
    {
        Step<int, Form<int>> s1 = input => Task.FromResult(Form<int>.Mark(input));
        Step<int, Form<string>> s2 = _ => Task.FromResult(Form<string>.Void());

        var crossProduct = Ouroboros.Core.SpencerBrown.LawsOfForm.CrossProduct(s1, s2);
        var result = await crossProduct(5);

        result.IsVoid.Should().BeTrue();
    }

    // --- ReEntry ---

    [Fact]
    public void ReEntry_ReturnsGeneratorResult()
    {
        var result = Ouroboros.Core.SpencerBrown.LawsOfForm.ReEntry<int>(
            seed => Form<int>.Mark(42));

        result.IsMarked.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ReEntry_ReceivesVoidAsSeed()
    {
        Form<int> seedCapture = default;
        Ouroboros.Core.SpencerBrown.LawsOfForm.ReEntry<int>(seed =>
        {
            seedCapture = seed;
            return Form<int>.Mark(1);
        });

        seedCapture.IsVoid.Should().BeTrue();
    }

    // --- ReEntryArrow ---

    [Fact]
    public async Task ReEntryArrow_IteratesSpecifiedTimes()
    {
        int callCount = 0;
        Step<Form<int>, Form<int>> step = input =>
        {
            callCount++;
            return Task.FromResult(input);
        };

        var arrow = Ouroboros.Core.SpencerBrown.LawsOfForm.ReEntryArrow(3, step);
        await arrow(Form<int>.Mark(1));

        callCount.Should().Be(3);
    }

    [Fact]
    public async Task ReEntryArrow_ZeroIterations_ReturnsSameInput()
    {
        Step<Form<int>, Form<int>> step = _ =>
            Task.FromResult(Form<int>.Void());

        var arrow = Ouroboros.Core.SpencerBrown.LawsOfForm.ReEntryArrow(0, step);
        var input = Form<int>.Mark(42);

        var result = await arrow(input);

        result.Should().Be(input);
    }

    [Fact]
    public async Task ReEntryArrow_TransformsAcrossIterations()
    {
        // Each iteration crosses the form
        Step<Form<int>, Form<int>> crossStep = input =>
            Task.FromResult(input.Cross());

        var arrow = Ouroboros.Core.SpencerBrown.LawsOfForm.ReEntryArrow(2, crossStep);
        var input = Form<int>.Mark(5); // depth 1

        var result = await arrow(input);

        result.Depth.Should().Be(3); // depth 1 + 2 crosses
    }
}
