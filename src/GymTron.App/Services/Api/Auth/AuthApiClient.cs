using System.Net;
using System.Net.Http.Json;
using GymTron.App.Services.Api.Models;

namespace GymTron.App.Services.Api.Auth;

public class AuthApiClient(HttpClient httpClient) : IAuthApiClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<AuthResultDto?> LoginAsync(string identifier, string password, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", new LoginRequest(identifier, password), ct);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AuthResultDto>(cancellationToken: ct);
    }

    public async Task<AuthResultDto?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", new RefreshTokenRequest(refreshToken), ct);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AuthResultDto>(cancellationToken: ct);
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        await _httpClient.PostAsJsonAsync("api/auth/revoke", new RevokeTokenRequest(refreshToken), ct);
    }

    public async Task<bool> RegisterAsync(string username, string email, string password, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", new RegisterRequest(username, email, password), ct);
        return response.IsSuccessStatusCode;
    }
}
