using GymTron.Infrastructure.Persistence.DAL.Models;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal interface IRefreshTokenDAL
{
    Task<RefreshTokenDALModel?> GetByTokenHash(string tokenHash, CancellationToken cancellationToken = default);
    Task Add(int userId, string tokenHash, DateTime expiresAt, DateTime createdAt, CancellationToken cancellationToken = default);
    Task Update(int id, DateTime? revokedAt, string? replacedByTokenHash, CancellationToken cancellationToken = default);
    Task RevokeAllForUser(int userId, DateTime revokedAt, CancellationToken cancellationToken = default);
}
