namespace GymTron.App.Services.Api.Models;

public record LoginRequest(string Identifier, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record RevokeTokenRequest(string RefreshToken);

public record RegisterRequest(string Username, string Email, string Password);

public record AuthResultDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    string TokenType);
