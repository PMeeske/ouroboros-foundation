namespace Ouroboros.Core.Vectors;

/// <summary>
/// Extension methods for thought vector operations.
/// </summary>
public static class ThoughtVectorExtensions
{
    /// <summary>
    /// Convolves this thought with another to create a combined representation.
    /// </summary>
    public static float[] ConvolveWith(this float[] thought, float[] other)
        => VectorConvolution.CircularConvolve(thought, other);

    /// <summary>
    /// Expands this thought to a higher dimension.
    /// </summary>
    public static float[] ExpandTo(this float[] thought, int targetDimension, int seed = 42)
        => VectorConvolution.ExpandDimension(thought, targetDimension, seed);

    /// <summary>
    /// Creates a meta-thought combining this with other thoughts.
    /// </summary>
    public static float[] CombineWith(this float[] thought, params float[][] others)
        => VectorConvolution.CreateMetaThought(new[] { thought }.Concat(others));

    /// <summary>
    /// Computes the gradient from this thought to another.
    /// </summary>
    public static float[] GradientTo(this float[] thought, float[] target)
        => VectorConvolution.ThoughtGradient(thought, target);

    /// <summary>
    /// Amplifies patterns through self-resonance.
    /// </summary>
    public static float[] Resonate(this float[] thought, int iterations = 3)
        => VectorConvolution.ThoughtResonance(thought, iterations);

    /// <summary>
    /// Binds this role vector with a filler using holographic representation.
    /// </summary>
    public static float[] Bind(this float[] role, float[] filler)
        => VectorConvolution.HolographicBind(role, filler);

    /// <summary>
    /// Unbinds using this as the role to retrieve the filler.
    /// </summary>
    public static float[] Unbind(this float[] role, float[] bound)
        => VectorConvolution.HolographicUnbind(bound, role);
}