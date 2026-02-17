namespace Ouroboros.Core.Interop;

/// <summary>
/// Enhanced step examples integrating with our monadic operations
/// </summary>
public static class EnhancedSteps
{
    /// <summary>
    /// Example: Step that uppercases text
    /// </summary>
    public static readonly Step<string, string> Upper = async s =>
    {
        await Task.Yield();
        return s.ToUpperInvariant();
    };

    /// <summary>
    /// Example: Step that gets string length
    /// </summary>
    public static readonly Step<string, int> Length = async s =>
    {
        await Task.Yield();
        return s.Length;
    };

    /// <summary>
    /// Example: Step that formats number
    /// </summary>
    public static readonly Step<int, string> Show = async n =>
    {
        await Task.Yield();
        return $"length={n}";
    };

    /// <summary>
    /// Example: KleisliResult that safely parses integers
    /// </summary>
    public static readonly KleisliResult<string, int, string> SafeParse = async s =>
    {
        await Task.Yield();
        return int.TryParse(s, out int result)
            ? Result<int, string>.Success(result)
            : Result<int, string>.Failure($"Cannot parse '{s}' as integer");
    };

    /// <summary>
    /// Example: KleisliOption that returns value if positive
    /// </summary>
    public static readonly KleisliOption<int, int> OnlyPositive = async n =>
    {
        await Task.Yield();
        return n > 0 ? Option<int>.Some(n) : Option<int>.None();
    };
}