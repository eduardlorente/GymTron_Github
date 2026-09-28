using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Domain.Services;
using NSubstitute;

namespace GymTron.UnitTests.Domain;

public class UserTests
{
    private readonly IClock _clock = Substitute.For<IClock>();

    public UserTests()
    {
        _clock.UtcNow.Returns(new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void UserTypes_EnumValues_MatchExpectedDimensions()
    {
        Assert.Equal(0, (int)UserTypes.Undefined);
        Assert.Equal(1, (int)UserTypes.Standard);
        Assert.Equal(2, (int)UserTypes.Administrator);
    }

    [Fact]
    public void User_New_WithDefaultTypeId_CreatesStandardUser()
    {
        var user = User.New("testuser", "test@gymtron.local", "hash123", _clock);

        Assert.Equal(0, user.Id);
        Assert.Equal("testuser", user.Username);
        Assert.Equal("test@gymtron.local", user.Email);
        Assert.Equal("hash123", user.PasswordHash);
        Assert.Equal(UserTypes.Standard, user.TypeId);
        Assert.True(user.Status.IsActive);
    }

    [Fact]
    public void User_New_WithExplicitTypeId_CreatesAdministratorUser()
    {
        var user = User.New("admin", "admin@gymtron.local", "hash123", UserTypes.Administrator, _clock);

        Assert.Equal(UserTypes.Administrator, user.TypeId);
    }

    [Fact]
    public void User_FromDatabase_RehydratesPropertiesCorrectly()
    {
        var created = new DateTime(2025, 5, 10, 8, 30, 0, DateTimeKind.Utc);
        var user = User.FromDatabase(42, "admin", "admin@gymtron.local", "hash123", UserTypes.Administrator, true, created);

        Assert.Equal(42, user.Id);
        Assert.Equal("admin", user.Username);
        Assert.Equal("admin@gymtron.local", user.Email);
        Assert.Equal("hash123", user.PasswordHash);
        Assert.Equal(UserTypes.Administrator, user.TypeId);
        Assert.True(user.Status.IsActive);
        Assert.Equal(created, user.Status.CreatedOn);
    }

    [Fact]
    public void User_UpdateDetails_UpdatesFieldsAndModifiedOn()
    {
        var user = User.New("oldname", "old@gymtron.local", "hash", UserTypes.Standard, _clock);

        _clock.UtcNow.Returns(new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc));
        user.UpdateDetails("newname", "new@gymtron.local", UserTypes.Administrator, _clock);

        Assert.Equal("newname", user.Username);
        Assert.Equal("new@gymtron.local", user.Email);
        Assert.Equal(UserTypes.Administrator, user.TypeId);
        Assert.Equal(new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc), user.Status.ModifiedOn);
    }

    [Fact]
    public void User_DeactivateAndActivate_TransitionsStatusCorrectly()
    {
        var user = User.New("user", "user@gymtron.local", "hash", UserTypes.Standard, _clock);
        Assert.True(user.Status.IsActive);

        _clock.UtcNow.Returns(new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc));
        user.Deactivate(_clock);
        Assert.False(user.Status.IsActive);
        Assert.Equal(new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc), user.Status.DeletedOn);

        _clock.UtcNow.Returns(new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc));
        user.Activate(_clock);
        Assert.True(user.Status.IsActive);
    }
}
