namespace Ouroboros.Core.Security.Authentication;

/// <summary>
/// Interface for authentication providers.
/// </summary>
public interface IAuthenticationProvider
{
    /// <summary>
    /// Authenticates a user with username and password.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<AuthenticationResult> AuthenticateAsync(string username, string password, CancellationToken ct = default);

    /// <summary>
    /// Validates an authentication token.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<AuthenticationResult> ValidateTokenAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Refreshes an authentication token.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<AuthenticationResult> RefreshTokenAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Revokes an authentication token.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> RevokeTokenAsync(string token, CancellationToken ct = default);
}