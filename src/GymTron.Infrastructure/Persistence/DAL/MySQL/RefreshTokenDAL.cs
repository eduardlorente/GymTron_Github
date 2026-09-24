using System.Data;
using Dapper;
using GymTron.Infrastructure.Persistence.DAL.Models;
using MySqlConnector;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal class RefreshTokenDAL(string connectionString) : IRefreshTokenDAL
{
    public async Task<RefreshTokenDALModel?> GetByTokenHash(string tokenHash, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"SELECT id AS Id, user_id AS UserId, token_hash AS TokenHash, expires_at AS ExpiresAt, 
                              created_at AS CreatedAt, revoked_at AS RevokedAt, replaced_by_token_hash AS ReplacedByTokenHash 
                       FROM refresh_tokens 
                       WHERE token_hash = @TokenHash;";

        return await dbConnection.QueryFirstOrDefaultAsync<RefreshTokenDALModel>(
            new CommandDefinition(sql, new { TokenHash = tokenHash }, cancellationToken: cancellationToken));
    }

    public async Task Add(int userId, string tokenHash, DateTime expiresAt, DateTime createdAt, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"INSERT INTO refresh_tokens (user_id, token_hash, expires_at, created_at) 
                       VALUES (@UserId, @TokenHash, @ExpiresAt, @CreatedAt);";

        await dbConnection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt,
                CreatedAt = createdAt
            }, cancellationToken: cancellationToken));
    }

    public async Task Update(int id, DateTime? revokedAt, string? replacedByTokenHash, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"UPDATE refresh_tokens 
                       SET revoked_at = @RevokedAt, replaced_by_token_hash = @ReplacedByTokenHash 
                       WHERE id = @Id;";

        await dbConnection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                Id = id,
                RevokedAt = revokedAt,
                ReplacedByTokenHash = replacedByTokenHash
            }, cancellationToken: cancellationToken));
    }

    public async Task RevokeAllForUser(int userId, DateTime revokedAt, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"UPDATE refresh_tokens 
                       SET revoked_at = @RevokedAt 
                       WHERE user_id = @UserId AND revoked_at IS NULL;";

        await dbConnection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                UserId = userId,
                RevokedAt = revokedAt
            }, cancellationToken: cancellationToken));
    }

    public async Task<int> DeleteExpiredAndRevoked(DateTime olderThanUtc, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        const string sql = @"DELETE FROM refresh_tokens 
                             WHERE expires_at < @OlderThanUtc 
                                OR (revoked_at IS NOT NULL AND revoked_at < @OlderThanUtc);";
        return await dbConnection.ExecuteAsync(
            new CommandDefinition(sql, new { OlderThanUtc = olderThanUtc }, cancellationToken: cancellationToken));
    }
}
