namespace Ouroboros.Core.Security.Authentication;

/// <summary>
/// Simple in-memory authentication provider for development/testing.
/// </summary>
public class InMemoryAuthenticationProvider : IAuthenticationProvider
{
    private readonly Dictionary<string, (string Password, AuthenticationPrincipal Principal)> users = new();
    private readonly HashSet<string> revokedTokens = new();
    private readonly object @lock = new();

    /// <summary>
    /// Registers a user.
    /// </summary>
    public void RegisterUser(string username, string password, AuthenticationPrincipal principal)
    {
        lock (this.@lock)
        {
            this.users[username] = (password, principal);
        }
    }

    /// <summary>
    /// Authenticates a user with username and password.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public Task<AuthenticationResult> AuthenticateAsync(string username, string password, CancellationToken ct = default)
    {
        lock (this.@lock)
        {
            if (!this.users.TryGetValue(username, out (string Password, AuthenticationPrincipal Principal) user))
            {
                return Task.FromResult(AuthenticationResult.Failure("Invalid username or password"));
            }

            if (user.Password != password)
            {
                return Task.FromResult(AuthenticationResult.Failure("Invalid username or password"));
            }

            // Generate a simple token (in production, use JWT)
            string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            return Task.FromResult(AuthenticationResult.Success(user.Principal, token));
        }
    }

    /// <summary>
    /// Validates an authentication token.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public Task<AuthenticationResult> ValidateTokenAsync(string token, CancellationToken ct = default)
    {
        lock (this.@lock)
        {
            if (this.revokedTokens.Contains(token))
            {
                return Task.FromResult(AuthenticationResult.Failure("Token has been revoked"));
            }

            // In a real implementation, decode the token and extract the principal
            // For now, return a dummy principal
            return Task.FromResult(AuthenticationResult.Failure("Token validation not fully implemented"));
        }
    }

    /// <summary>
    /// Refreshes an authentication token.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public Task<AuthenticationResult> RefreshTokenAsync(string token, CancellationToken ct = default)
    {
        return Task.FromResult(AuthenticationResult.Failure("Token refresh not implemented"));
    }

    /// <summary>
    /// Revokes an authentication token.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public Task<bool> RevokeTokenAsync(string token, CancellationToken ct = default)
    {
        lock (this.@lock)
        {
            this.revokedTokens.Add(token);
            return Task.FromResult(true);
        }
    }
}