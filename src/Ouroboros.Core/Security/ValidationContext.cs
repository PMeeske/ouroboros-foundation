namespace Ouroboros.Core.Security;

/// <summary>
/// Validation context specifying rules for input validation.
/// </summary>
public class ValidationContext
{
    /// <summary>
    /// Gets or sets maximum allowed length of input.
    /// </summary>
    public int MaxLength { get; set; } = 10_000;

    /// <summary>
    /// Gets or sets minimum required length of input.
    /// </summary>
    public int MinLength { get; set; } = 0;

    /// <summary>
    /// Gets or sets a value indicating whether allow empty input.
    /// </summary>
    public bool AllowEmpty { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether trim leading and trailing whitespace.
    /// </summary>
    public bool TrimWhitespace { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether normalize line endings to LF.
    /// </summary>
    public bool NormalizeLineEndings { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether escape HTML characters.
    /// </summary>
    public bool EscapeHtml { get; set; } = false;

    /// <summary>
    /// Gets or sets characters that are explicitly blocked.
    /// </summary>
    public HashSet<char>? BlockedCharacters { get; set; }

    /// <summary>
    /// Gets default validation context for general text input.
    /// </summary>
    public static ValidationContext Default => new();

    /// <summary>
    /// Gets strict validation context for sensitive operations.
    /// </summary>
    public static ValidationContext Strict => new()
    {
        MaxLength = 1000,
        EscapeHtml = true,
        BlockedCharacters = new HashSet<char> { '<', '>', '&', '"', '\'' },
    };

    /// <summary>
    /// Gets validation context for tool parameters.
    /// </summary>
    public static ValidationContext ToolParameter => new()
    {
        MaxLength = 5000,
        TrimWhitespace = true,
        NormalizeLineEndings = true,
    };
}