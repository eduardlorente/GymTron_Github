namespace GymTron.Web.Services.Api;

public record AuthResultDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    string TokenType);

public interface IAuthApiClient
{
    Task<AuthResultDto?> LoginAsync(string identifier, string password, CancellationToken ct = default);
    Task<AuthResultDto?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}
