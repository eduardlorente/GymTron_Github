using GymTron.App.Services.Api.Auth;

namespace GymTron.App.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IAuthApiClient _authApiClient;
    private readonly ITokenStorage _tokenStorage;
    private readonly ISessionManager? _sessionManager;

    public event EventHandler? SessionExpired;

    public AuthService(IAuthApiClient authApiClient, ITokenStorage tokenStorage, ISessionManager? sessionManager = null)
    {
        _authApiClient = authApiClient;
        _tokenStorage = tokenStorage;
        _sessionManager = sessionManager;

        AuthHttpMessageHandler.SessionExpired += (s, e) =>
        {
            _sessionManager?.Lock();
            SessionExpired?.Invoke(this, EventArgs.Empty);
        };
    }

    public async Task<bool> LoginAsync(string identifier, string password, CancellationToken ct = default)
    {
        var result = await _authApiClient.LoginAsync(identifier, password, ct);
        if (result == null || string.IsNullOrWhiteSpace(result.AccessToken))
        {
            return false;
        }

        await _tokenStorage.SaveTokensAsync(result.AccessToken, result.RefreshToken);
        return true;
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        string? refreshToken = await _tokenStorage.GetRefreshTokenAsync();
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _authApiClient.RevokeTokenAsync(refreshToken, ct);
        }

        _sessionManager?.Lock();
        await _tokenStorage.ClearTokensAsync();
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        string? accessToken = await _tokenStorage.GetAccessTokenAsync();
        return !string.IsNullOrWhiteSpace(accessToken);
    }

    public async Task<bool> RegisterAsync(string username, string email, string password, CancellationToken ct = default)
    {
        return await _authApiClient.RegisterAsync(username, email, password, ct);
    }
}
