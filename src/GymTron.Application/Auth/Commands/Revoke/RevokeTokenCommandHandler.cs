using GymTron.Application.Base;
using GymTron.Domain.Entities;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Auth.Commands.Revoke;

internal class RevokeTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    ITokenService tokenService,
    IClock clock,
    IExceptionLogger<RevokeTokenCommand> logger)
    : BaseCommandHandler<RevokeTokenCommand>(logger)
{
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IClock _clock = clock;

    protected override async Task HandleCommand(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        string tokenHash = _tokenService.HashToken(request.RefreshToken);
        RefreshToken? storedToken = await _refreshTokenRepository.GetByTokenHash(tokenHash, cancellationToken);

        if (storedToken != null && storedToken.IsActive(_clock.UtcNow))
        {
            storedToken.Revoke(_clock.UtcNow);
            await _refreshTokenRepository.Update(storedToken, cancellationToken);
        }
    }
}
