using System.Net;
using System.Net.Http.Headers;
using GymTron.App.Services.Auth;

namespace GymTron.App.Services.Api.Auth;

public class AuthHttpMessageHandler(
    ITokenStorage tokenStorage,
    IAuthApiClient authApiClient)
    : DelegatingHandler
{
    private readonly ITokenStorage _tokenStorage = tokenStorage;
    private readonly IAuthApiClient _authApiClient = authApiClient;
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    public static event EventHandler? SessionExpired;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? accessToken = await _tokenStorage.GetAccessTokenAsync();

        if (!string.IsNullOrWhiteSpace(accessToken) && request.Headers.Authorization == null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            string? newAccessToken = await TryRefreshTokenAsync(accessToken, cancellationToken);

            if (!string.IsNullOrWhiteSpace(newAccessToken))
            {
                // Re-send original request with the new access token
                HttpRequestMessage retryRequest = await CloneHttpRequestMessageAsync(request);
                retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);

                response.Dispose();
                return await base.SendAsync(retryRequest, cancellationToken);
            }
        }

        return response;
    }

    private async Task<string?> TryRefreshTokenAsync(string? originalAccessToken, CancellationToken cancellationToken)
    {
        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            // Check if another concurrent request already refreshed the token while we were waiting
            string? currentAccessToken = await _tokenStorage.GetAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(currentAccessToken) && currentAccessToken != originalAccessToken)
            {
                return currentAccessToken;
            }

            string? refreshToken = await _tokenStorage.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                await HandleSessionExpiredAsync();
                return null;
            }

            var authResult = await _authApiClient.RefreshTokenAsync(refreshToken, cancellationToken);
            if (authResult == null || string.IsNullOrWhiteSpace(authResult.AccessToken))
            {
                await HandleSessionExpiredAsync();
                return null;
            }

            await _tokenStorage.SaveTokensAsync(authResult.AccessToken, authResult.RefreshToken);
            return authResult.AccessToken;
        }
        catch
        {
            await HandleSessionExpiredAsync();
            return null;
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    private async Task HandleSessionExpiredAsync()
    {
        await _tokenStorage.ClearTokensAsync();
        SessionExpired?.Invoke(null, EventArgs.Empty);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (Application.Current is App app)
            {
                app.SwitchToLogin();
            }
        });
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
    {
        var clone = new HttpRequestMessage(req.Method, req.RequestUri)
        {
            Version = req.Version
        };

        foreach (var prop in req.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(prop.Key), prop.Value);
        }

        foreach (var header in req.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (req.Content != null)
        {
            var ms = new MemoryStream();
            await req.Content.CopyToAsync(ms);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            foreach (var header in req.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }
}
