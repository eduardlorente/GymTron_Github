using GymTron.Application.Auth.Models;
using GymTron.Application.Base;
using GymTron.Domain.Entities;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;

namespace GymTron.Application.Auth.Commands.Login;

internal class LoginCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IClock clock,
    IExceptionLogger<LoginCommand> logger)
    : BaseCommandHandlerWithResponse<LoginCommand, AuthResult>(logger)
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IClock _clock = clock;

    protected override async Task<AuthResult> HandleCommand(LoginCommand request, CancellationToken cancellationToken)
    {
        User? user = await _userRepository.GetByUsernameOrEmail(request.Identifier, cancellationToken);
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username/email or password.");
        }

        if (!user.Status.IsActive)
        {
            throw new UnauthorizedAccessException("User account is inactive or disabled.");
        }

        string accessToken = _tokenService.GenerateAccessToken(user);
        string rawRefreshToken = _tokenService.GenerateRefreshToken();
        string tokenHash = _tokenService.HashToken(rawRefreshToken);

        DateTime expiresAt = _clock.UtcNow.AddDays(_tokenService.RefreshTokenExpirationDays);
        RefreshToken refreshToken = RefreshToken.New(user.Id, tokenHash, expiresAt, _clock);

        await _refreshTokenRepository.Add(refreshToken, cancellationToken);

        return new AuthResult(
            accessToken,
            rawRefreshToken,
            _tokenService.AccessTokenExpirationMinutes * 60);
    }
}
