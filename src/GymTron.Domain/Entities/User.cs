using GymTron.Domain.Enums;
using GymTron.Domain.Services;
using GymTron.Domain.ValueObjects;

namespace GymTron.Domain.Entities;

public class User : Entity<int>
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    private User(int id, string username, string email, string passwordHash, EntityStatus status)
        : base(id)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Status = status;
    }

    public static User New(string username, string email, string passwordHash, IClock clock)
    {
        return new User(0, username, email, passwordHash, EntityStatus.New(clock));
    }

    public static User New(string username, string email, string passwordHash, DateTime createdOn)
    {
        return new User(0, username, email, passwordHash, EntityStatus.FromDatabase(EntityStatusTypes.ACTIVE, createdOn));
    }

    public static User FromDatabase(int id, string username, string email, string passwordHash, bool isActive, DateTime createdOn)
    {
        var statusType = isActive ? EntityStatusTypes.ACTIVE : EntityStatusTypes.DELETED;
        return new User(id, username, email, passwordHash, EntityStatus.FromDatabase(statusType, createdOn));
    }

    public void UpdatePassword(string newPasswordHash, IClock clock)
    {
        PasswordHash = newPasswordHash;
        Status.Update(clock);
    }
}
