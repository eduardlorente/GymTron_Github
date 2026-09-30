using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Infrastructure.Persistence.DAL.MySQL;
using GymTron.Infrastructure.Persistence.Repositories;
using GymTron.IntegrationTests.Database;

namespace GymTron.IntegrationTests;

[Collection(MySqlCollection.Name)]
public sealed class UserDalAndRepositoryTests(MySqlCollectionFixture fixture) : MySqlIntegrationTest(fixture)
{
    [Fact]
    public async Task UserDAL_Add_InsertsAndReturnsId()
    {
        var dal = new UserDAL(ConnectionString);
        DateTime createdAt = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Unspecified);

        int id = await dal.Add("testuser", "test@gymtron.local", "hash123", (int)UserTypes.Standard, createdAt);

        Assert.True(id > 0);
        var user = await dal.GetById(id);
        Assert.NotNull(user);
        Assert.Equal("testuser", user.Username);
        Assert.Equal("test@gymtron.local", user.Email);
        Assert.Equal("hash123", user.PasswordHash);
        Assert.Equal((int)UserTypes.Standard, user.TypeId);
        Assert.True(user.IsActive);
    }

    [Fact]
    public async Task UserDAL_GetByUsernameOrEmail_FindsByBothCriteria()
    {
        var dal = new UserDAL(ConnectionString);
        DateTime createdAt = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Unspecified);
        await dal.Add("userA", "userA@gymtron.local", "hashA", (int)UserTypes.Administrator, createdAt);

        var byUsername = await dal.GetByUsernameOrEmail("userA");
        var byEmail = await dal.GetByUsernameOrEmail("userA@gymtron.local");
        var notFound = await dal.GetByUsernameOrEmail("nonexistent");

        Assert.NotNull(byUsername);
        Assert.NotNull(byEmail);
        Assert.Null(notFound);
        Assert.Equal(byUsername.Id, byEmail.Id);
    }

    [Fact]
    public async Task UserDAL_ExistsByUsernameOrEmail_ValidatesCorrectly()
    {
        var dal = new UserDAL(ConnectionString);
        DateTime createdAt = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Unspecified);
        int userId = await dal.Add("existing", "existing@gymtron.local", "hash", (int)UserTypes.Standard, createdAt);

        Assert.True(await dal.ExistsByUsernameOrEmail("existing", "other@gymtron.local"));
        Assert.True(await dal.ExistsByUsernameOrEmail("other", "existing@gymtron.local"));
        Assert.False(await dal.ExistsByUsernameOrEmail("other", "other@gymtron.local"));

        // Exclude overload
        Assert.False(await dal.ExistsByUsernameOrEmail("existing", "other@gymtron.local", userId));
        Assert.True(await dal.ExistsByUsernameOrEmail("existing", "other@gymtron.local", userId + 1));
    }

    [Fact]
    public async Task UserDAL_UpdateAndSoftDelete_AppliesChanges()
    {
        var dal = new UserDAL(ConnectionString);
        DateTime createdAt = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Unspecified);
        int userId = await dal.Add("initial", "initial@gymtron.local", "hash1", (int)UserTypes.Standard, createdAt);

        await dal.Update(userId, "updated", "updated@gymtron.local", "newhash", (int)UserTypes.Administrator, true);
        var updated = await dal.GetById(userId);
        Assert.NotNull(updated);
        Assert.Equal("updated", updated.Username);
        Assert.Equal("updated@gymtron.local", updated.Email);
        Assert.Equal("newhash", updated.PasswordHash);
        Assert.Equal((int)UserTypes.Administrator, updated.TypeId);
        Assert.True(updated.IsActive);

        await dal.SoftDelete(userId);
        var deleted = await dal.GetById(userId);
        Assert.NotNull(deleted);
        Assert.False(deleted.IsActive);
    }

    [Fact]
    public async Task UserDAL_GetAll_ReturnsAllUsersOrderedById()
    {
        var dal = new UserDAL(ConnectionString);
        DateTime createdAt = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Unspecified);
        await dal.Add("u1", "u1@gymtron.local", "h1", (int)UserTypes.Standard, createdAt);
        await dal.Add("u2", "u2@gymtron.local", "h2", (int)UserTypes.Administrator, createdAt);

        var all = (await dal.GetAll()).ToList();

        Assert.Equal(2, all.Count);
        Assert.Equal("u1", all[0].Username);
        Assert.Equal("u2", all[1].Username);
    }

    [Fact]
    public async Task UserRepository_FullLifecycle_MapsDomainEntity()
    {
        var dal = new UserDAL(ConnectionString);
        var repository = new UserRepository(dal);
        var clock = new TestClock(new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc));

        var newUser = User.New("domainUser", "domain@gymtron.local", "domainHash", UserTypes.Administrator, clock);
        int id = await repository.Add(newUser);
        Assert.True(id > 0);

        var fetched = await repository.GetById(id);
        Assert.NotNull(fetched);
        Assert.Equal("domainUser", fetched.Username);
        Assert.Equal(UserTypes.Administrator, fetched.TypeId);
        Assert.True(fetched.Status.IsActive);

        var byEmail = await repository.GetByUsernameOrEmail("domain@gymtron.local");
        Assert.NotNull(byEmail);
        Assert.Equal(id, byEmail.Id);

        Assert.True(await repository.ExistsByUsernameOrEmail("domainUser", "none@gymtron.local"));
        Assert.False(await repository.ExistsByUsernameOrEmail("domainUser", "none@gymtron.local", id));

        var allUsers = await repository.GetAll();
        Assert.Single(allUsers);

        await repository.SoftDelete(id);
        var afterDelete = await repository.GetById(id);
        Assert.NotNull(afterDelete);
        Assert.False(afterDelete.Status.IsActive);

        var userToUpdate = User.FromDatabase(id, "renamedUser", "renamed@gymtron.local", "renamedHash", UserTypes.Standard, true, DateTime.UtcNow);
        await repository.Update(userToUpdate);
        var afterUpdate = await repository.GetById(id);
        Assert.NotNull(afterUpdate);
        Assert.Equal("renamedUser", afterUpdate.Username);
        Assert.Equal(UserTypes.Standard, afterUpdate.TypeId);
    }

    [Fact]
    public async Task UserRepository_GetById_NonExistent_ReturnsNull()
    {
        var dal = new UserDAL(ConnectionString);
        var repository = new UserRepository(dal);

        var user = await repository.GetById(999_999);
        var userByIdentifier = await repository.GetByUsernameOrEmail("notfound");

        Assert.Null(user);
        Assert.Null(userByIdentifier);
    }

    private sealed class TestClock(DateTime time) : GymTron.Domain.Services.IClock
    {
        public DateTime UtcNow => time;
        public DateTime Now => time;
    }
}
