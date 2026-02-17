namespace Ouroboros.Core.Security;

/// <summary>
/// Options for input validation.
/// </summary>
public class ValidationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether check for injection patterns (SQL, command, script).
    /// </summary>
    public bool CheckInjectionPatterns { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether check for dangerous characters.
    /// </summary>
    public bool CheckDangerousCharacters { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether remove control characters during sanitization.
    /// </summary>
    public bool RemoveControlCharacters { get; set; } = true;

    /// <summary>
    /// Gets default validation options.
    /// </summary>
    public static ValidationOptions Default => new();

    /// <summary>
    /// Gets lenient validation options (fewer checks).
    /// </summary>
    public static ValidationOptions Lenient => new()
    {
        CheckInjectionPatterns = false,
        CheckDangerousCharacters = true,
    };
}