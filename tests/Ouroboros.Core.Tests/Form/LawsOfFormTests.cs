using Ouroboros.Core.SpencerBrown;
using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.Form;

[Trait("Category", "Unit")]
public class LawsOfFormTests
{
    [Fact]
    public void Mark_CreatesMarkedForm()
    {
        var result = LawsOfForm.Mark(42);

        result.IsMarked.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Void_CreatesUnmarkedForm()
    {
        var result = LawsOfForm.Void<int>();

        result.IsVoid.Should().BeTrue();
        result.IsMarked.Should().BeFalse();
    }

    [Fact]
    public void Cross_MarkedForm_IncrementsDepth()
    {
        var form = LawsOfForm.Mark(1);

        var result = LawsOfForm.Cross(form);

        result.IsMarked.Should().BeTrue();
        result.Depth.Should().BeGreaterThan(form.Depth);
    }

    [Fact]
    public void Cross_VoidForm_CreatesMark()
    {
        var form = LawsOfForm.Void<int>();

        var result = LawsOfForm.Cross(form);

        result.IsMarked.Should().BeTrue();
    }

    [Fact]
    public void Call_MarkedForm_CondensesToDepthOne()
    {
        var form = LawsOfForm.Mark(5);
        var crossed = LawsOfForm.Cross(form);

        var result = LawsOfForm.Call(crossed);

        result.IsMarked.Should().BeTrue();
        result.Depth.Should().Be(1);
    }

    [Fact]
    public void Call_VoidForm_RemainsVoid()
    {
        var form = LawsOfForm.Void<int>();

        var result = LawsOfForm.Call(form);

        result.IsVoid.Should().BeTrue();
    }

    [Fact]
    public void Recross_DepthTwo_ReducesByTwo()
    {
        var form = LawsOfForm.Mark(1);
        var crossed = LawsOfForm.Cross(LawsOfForm.Cross(form));

        var result = LawsOfForm.Recross(crossed);

        result.Depth.Should().Be(crossed.Depth - 2);
    }

    [Fact]
    public void Recross_DepthOne_ReturnsVoid()
    {
        var form = LawsOfForm.Mark(1);

        var result = LawsOfForm.Recross(form);

        result.IsVoid.Should().BeTrue();
    }

    [Fact]
    public async Task MarkArrow_MarksInput()
    {
        var arrow = LawsOfForm.MarkArrow<int>();

        var result = await arrow(42);

        result.IsMarked.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task CrossArrow_CrossesInput()
    {
        var arrow = LawsOfForm.CrossArrow<int>();
        var form = Form<int>.Mark(1);

        var result = await arrow(form);

        result.IsMarked.Should().BeTrue();
        result.Depth.Should().BeGreaterThan(1);
    }

    [Fact]
    public void Product_BothMarked_ReturnsMarkedProduct()
    {
        var a = Form<int>.Mark(1);
        var b = Form<string>.Mark("hello");

        var result = LawsOfForm.Product(a, b);

        result.IsMarked.Should().BeTrue();
        result.Value.Should().Be((1, "hello"));
    }

    [Fact]
    public void Product_OneVoid_ReturnsVoid()
    {
        var a = Form<int>.Mark(1);
        var b = Form<string>.Void();

        var result = LawsOfForm.Product(a, b);

        result.IsVoid.Should().BeTrue();
    }

    [Fact]
    public void Product_BothVoid_ReturnsVoid()
    {
        var a = Form<int>.Void();
        var b = Form<string>.Void();

        var result = LawsOfForm.Product(a, b);

        result.IsVoid.Should().BeTrue();
    }

    [Fact]
    public async Task CrossProduct_BothMarked_ReturnsMarkedProduct()
    {
        Step<int, Form<int>> step1 = input => Task.FromResult(Form<int>.Mark(input));
        Step<int, Form<string>> step2 = input => Task.FromResult(Form<string>.Mark(input.ToString()));

        var arrow = LawsOfForm.CrossProduct(step1, step2);
        var result = await arrow(42);

        result.IsMarked.Should().BeTrue();
    }

    [Fact]
    public void ReEntry_GeneratesFromSeed()
    {
        var result = LawsOfForm.ReEntry<int>(form =>
        {
            if (form.IsVoid)
                return Form<int>.Mark(1);
            return form;
        });

        result.IsMarked.Should().BeTrue();
        result.Value.Should().Be(1);
    }

    [Fact]
    public async Task ReEntryArrow_IteratesSpecifiedTimes()
    {
        int callCount = 0;
        Step<Form<int>, Form<int>> step = form =>
        {
            callCount++;
            return Task.FromResult(form);
        };

        var arrow = LawsOfForm.ReEntryArrow<int>(3, step);
        await arrow(Form<int>.Mark(1));

        callCount.Should().Be(3);
    }
}
