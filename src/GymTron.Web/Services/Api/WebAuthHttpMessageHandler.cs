using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GymTron.Web.Services.Api;

public class WebAuthHttpMessageHandler(
    IHttpContextAccessor httpContextAccessor,
    IAuthApiClient authApiClient)
    : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IAuthApiClient _authApiClient = authApiClient;
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        string? accessToken = null;

        if (httpContext != null)
        {
            accessToken = await httpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "access_token");
            if (!string.IsNullOrWhiteSpace(accessToken) && request.Headers.Authorization == null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized && httpContext != null)
        {
            string? newAccessToken = await TryRefreshTokenAsync(httpContext, accessToken, cancellationToken);
            if (!string.IsNullOrWhiteSpace(newAccessToken))
            {
                var retryRequest = await CloneHttpRequestMessageAsync(request);
                retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccessToken);

                response.Dispose();
                return await base.SendAsync(retryRequest, cancellationToken);
            }
        }

        return response;
    }

    private async Task<string?> TryRefreshTokenAsync(HttpContext httpContext, string? originalAccessToken, CancellationToken cancellationToken)
    {
        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            string? currentAccessToken = await httpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "access_token");
            if (!string.IsNullOrWhiteSpace(currentAccessToken) && currentAccessToken != originalAccessToken)
            {
                return currentAccessToken;
            }

            string? refreshToken = await httpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "refresh_token");
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return null;
            }

            var authResult = await _authApiClient.RefreshTokenAsync(refreshToken, cancellationToken);
            if (authResult == null || string.IsNullOrWhiteSpace(authResult.AccessToken))
            {
                return null;
            }

            var authenticateResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (authenticateResult.Succeeded && authenticateResult.Principal != null)
            {
                var properties = authenticateResult.Properties ?? new AuthenticationProperties();
                properties.StoreTokens(
                [
                    new AuthenticationToken { Name = "access_token", Value = authResult.AccessToken },
                    new AuthenticationToken { Name = "refresh_token", Value = authResult.RefreshToken }
                ]);

                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    authenticateResult.Principal,
                    properties);
            }

            return authResult.AccessToken;
        }
        catch
        {
            return null;
        }
        finally
        {
            RefreshLock.Release();
        }
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
