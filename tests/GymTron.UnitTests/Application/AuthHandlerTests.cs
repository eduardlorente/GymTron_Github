using GymTron.Application.Auth.Commands.Login;
using GymTron.Application.Auth.Commands.Refresh;
using GymTron.Application.Auth.Commands.Register;
using GymTron.Application.Auth.Commands.Revoke;
using GymTron.Application.Auth.Models;
using GymTron.Domain.Entities;
using GymTron.Domain.Exceptions;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;
using GymTron.UnitTests.Helpers;
using NSubstitute;

namespace GymTron.UnitTests.Application;

public class AuthHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly FakeClock _clock = new(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));

    public AuthHandlerTests()
    {
        _tokenService.AccessTokenExpirationMinutes.Returns(15);
        _tokenService.RefreshTokenExpirationDays.Returns(7);
        _tokenService.HashToken(Arg.Any<string>()).Returns(call => "hash_" + call.Arg<string>());
    }

    [Fact]
    public async Task Register_WhenUserAlreadyExists_ThrowsInvalidDomainOperationException()
    {
        _userRepository.ExistsByUsernameOrEmail("existingUser", "user@gymtron.com", Arg.Any<CancellationToken>())
            .Returns(true);
        var logger = Substitute.For<IExceptionLogger<RegisterUserCommand>>();
        var handler = new RegisterUserCommandHandler(_userRepository, _passwordHasher, _clock, logger);
        var command = new RegisterUserCommand(Guid.NewGuid(), "existingUser", "user@gymtron.com", "SecretPass123");

        await Assert.ThrowsAsync<InvalidDomainOperationException>(() => handler.Handle(command, CancellationToken.None));
        await _userRepository.DidNotReceive().Add(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Register_WhenValid_HashesPasswordAndPersistsUser()
    {
        _userRepository.ExistsByUsernameOrEmail("newUser", "new@gymtron.com", Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.HashPassword("SecretPass123").Returns("hashed_secret");
        _userRepository.Add(Arg.Any<User>(), Arg.Any<CancellationToken>()).Returns(42);

        var logger = Substitute.For<IExceptionLogger<RegisterUserCommand>>();
        var handler = new RegisterUserCommandHandler(_userRepository, _passwordHasher, _clock, logger);
        var command = new RegisterUserCommand(Guid.NewGuid(), "newUser", "new@gymtron.com", "SecretPass123");

        int userId = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(42, userId);
        await _userRepository.Received(1).Add(Arg.Is<User>(u => u.Username == "newUser" && u.PasswordHash == "hashed_secret"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Login_WhenCredentialsValid_ReturnsAuthResultAndSavesRefreshToken()
    {
        var user = User.FromDatabase(1, "tester", "tester@gymtron.com", "hashed_pwd", true, _clock.UtcNow);
        _userRepository.GetByUsernameOrEmail("tester", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword("correct_password", "hashed_pwd").Returns(true);
        _tokenService.GenerateAccessToken(user).Returns("jwt_access_token");
        _tokenService.GenerateRefreshToken().Returns("raw_refresh_token");

        var logger = Substitute.For<IExceptionLogger<LoginCommand>>();
        var handler = new LoginCommandHandler(_userRepository, _refreshTokenRepository, _passwordHasher, _tokenService, _clock, logger);
        var command = new LoginCommand(Guid.NewGuid(), "tester", "correct_password");

        AuthResult result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("jwt_access_token", result.AccessToken);
        Assert.Equal("raw_refresh_token", result.RefreshToken);
        Assert.Equal(15 * 60, result.ExpiresInSeconds);
        await _refreshTokenRepository.Received(1).Add(Arg.Is<RefreshToken>(rt => rt.UserId == 1), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Login_WhenPasswordInvalid_ThrowsUnauthorizedAccessException()
    {
        var user = User.FromDatabase(1, "tester", "tester@gymtron.com", "hashed_pwd", true, _clock.UtcNow);
        _userRepository.GetByUsernameOrEmail("tester", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyPassword("wrong_password", "hashed_pwd").Returns(false);

        var logger = Substitute.For<IExceptionLogger<LoginCommand>>();
        var handler = new LoginCommandHandler(_userRepository, _refreshTokenRepository, _passwordHasher, _tokenService, _clock, logger);
        var command = new LoginCommand(Guid.NewGuid(), "tester", "wrong_password");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
        await _refreshTokenRepository.DidNotReceive().Add(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshToken_WhenValid_RotatesTokenAndReturnsNewPair()
    {
        DateTime expiresAt = _clock.UtcNow.AddDays(7);
        var storedToken = RefreshToken.FromDatabase(10, 1, "hash_valid_token", expiresAt, null, null, _clock.UtcNow);
        _refreshTokenRepository.GetByTokenHash("hash_valid_token", Arg.Any<CancellationToken>()).Returns(storedToken);

        var user = User.FromDatabase(1, "tester", "tester@gymtron.com", "pwd", true, _clock.UtcNow);
        _userRepository.GetById(1, Arg.Any<CancellationToken>()).Returns(user);

        _tokenService.GenerateAccessToken(user).Returns("new_access_token");
        _tokenService.GenerateRefreshToken().Returns("new_refresh_token");

        var logger = Substitute.For<IExceptionLogger<RefreshTokenCommand>>();
        var handler = new RefreshTokenCommandHandler(_userRepository, _refreshTokenRepository, _tokenService, _clock, logger);
        var command = new RefreshTokenCommand(Guid.NewGuid(), "valid_token");

        AuthResult result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("new_access_token", result.AccessToken);
        Assert.Equal("new_refresh_token", result.RefreshToken);
        Assert.True(storedToken.IsRevoked);
        Assert.Equal("hash_new_refresh_token", storedToken.ReplacedByTokenHash);

        await _refreshTokenRepository.Received(1).Update(storedToken, Arg.Any<CancellationToken>());
        await _refreshTokenRepository.Received(1).Add(Arg.Is<RefreshToken>(rt => rt.TokenHash == "hash_new_refresh_token"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshToken_WhenRevokedTokenReused_TerminatesAllSessionsAndThrows()
    {
        DateTime expiresAt = _clock.UtcNow.AddDays(7);
        var revokedToken = RefreshToken.FromDatabase(10, 1, "hash_stolen_token", expiresAt, _clock.UtcNow.AddMinutes(-5), "replaced_hash", _clock.UtcNow);
        _refreshTokenRepository.GetByTokenHash("hash_stolen_token", Arg.Any<CancellationToken>()).Returns(revokedToken);

        var logger = Substitute.For<IExceptionLogger<RefreshTokenCommand>>();
        var handler = new RefreshTokenCommandHandler(_userRepository, _refreshTokenRepository, _tokenService, _clock, logger);
        var command = new RefreshTokenCommand(Guid.NewGuid(), "stolen_token");

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("reuse detected", ex.Message, StringComparison.OrdinalIgnoreCase);

        // Crucial security behavior: all tokens for user revoked
        await _refreshTokenRepository.Received(1).RevokeAllForUser(1, _clock.UtcNow, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RefreshToken_WhenExpired_ThrowsUnauthorizedAccessException()
    {
        DateTime expiredAt = _clock.UtcNow.AddMinutes(-1);
        var expiredToken = RefreshToken.FromDatabase(10, 1, "hash_expired_token", expiredAt, null, null, _clock.UtcNow);
        _refreshTokenRepository.GetByTokenHash("hash_expired_token", Arg.Any<CancellationToken>()).Returns(expiredToken);

        var logger = Substitute.For<IExceptionLogger<RefreshTokenCommand>>();
        var handler = new RefreshTokenCommandHandler(_userRepository, _refreshTokenRepository, _tokenService, _clock, logger);
        var command = new RefreshTokenCommand(Guid.NewGuid(), "expired_token");

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("expired", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Revoke_WhenActive_RevokesToken()
    {
        DateTime expiresAt = _clock.UtcNow.AddDays(7);
        var activeToken = RefreshToken.FromDatabase(10, 1, "hash_token_to_revoke", expiresAt, null, null, _clock.UtcNow);
        _refreshTokenRepository.GetByTokenHash("hash_token_to_revoke", Arg.Any<CancellationToken>()).Returns(activeToken);

        var logger = Substitute.For<IExceptionLogger<RevokeTokenCommand>>();
        var handler = new RevokeTokenCommandHandler(_refreshTokenRepository, _tokenService, _clock, logger);
        var command = new RevokeTokenCommand(Guid.NewGuid(), "token_to_revoke");

        await handler.Handle(command, CancellationToken.None);

        Assert.True(activeToken.IsRevoked);
        await _refreshTokenRepository.Received(1).Update(activeToken, Arg.Any<CancellationToken>());
    }
}
