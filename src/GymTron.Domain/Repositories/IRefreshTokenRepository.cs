using GymTron.Domain.Entities;

namespace GymTron.Domain.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenHash(string tokenHash, CancellationToken cancellationToken = default);
    Task Add(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task Update(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAllForUser(int userId, DateTime revokedAt, CancellationToken cancellationToken = default);
}
