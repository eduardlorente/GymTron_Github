using GymTron.App.Services.Api.Models;

namespace GymTron.App.Services.Api.Auth;

public interface IAuthApiClient
{
    Task<AuthResultDto?> LoginAsync(string identifier, string password, CancellationToken ct = default);
    Task<AuthResultDto?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<bool> RegisterAsync(string username, string email, string password, CancellationToken ct = default);
}
