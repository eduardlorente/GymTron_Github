using System.Data;
using Dapper;
using GymTron.Infrastructure.Persistence.DAL.Models;
using MySqlConnector;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal class UserDAL(string connectionString) : IUserDAL
{
    public async Task<UserDALModel?> GetById(int id, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"SELECT id AS Id, username AS Username, email AS Email, password_hash AS PasswordHash, type_id AS TypeId, is_active AS IsActive, created_at AS CreatedAt 
                       FROM users 
                       WHERE id = @Id;";

        return await dbConnection.QueryFirstOrDefaultAsync<UserDALModel>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<UserDALModel?> GetByUsernameOrEmail(string identifier, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"SELECT id AS Id, username AS Username, email AS Email, password_hash AS PasswordHash, type_id AS TypeId, is_active AS IsActive, created_at AS CreatedAt 
                       FROM users 
                       WHERE username = @Identifier OR email = @Identifier;";

        return await dbConnection.QueryFirstOrDefaultAsync<UserDALModel>(
            new CommandDefinition(sql, new { Identifier = identifier }, cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsByUsernameOrEmail(string username, string email, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"SELECT COUNT(1) 
                       FROM users 
                       WHERE username = @Username OR email = @Email;";

        int count = await dbConnection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { Username = username, Email = email }, cancellationToken: cancellationToken));

        return count > 0;
    }

    public async Task<bool> ExistsByUsernameOrEmail(string username, string email, int excludeUserId, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"SELECT COUNT(1) 
                       FROM users 
                       WHERE (username = @Username OR email = @Email) AND id != @ExcludeUserId;";

        int count = await dbConnection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { Username = username, Email = email, ExcludeUserId = excludeUserId }, cancellationToken: cancellationToken));

        return count > 0;
    }

    public async Task<IEnumerable<UserDALModel>> GetAll(CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"SELECT id AS Id, username AS Username, email AS Email, password_hash AS PasswordHash, type_id AS TypeId, is_active AS IsActive, created_at AS CreatedAt 
                       FROM users 
                       ORDER BY id ASC;";

        return await dbConnection.QueryAsync<UserDALModel>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<int> Add(string username, string email, string passwordHash, int typeId, DateTime createdAt, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"INSERT INTO users (username, email, password_hash, type_id, is_active, created_at) 
                       VALUES (@Username, @Email, @PasswordHash, @TypeId, 1, @CreatedAt);
                       SELECT LAST_INSERT_ID();";

        return await dbConnection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                TypeId = typeId,
                CreatedAt = createdAt
            }, cancellationToken: cancellationToken));
    }

    public async Task Update(int id, string username, string email, string passwordHash, int typeId, bool isActive, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"UPDATE users 
                       SET username = @Username, email = @Email, password_hash = @PasswordHash, type_id = @TypeId, is_active = @IsActive 
                       WHERE id = @Id;";

        await dbConnection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                Id = id,
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                TypeId = typeId,
                IsActive = isActive
            }, cancellationToken: cancellationToken));
    }

    public async Task SoftDelete(int id, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"UPDATE users 
                       SET is_active = 0 
                       WHERE id = @Id;";

        await dbConnection.ExecuteAsync(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }
}
