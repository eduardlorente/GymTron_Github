using GymTron.Domain.Services;
using GymTron.Domain.ValueObjects;

namespace GymTron.Domain.Entities;

public class RefreshToken : Entity<int>
{
    public int UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsExpired(DateTime now) => now >= ExpiresAt;
    public bool IsActive(DateTime now) => !IsRevoked && !IsExpired(now);

    private RefreshToken(
        int id,
        int userId,
        string tokenHash,
        DateTime expiresAt,
        DateTime? revokedAt,
        string? replacedByTokenHash,
        DateTime createdOn)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        RevokedAt = revokedAt;
        ReplacedByTokenHash = replacedByTokenHash;
        Status = EntityStatus.FromDatabase(createdOn);
    }

    public static RefreshToken New(
        int userId,
        string tokenHash,
        DateTime expiresAt,
        IClock clock)
    {
        return new RefreshToken(0, userId, tokenHash, expiresAt, null, null, clock.UtcNow);
    }

    public static RefreshToken FromDatabase(
        int id,
        int userId,
        string tokenHash,
        DateTime expiresAt,
        DateTime? revokedAt,
        string? replacedByTokenHash,
        DateTime createdOn)
    {
        return new RefreshToken(id, userId, tokenHash, expiresAt, revokedAt, replacedByTokenHash, createdOn);
    }

    public void Revoke(DateTime now, string? replacedByTokenHash = null)
    {
        RevokedAt = now;
        ReplacedByTokenHash = replacedByTokenHash;
        Status.Update(now);
    }
}
