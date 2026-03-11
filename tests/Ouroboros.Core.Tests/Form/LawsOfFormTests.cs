using Ouroboros.Core.SpencerBrown;
using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.Form;

[Trait("Category", "Unit")]
public class LawsOfFormTests
{
    #region Mark

    [Fact]
    public void Mark_CreatesMarkedForm()
    {
        var form = LawsOfForm.Mark(42);

        form.IsMarked.Should().BeTrue();
        form.Value.Should().Be(42);
    }

    [Fact]
    public void Mark_WithString_Works()
    {
        var form = LawsOfForm.Mark("hello");

        form.IsMarked.Should().BeTrue();
        form.Value.Should().Be("hello");
    }

    #endregion

    #region Void

    [Fact]
    public void Void_CreatesVoidForm()
    {
        var form = LawsOfForm.Void<int>();

        form.IsVoid.Should().BeTrue();
    }

    [Fact]
    public void Void_WithString_CreatesVoidForm()
    {
        var form = LawsOfForm.Void<string>();

        form.IsVoid.Should().BeTrue();
    }

    #endregion

    #region Cross

    [Fact]
    public void Cross_CrossesForm()
    {
        var form = Form<int>.Void();

        var crossed = LawsOfForm.Cross(form);

        crossed.IsMarked.Should().BeTrue();
    }

    [Fact]
    public void Cross_MarkedForm_IncrementsDepth()
    {
        var form = Form<int>.Mark(42);

        var crossed = LawsOfForm.Cross(form);

        crossed.Depth.Should().Be(2);
    }

    #endregion

    #region Call (Condensation)

    [Fact]
    public void Call_MarkedForm_ReturnsSameMarkedness()
    {
        var form = Form<int>.Mark(42);

        var called = LawsOfForm.Call(form);

        called.IsMarked.Should().BeTrue();
        called.Depth.Should().Be(1);
    }

    [Fact]
    public void Call_VoidForm_ReturnsVoid()
    {
        var form = Form<int>.Void();

        var called = LawsOfForm.Call(form);

        called.IsVoid.Should().BeTrue();
    }

    #endregion

    #region Recross (Cancellation)

    [Fact]
    public void Recross_HighDepth_ReducesByTwo()
    {
        var form = Form<int>.Mark(42).Cross().Cross(); // depth 3

        var recrossed = LawsOfForm.Recross(form);

        recrossed.Depth.Should().Be(1);
    }

    [Fact]
    public void Recross_DepthOne_ReturnsVoid()
    {
        var form = Form<int>.Mark(42); // depth 1

        var recrossed = LawsOfForm.Recross(form);

        recrossed.IsVoid.Should().BeTrue();
    }

    #endregion

    #region MarkArrow

    [Fact]
    public async Task MarkArrow_MarksInput()
    {
        var arrow = LawsOfForm.MarkArrow<int>();

        var result = await arrow(42);

        result.IsMarked.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task MarkArrow_WithString_MarksInput()
    {
        var arrow = LawsOfForm.MarkArrow<string>();

        var result = await arrow("test");

        result.IsMarked.Should().BeTrue();
        result.Value.Should().Be("test");
    }

    #endregion

    #region CrossArrow

    [Fact]
    public async Task CrossArrow_CrossesInput()
    {
        var arrow = LawsOfForm.CrossArrow<int>();
        var form = Form<int>.Mark(42);

        var result = await arrow(form);

        result.Depth.Should().Be(2);
    }

    [Fact]
    public async Task CrossArrow_FromVoid_CreatesMark()
    {
        var arrow = LawsOfForm.CrossArrow<int>();
        var form = Form<int>.Void();

        var result = await arrow(form);

        result.IsMarked.Should().BeTrue();
    }

    #endregion

    #region Product

    [Fact]
    public void Product_BothMarked_ReturnsMarkedTuple()
    {
        var a = Form<int>.Mark(1);
        var b = Form<string>.Mark("two");

        var product = LawsOfForm.Product(a, b);

        product.IsMarked.Should().BeTrue();
        product.Value.Should().Be((1, "two"));
    }

    [Fact]
    public void Product_FirstVoid_ReturnsVoid()
    {
        var a = Form<int>.Void();
        var b = Form<string>.Mark("two");

        var product = LawsOfForm.Product(a, b);

        product.IsVoid.Should().BeTrue();
    }

    [Fact]
    public void Product_SecondVoid_ReturnsVoid()
    {
        var a = Form<int>.Mark(1);
        var b = Form<string>.Void();

        var product = LawsOfForm.Product(a, b);

        product.IsVoid.Should().BeTrue();
    }

    [Fact]
    public void Product_BothVoid_ReturnsVoid()
    {
        var a = Form<int>.Void();
        var b = Form<string>.Void();

        var product = LawsOfForm.Product(a, b);

        product.IsVoid.Should().BeTrue();
    }

    #endregion

    #region CrossProduct

    [Fact]
    public async Task CrossProduct_BothSucceed_ReturnsCombinedProduct()
    {
        Step<string, Form<int>> step1 = s => Task.FromResult(Form<int>.Mark(s.Length));
        Step<string, Form<string>> step2 = s => Task.FromResult(Form<string>.Mark(s.ToUpper()));

        var combined = LawsOfForm.CrossProduct(step1, step2);
        var result = await combined("hi");

        result.IsMarked.Should().BeTrue();
        result.Value.Should().Be((2, "HI"));
    }

    [Fact]
    public async Task CrossProduct_FirstFails_ReturnsVoid()
    {
        Step<string, Form<int>> step1 = _ => Task.FromResult(Form<int>.Void());
        Step<string, Form<string>> step2 = s => Task.FromResult(Form<string>.Mark(s));

        var combined = LawsOfForm.CrossProduct(step1, step2);
        var result = await combined("test");

        result.IsVoid.Should().BeTrue();
    }

    [Fact]
    public async Task CrossProduct_SecondFails_ReturnsVoid()
    {
        Step<string, Form<int>> step1 = s => Task.FromResult(Form<int>.Mark(s.Length));
        Step<string, Form<string>> step2 = _ => Task.FromResult(Form<string>.Void());

        var combined = LawsOfForm.CrossProduct(step1, step2);
        var result = await combined("test");

        result.IsVoid.Should().BeTrue();
    }

    #endregion

    #region ReEntry

    [Fact]
    public void ReEntry_WithIdentity_ReturnsGeneratorOfVoid()
    {
        var result = LawsOfForm.ReEntry<int>(f => f);

        result.IsVoid.Should().BeTrue();
    }

    [Fact]
    public void ReEntry_WithCross_ReturnsMarked()
    {
        var result = LawsOfForm.ReEntry<int>(f => f.Cross());

        result.IsMarked.Should().BeTrue();
    }

    [Fact]
    public void ReEntry_WithMarkGenerator_ReturnsMarked()
    {
        var result = LawsOfForm.ReEntry<int>(_ => Form<int>.Mark(42));

        result.IsMarked.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    #endregion

    #region ReEntryArrow

    [Fact]
    public async Task ReEntryArrow_ZeroIterations_ReturnsInput()
    {
        Step<Form<int>, Form<int>> step = f => Task.FromResult(f.Cross());

        var arrow = LawsOfForm.ReEntryArrow(0, step);
        var input = Form<int>.Mark(42);

        var result = await arrow(input);

        result.Should().Be(input);
    }

    [Fact]
    public async Task ReEntryArrow_OneIteration_AppliesOnce()
    {
        int callCount = 0;
        Step<Form<int>, Form<int>> step = f =>
        {
            callCount++;
            return Task.FromResult(f.Cross());
        };

        var arrow = LawsOfForm.ReEntryArrow(1, step);

        await arrow(Form<int>.Mark(42));

        callCount.Should().Be(1);
    }

    [Fact]
    public async Task ReEntryArrow_MultipleIterations_AppliesMultipleTimes()
    {
        int callCount = 0;
        Step<Form<int>, Form<int>> step = f =>
        {
            callCount++;
            return Task.FromResult(f);
        };

        var arrow = LawsOfForm.ReEntryArrow(5, step);

        await arrow(Form<int>.Void());

        callCount.Should().Be(5);
    }

    #endregion
}
