using Ouroboros.Core.SpencerBrown;

namespace Ouroboros.Core.Tests.Form;

[Trait("Category", "Unit")]
public class FormTests
{
    #region Mark

    [Fact]
    public void Mark_CreatesMarkedForm()
    {
        var form = Form<int>.Mark(42);

        form.IsMarked.Should().BeTrue();
        form.IsVoid.Should().BeFalse();
        form.Value.Should().Be(42);
    }

    [Fact]
    public void Mark_SetsDepthTo1()
    {
        var form = Form<string>.Mark("hello");

        form.Depth.Should().Be(1);
    }

    [Fact]
    public void Mark_WithNullableReferenceType_StoresNull()
    {
        var form = Form<string>.Mark(null!);

        form.IsMarked.Should().BeTrue();
    }

    #endregion

    #region Void

    [Fact]
    public void Void_CreatesUnmarkedForm()
    {
        var form = Form<int>.Void();

        form.IsVoid.Should().BeTrue();
        form.IsMarked.Should().BeFalse();
        form.Value.Should().Be(default(int));
    }

    [Fact]
    public void Void_HasDepthZero()
    {
        var form = Form<int>.Void();

        form.Depth.Should().Be(0);
    }

    #endregion

    #region Cross

    [Fact]
    public void Cross_FromVoid_CreatesMark()
    {
        var form = Form<int>.Void();

        var crossed = form.Cross();

        crossed.IsMarked.Should().BeTrue();
        crossed.Depth.Should().Be(1);
    }

    [Fact]
    public void Cross_FromMarked_IncrementsDepth()
    {
        var form = Form<int>.Mark(42);

        var crossed = form.Cross();

        crossed.IsMarked.Should().BeTrue();
        crossed.Depth.Should().Be(2);
    }

    [Fact]
    public void Cross_MultipleTimes_IncrementsDepthEachTime()
    {
        var form = Form<int>.Mark(42);

        var crossed = form.Cross().Cross();

        crossed.Depth.Should().Be(3);
    }

    #endregion

    #region Call (Law of Calling)

    [Fact]
    public void Call_MarkedForm_ReturnsMarkedAtDepth1()
    {
        var form = Form<int>.Mark(42).Cross(); // depth 2

        var called = form.Call();

        called.IsMarked.Should().BeTrue();
        called.Depth.Should().Be(1);
    }

    [Fact]
    public void Call_VoidForm_ReturnsVoid()
    {
        var form = Form<int>.Void();

        var called = form.Call();

        called.IsVoid.Should().BeTrue();
    }

    [Fact]
    public void Call_IsIdempotent()
    {
        var form = Form<int>.Mark(42);

        var called = form.Call().Call();

        called.Should().Be(form.Call());
    }

    #endregion

    #region Recross (Law of Crossing)

    [Fact]
    public void Recross_DepthGreaterThanOrEqual2_ReducesBy2()
    {
        var form = Form<int>.Mark(42).Cross().Cross(); // depth 3

        var recrossed = form.Recross();

        recrossed.Depth.Should().Be(1);
        recrossed.IsMarked.Should().BeTrue();
    }

    [Fact]
    public void Recross_DepthExactly1_ReturnsVoid()
    {
        var form = Form<int>.Mark(42); // depth 1

        var recrossed = form.Recross();

        recrossed.IsVoid.Should().BeTrue();
    }

    [Fact]
    public void Recross_DepthZero_ReturnsSelf()
    {
        var form = Form<int>.Void(); // depth 0

        var recrossed = form.Recross();

        recrossed.Should().Be(form);
    }

    [Fact]
    public void Recross_DepthExactly2_ReducesToDepth0()
    {
        var form = Form<int>.Mark(42).Cross(); // depth 2

        var recrossed = form.Recross();

        recrossed.Depth.Should().Be(0);
    }

    #endregion

    #region Bind

    [Fact]
    public void Bind_Marked_AppliesFunction()
    {
        var form = Form<int>.Mark(42);

        var result = form.Bind(v => Form<string>.Mark(v.ToString()));

        result.IsMarked.Should().BeTrue();
        result.Value.Should().Be("42");
    }

    [Fact]
    public void Bind_Void_ReturnsVoid()
    {
        var form = Form<int>.Void();

        var result = form.Bind(v => Form<string>.Mark(v.ToString()));

        result.IsVoid.Should().BeTrue();
    }

    [Fact]
    public void Bind_MarkedWithNullValue_ReturnsVoid()
    {
        var form = Form<string>.Mark(null!);

        var result = form.Bind(v => Form<int>.Mark(v.Length));

        result.IsVoid.Should().BeTrue();
    }

    #endregion

    #region Map

    [Fact]
    public void Map_Marked_TransformsValue()
    {
        var form = Form<int>.Mark(5);

        var result = form.Map(v => v * 2);

        result.IsMarked.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void Map_Void_ReturnsVoid()
    {
        var form = Form<int>.Void();

        var result = form.Map(v => v * 2);

        result.IsVoid.Should().BeTrue();
    }

    [Fact]
    public void Map_MarkedWithNullValue_ReturnsVoid()
    {
        var form = Form<string>.Mark(null!);

        var result = form.Map(v => v.Length);

        result.IsVoid.Should().BeTrue();
    }

    #endregion

    #region Match (Catamorphism)

    [Fact]
    public void Match_Marked_CallsWhenMarked()
    {
        var form = Form<int>.Mark(42);

        var result = form.Match(
            whenMarked: v => $"marked:{v}",
            whenVoid: () => "void");

        result.Should().Be("marked:42");
    }

    [Fact]
    public void Match_Void_CallsWhenVoid()
    {
        var form = Form<int>.Void();

        var result = form.Match(
            whenMarked: v => $"marked:{v}",
            whenVoid: () => "void");

        result.Should().Be("void");
    }

    [Fact]
    public void Match_MarkedWithNullValue_CallsWhenVoid()
    {
        var form = Form<string>.Mark(null!);

        var result = form.Match(
            whenMarked: v => $"marked:{v}",
            whenVoid: () => "void");

        result.Should().Be("void");
    }

    #endregion

    #region GetValueOrDefault

    [Fact]
    public void GetValueOrDefault_Marked_ReturnsValue()
    {
        var form = Form<int>.Mark(42);

        form.GetValueOrDefault(0).Should().Be(42);
    }

    [Fact]
    public void GetValueOrDefault_Void_ReturnsDefault()
    {
        var form = Form<int>.Void();

        form.GetValueOrDefault(99).Should().Be(99);
    }

    [Fact]
    public void GetValueOrDefault_MarkedWithNullValue_ReturnsDefault()
    {
        var form = Form<string>.Mark(null!);

        form.GetValueOrDefault("default").Should().Be("default");
    }

    #endregion

    #region Operators

    [Fact]
    public void BangOperator_CrossesForm()
    {
        var form = Form<int>.Void();

        var crossed = !form;

        crossed.IsMarked.Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_EqualForms_ReturnsTrue()
    {
        var a = Form<int>.Mark(42);
        var b = Form<int>.Mark(42);

        (a == b).Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_DifferentForms_ReturnsTrue()
    {
        var a = Form<int>.Mark(42);
        var b = Form<int>.Void();

        (a != b).Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_EqualForms_ReturnsFalse()
    {
        var a = Form<int>.Mark(42);
        var b = Form<int>.Mark(42);

        (a != b).Should().BeFalse();
    }

    #endregion

    #region Equality

    [Fact]
    public void Equals_SameMarkedForm_ReturnsTrue()
    {
        var a = Form<int>.Mark(42);
        var b = Form<int>.Mark(42);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var a = Form<int>.Mark(42);
        var b = Form<int>.Mark(99);

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_MarkedVsVoid_ReturnsFalse()
    {
        var a = Form<int>.Mark(0);
        var b = Form<int>.Void();

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentDepths_ReturnsFalse()
    {
        var a = Form<int>.Mark(42);
        var b = Form<int>.Mark(42).Cross();

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_Object_ReturnsCorrectResult()
    {
        var form = Form<int>.Mark(42);
        object other = Form<int>.Mark(42);

        form.Equals(other).Should().BeTrue();
    }

    [Fact]
    public void Equals_Object_WrongType_ReturnsFalse()
    {
        var form = Form<int>.Mark(42);
        object other = "not a form";

        form.Equals(other).Should().BeFalse();
    }

    [Fact]
    public void Equals_Object_Null_ReturnsFalse()
    {
        var form = Form<int>.Mark(42);

        form.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_EqualForms_HaveSameHashCode()
    {
        var a = Form<int>.Mark(42);
        var b = Form<int>.Mark(42);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentForms_LikelyDifferentHashCode()
    {
        var a = Form<int>.Mark(42);
        var b = Form<int>.Mark(99);

        // Not guaranteed, but very likely
        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_VoidForms_AreEqual()
    {
        var a = Form<int>.Void();
        var b = Form<int>.Void();

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_Void_ReturnsVoidSymbol()
    {
        var form = Form<int>.Void();

        form.ToString().Should().Be("\u2205"); // ∅
    }

    [Fact]
    public void ToString_MarkedDepth1_ReturnsOneMarkWithValue()
    {
        var form = Form<int>.Mark(42);

        form.ToString().Should().Be("\u22a2[42]"); // ⊢[42]
    }

    [Fact]
    public void ToString_MarkedDepth2_ReturnsTwoMarksWithValue()
    {
        var form = Form<int>.Mark(42).Cross();

        form.ToString().Should().Be("\u22a2\u22a2[42]"); // ⊢⊢[42]
    }

    [Fact]
    public void ToString_MarkedDepth3_ReturnsThreeMarksWithValue()
    {
        var form = Form<int>.Mark(42).Cross().Cross();

        form.ToString().Should().Be("\u22a2\u22a2\u22a2[42]"); // ⊢⊢⊢[42]
    }

    #endregion
}
