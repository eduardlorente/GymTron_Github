using GymTron.Domain.Entities;

namespace GymTron.Domain.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashToken(string token);
    int AccessTokenExpirationMinutes { get; }
    int RefreshTokenExpirationDays { get; }
}
