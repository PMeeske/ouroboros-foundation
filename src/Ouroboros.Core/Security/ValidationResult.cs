namespace Ouroboros.Core.Security;

/// <summary>
/// Result of input validation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets a value indicating whether indicates whether the input is valid.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the sanitized input value if validation succeeded.
    /// </summary>
    public string? SanitizedValue { get; init; }

    /// <summary>
    /// Gets list of validation errors if validation failed.
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns></returns>
    public static ValidationResult Success(string sanitizedValue) =>
        new() { IsValid = true, SanitizedValue = sanitizedValue };

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <returns></returns>
    public static ValidationResult Failure(params string[] errors) =>
        new() { IsValid = false, Errors = errors.ToList() };
}