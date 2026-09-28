using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL;
using GymTron.Infrastructure.Persistence.Repositories;
using NSubstitute;

namespace GymTron.UnitTests.Infrastructure;

public class UserRepositoryTests
{
    private readonly IUserDAL _dal = Substitute.For<IUserDAL>();

    [Fact]
    public async Task UserRepository_GetById_MapsModelCorrectly()
    {
        var model = new UserDALModel
        {
            Id = 5,
            Username = "adm",
            Email = "adm@gymtron.local",
            PasswordHash = "hash",
            TypeId = (int)UserTypes.Administrator,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1)
        };
        _dal.GetById(5, Arg.Any<CancellationToken>()).Returns(model);

        var repo = new UserRepository(_dal);
        var user = await repo.GetById(5);

        Assert.NotNull(user);
        Assert.Equal(5, user.Id);
        Assert.Equal("adm", user.Username);
        Assert.Equal(UserTypes.Administrator, user.TypeId);
    }

    [Fact]
    public async Task UserRepository_GetAll_MapsAllUsers()
    {
        var list = new List<UserDALModel>
        {
            new() { Id = 1, Username = "u1", Email = "u1@gymtron.local", PasswordHash = "h1", TypeId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Username = "u2", Email = "u2@gymtron.local", PasswordHash = "h2", TypeId = 2, IsActive = false, CreatedAt = DateTime.UtcNow }
        };
        _dal.GetAll(Arg.Any<CancellationToken>()).Returns(list);

        var repo = new UserRepository(_dal);
        var users = await repo.GetAll();

        Assert.Equal(2, users.Count);
        Assert.Equal(UserTypes.Standard, users[0].TypeId);
        Assert.True(users[0].Status.IsActive);
        Assert.Equal(UserTypes.Administrator, users[1].TypeId);
        Assert.False(users[1].Status.IsActive);
    }

    [Fact]
    public async Task UserRepository_AddUpdateSoftDelete_DelegatesToDal()
    {
        var repo = new UserRepository(_dal);
        var user = User.FromDatabase(10, "u", "e@gymtron.local", "h", UserTypes.Administrator, true, DateTime.UtcNow);

        await repo.Add(user);
        await _dal.Received(1).Add("u", "e@gymtron.local", "h", (int)UserTypes.Administrator, user.Status.CreatedOn, Arg.Any<CancellationToken>());

        await repo.Update(user);
        await _dal.Received(1).Update(10, "u", "e@gymtron.local", "h", (int)UserTypes.Administrator, true, Arg.Any<CancellationToken>());

        await repo.SoftDelete(10);
        await _dal.Received(1).SoftDelete(10, Arg.Any<CancellationToken>());
    }
}
