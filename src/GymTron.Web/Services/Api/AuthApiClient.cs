using System.Net.Http.Json;

namespace GymTron.Web.Services.Api;

public record LoginApiRequest(string Identifier, string Password);
public record RefreshTokenApiRequest(string RefreshToken);

public class AuthApiClient(HttpClient httpClient) : IAuthApiClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<AuthResultDto?> LoginAsync(string identifier, string password, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", new LoginApiRequest(identifier, password), ct);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<AuthResultDto>(cancellationToken: ct);
    }

    public async Task<AuthResultDto?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", new RefreshTokenApiRequest(refreshToken), ct);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<AuthResultDto>(cancellationToken: ct);
    }
}
