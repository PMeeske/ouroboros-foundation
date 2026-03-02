namespace Ouroboros.Tools;

/// <summary>
/// Represents DSL validation result.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the DSL input passed validation.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the list of validation error messages, if any.
    /// </summary>
    public string[] Errors { get; }

    /// <summary>
    /// Gets the list of suggested corrections or improvements.
    /// </summary>
    public string[] Suggestions { get; }

    public ValidationResult(bool isValid, string[] errors, string[] suggestions)
    {
        IsValid = isValid;
        Errors = errors;
        Suggestions = suggestions;
    }
}