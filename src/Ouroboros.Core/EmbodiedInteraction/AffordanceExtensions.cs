using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Extension methods for affordances.
/// </summary>
[ExcludeFromCodeCoverage]
public static class AffordanceExtensions
{
    /// <summary>
    /// Functional let binding.
    /// </summary>
    public static TResult Let<T, TResult>(this T value, Func<T, TResult> func) => func(value);
}
