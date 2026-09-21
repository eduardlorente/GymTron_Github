using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GymTron.Domain.Entities;
using GymTron.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GymTron.Infrastructure.Security;

public class JwtTokenService : ITokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly IClock _clock;

    public int AccessTokenExpirationMinutes { get; }
    public int RefreshTokenExpirationDays { get; }

    public JwtTokenService(IConfiguration configuration, IClock clock)
    {
        _clock = clock;
        string? secretKey = configuration["Jwt:SecretKey"];
        if (string.IsNullOrWhiteSpace(secretKey) || Encoding.UTF8.GetByteCount(secretKey) < 32)
        {
            throw new InvalidOperationException(
                "Required configuration 'Jwt:SecretKey' is missing, empty, or shorter than 32 bytes (256 bits).");
        }

        _secretKey = secretKey;
        _issuer = configuration["Jwt:Issuer"] ?? "GymTronApi";
        _audience = configuration["Jwt:Audience"] ?? "GymTronApp";
        AccessTokenExpirationMinutes = int.TryParse(configuration["Jwt:AccessTokenExpirationMinutes"], out int accessExp) ? accessExp : 15;
        RefreshTokenExpirationDays = int.TryParse(configuration["Jwt:RefreshTokenExpirationDays"], out int refreshExp) ? refreshExp : 7;
    }

    public string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = Encoding.UTF8.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            IssuedAt = _clock.UtcNow,
            NotBefore = _clock.UtcNow,
            Expires = _clock.UtcNow.AddMinutes(AccessTokenExpirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    public string HashToken(string token)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
