namespace GymTron.Application.Auth.Models;

public record AuthResult(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds,
    string TokenType = "Bearer");
