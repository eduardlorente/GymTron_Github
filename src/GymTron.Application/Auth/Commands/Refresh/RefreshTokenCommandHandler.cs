using GymTron.Application.Auth.Models;
using GymTron.Application.Base;
using GymTron.Domain.Entities;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Auth.Commands.Refresh;

internal class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenService tokenService,
    IClock clock,
    IExceptionLogger<RefreshTokenCommand> logger)
    : BaseCommandHandlerWithResponse<RefreshTokenCommand, AuthResult>(logger)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IClock _clock = clock;

    protected override async Task<AuthResult> HandleCommand(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        string tokenHash = _tokenService.HashToken(request.RefreshToken);
        RefreshToken? storedToken = await _refreshTokenRepository.GetByTokenHash(tokenHash, cancellationToken);

        if (storedToken == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        DateTime now = _clock.UtcNow;

        if (storedToken.IsRevoked)
        {
            // Possible token compromise/replay attack: invalidate all tokens for this user family
            await _refreshTokenRepository.RevokeAllForUser(storedToken.UserId, now, cancellationToken);
            throw new UnauthorizedAccessException("Revoked token reuse detected. All active sessions have been terminated.");
        }

        if (storedToken.IsExpired(now))
        {
            throw new UnauthorizedAccessException("Refresh token has expired. Please log in again.");
        }

        User? user = await _userRepository.GetById(storedToken.UserId, cancellationToken);
        if (user == null || !user.Status.IsActive)
        {
            throw new UnauthorizedAccessException("User not found or account is inactive.");
        }

        // Generate new token pair
        string newAccessToken = _tokenService.GenerateAccessToken(user);
        string newRawRefreshToken = _tokenService.GenerateRefreshToken();
        string newTokenHash = _tokenService.HashToken(newRawRefreshToken);

        // Rotate: revoke the current token and record its replacement
        storedToken.Revoke(now, newTokenHash);
        await _refreshTokenRepository.Update(storedToken, cancellationToken);

        // Save new refresh token
        DateTime newExpiresAt = now.AddDays(_tokenService.RefreshTokenExpirationDays);
        RefreshToken newRefreshToken = RefreshToken.New(user.Id, newTokenHash, newExpiresAt, _clock);
        await _refreshTokenRepository.Add(newRefreshToken, cancellationToken);

        return new AuthResult(
            newAccessToken,
            newRawRefreshToken,
            _tokenService.AccessTokenExpirationMinutes * 60);
    }
}
