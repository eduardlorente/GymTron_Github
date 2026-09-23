using GymTron.Domain.Entities;
using GymTron.Infrastructure.Security;
using GymTron.UnitTests.Helpers;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace GymTron.UnitTests.Infrastructure;

public class SecurityServiceTests
{
    [Fact]
    public void PasswordHasher_HashAndVerify_SucceedsForMatchingPassword()
    {
        var hasher = new PasswordHasherService();
        string password = "MySuperSecretPassword#2026";

        string hash = hasher.HashPassword(password);
        bool isValid = hasher.VerifyPassword(password, hash);

        Assert.NotEmpty(hash);
        Assert.True(isValid);
    }

    [Fact]
    public void PasswordHasher_Verify_SucceedsForKnownTestUserHash()
    {
        var hasher = new PasswordHasherService();
        const string knownHash = "GkRdnMxtr1jvD87kEmZPfw==:wRrYCQAK2CyLUFDru+eXqIK1QBi5sOXhTvuP1AhWIBc=:100000:SHA256";
        Assert.True(hasher.VerifyPassword("password", knownHash));
    }

    [Fact]
    public void PasswordHasher_Verify_FailsForWrongPassword()
    {
        var hasher = new PasswordHasherService();
        string password = "MySuperSecretPassword#2026";

        string hash = hasher.HashPassword(password);
        bool isValid = hasher.VerifyPassword("WrongPassword123", hash);

        Assert.False(isValid);
    }

    [Fact]
    public void PasswordHasher_Verify_ReturnsFalseForCorruptedHash()
    {
        var hasher = new PasswordHasherService();

        bool isValid = hasher.VerifyPassword("any", "corrupted.hash.format");

        Assert.False(isValid);
    }

    [Fact]
    public void JwtTokenService_GeneratesValidAccessTokenAndRefreshToken()
    {
        var configuration = NSubstitute.Substitute.For<IConfiguration>();
        configuration["Jwt:SecretKey"].Returns("VeryLongSecureSecretKeyForTestingJwtTokenGeneration12345!");
        configuration["Jwt:Issuer"].Returns("GymTronApiTest");
        configuration["Jwt:Audience"].Returns("GymTronAppTest");
        configuration["Jwt:AccessTokenExpirationMinutes"].Returns("20");
        configuration["Jwt:RefreshTokenExpirationDays"].Returns("14");

        var clock = new FakeClock(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        var tokenService = new JwtTokenService(configuration, clock);

        var user = User.FromDatabase(99, "testuser", "test@gymtron.com", "dummy_hash", true, clock.UtcNow);

        string accessToken = tokenService.GenerateAccessToken(user);
        string refreshToken = tokenService.GenerateRefreshToken();
        string tokenHash = tokenService.HashToken(refreshToken);

        Assert.NotEmpty(accessToken);
        Assert.NotEmpty(refreshToken);
        Assert.NotEmpty(tokenHash);
        Assert.Equal(64, tokenHash.Length); // SHA-256 hex string length
        Assert.Equal(20, tokenService.AccessTokenExpirationMinutes);
        Assert.Equal(14, tokenService.RefreshTokenExpirationDays);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void JwtTokenService_Throws_WhenSecretKeyIsMissingOrWhitespace(string? key)
    {
        var configuration = NSubstitute.Substitute.For<IConfiguration>();
        configuration["Jwt:SecretKey"].Returns(key);
        var clock = new FakeClock(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));

        Assert.Throws<InvalidOperationException>(() => new JwtTokenService(configuration, clock));
    }

    [Fact]
    public void JwtTokenService_Throws_WhenSecretKeyIsShorterThan32Bytes()
    {
        var configuration = NSubstitute.Substitute.For<IConfiguration>();
        configuration["Jwt:SecretKey"].Returns("TooShortKeyUnder32Bytes!");
        var clock = new FakeClock(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));

        Assert.Throws<InvalidOperationException>(() => new JwtTokenService(configuration, clock));
    }
}
