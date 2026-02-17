namespace Ouroboros.Domain.SelfModification;

/// <summary>
/// String extension methods.
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    /// Truncates a string to the specified length.
    /// </summary>
    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..(maxLength - 3)] + "...";
    }
}