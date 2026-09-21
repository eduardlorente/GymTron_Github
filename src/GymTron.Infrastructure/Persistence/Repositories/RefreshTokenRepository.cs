using GymTron.Domain.Entities;
using GymTron.Domain.Repositories;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL;

namespace GymTron.Infrastructure.Persistence.Repositories;

internal class RefreshTokenRepository(IRefreshTokenDAL refreshTokenDAL) : IRefreshTokenRepository
{
    private readonly IRefreshTokenDAL _refreshTokenDAL = refreshTokenDAL;

    public async Task<RefreshToken?> GetByTokenHash(string tokenHash, CancellationToken cancellationToken = default)
    {
        RefreshTokenDALModel? model = await _refreshTokenDAL.GetByTokenHash(tokenHash, cancellationToken);
        if (model == null)
        {
            return null;
        }

        return RefreshToken.FromDatabase(
            model.Id,
            model.UserId,
            model.TokenHash,
            model.ExpiresAt,
            model.RevokedAt,
            model.ReplacedByTokenHash,
            model.CreatedAt);
    }

    public async Task Add(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _refreshTokenDAL.Add(
            refreshToken.UserId,
            refreshToken.TokenHash,
            refreshToken.ExpiresAt,
            refreshToken.Status.CreatedOn,
            cancellationToken);
    }

    public async Task Update(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _refreshTokenDAL.Update(
            refreshToken.Id,
            refreshToken.RevokedAt,
            refreshToken.ReplacedByTokenHash,
            cancellationToken);
    }

    public async Task RevokeAllForUser(int userId, DateTime revokedAt, CancellationToken cancellationToken = default)
    {
        await _refreshTokenDAL.RevokeAllForUser(userId, revokedAt, cancellationToken);
    }
}
