namespace Ouroboros.Tools;

/// <summary>
/// Represents DSL validation result.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; }
    public string[] Errors { get; }
    public string[] Suggestions { get; }

    public ValidationResult(bool isValid, string[] errors, string[] suggestions)
    {
        IsValid = isValid;
        Errors = errors;
        Suggestions = suggestions;
    }
}