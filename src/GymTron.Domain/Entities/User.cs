using GymTron.Domain.Enums;
using GymTron.Domain.Services;
using GymTron.Domain.ValueObjects;

namespace GymTron.Domain.Entities;

public class User : Entity<int>
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserTypes TypeId { get; private set; } = UserTypes.Standard;

    private User(int id, string username, string email, string passwordHash, UserTypes typeId, EntityStatus status)
        : base(id)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        TypeId = typeId;
        Status = status;
    }

    public static User New(string username, string email, string passwordHash, IClock clock)
    {
        return New(username, email, passwordHash, UserTypes.Standard, clock);
    }

    public static User New(string username, string email, string passwordHash, UserTypes typeId, IClock clock)
    {
        return new User(0, username, email, passwordHash, typeId, EntityStatus.New(clock));
    }

    public static User New(string username, string email, string passwordHash, DateTime createdOn)
    {
        return new User(0, username, email, passwordHash, UserTypes.Standard, EntityStatus.FromDatabase(EntityStatusTypes.ACTIVE, createdOn));
    }

    public static User FromDatabase(int id, string username, string email, string passwordHash, bool isActive, DateTime createdOn)
    {
        return FromDatabase(id, username, email, passwordHash, UserTypes.Standard, isActive, createdOn);
    }

    public static User FromDatabase(int id, string username, string email, string passwordHash, UserTypes typeId, bool isActive, DateTime createdOn)
    {
        var statusType = isActive ? EntityStatusTypes.ACTIVE : EntityStatusTypes.DELETED;
        return new User(id, username, email, passwordHash, typeId, EntityStatus.FromDatabase(statusType, createdOn));
    }

    public void UpdateDetails(string username, string email, UserTypes typeId, IClock clock)
    {
        Username = username;
        Email = email;
        TypeId = typeId;
        Status.Update(clock);
    }

    public void UpdatePassword(string newPasswordHash, IClock clock)
    {
        PasswordHash = newPasswordHash;
        Status.Update(clock);
    }

    public void Deactivate(IClock clock)
    {
        Status.Delete(clock);
    }

    public void Activate(IClock clock)
    {
        Status.Update(EntityStatusTypes.ACTIVE, clock);
    }
}
