using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace GymTron.App.Services.Auth;

public class SecureTokenStorage(ILogger<SecureTokenStorage>? logger = null) : ITokenStorage
{
    private const string AccessTokenKey = "gymtron_access_token";
    private const string RefreshTokenKey = "gymtron_refresh_token";
    private readonly ILogger<SecureTokenStorage>? _logger = logger;

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(AccessTokenKey);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to retrieve access token from SecureStorage.");
            return null;
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(RefreshTokenKey);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to retrieve refresh token from SecureStorage.");
            return null;
        }
    }

    public async Task SaveTokensAsync(string accessToken, string refreshToken)
    {
        try
        {
            await SecureStorage.Default.SetAsync(AccessTokenKey, accessToken);
            await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to save authentication tokens into SecureStorage.", ex);
        }
    }

    public Task ClearTokensAsync()
    {
        try
        {
            SecureStorage.Default.Remove(AccessTokenKey);
            SecureStorage.Default.Remove(RefreshTokenKey);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to clear tokens from SecureStorage.");
        }

        return Task.CompletedTask;
    }
}
