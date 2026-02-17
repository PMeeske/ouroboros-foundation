namespace Ouroboros.Core.Hyperon.Parsing;

/// <summary>
/// Exception thrown during parsing.
/// </summary>
public sealed class ParseException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParseException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ParseException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ParseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}