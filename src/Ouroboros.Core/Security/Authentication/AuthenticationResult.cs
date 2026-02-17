namespace Ouroboros.Core.Security.Authentication;

/// <summary>
/// Result of an authentication attempt.
/// </summary>
public class AuthenticationResult
{
    /// <summary>
    /// Gets a value indicating whether whether authentication was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Gets authenticated principal (if successful).
    /// </summary>
    public AuthenticationPrincipal? Principal { get; init; }

    /// <summary>
    /// Gets authentication token (e.g., JWT).
    /// </summary>
    public string? Token { get; init; }

    /// <summary>
    /// Gets error message (if failed).
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a successful authentication result.
    /// </summary>
    /// <returns></returns>
    public static AuthenticationResult Success(AuthenticationPrincipal principal, string token) =>
        new()
        {
            IsSuccess = true,
            Principal = principal,
            Token = token,
        };

    /// <summary>
    /// Creates a failed authentication result.
    /// </summary>
    /// <returns></returns>
    public static AuthenticationResult Failure(string errorMessage) =>
        new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
        };
}