using System.Data;
using Dapper;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL.Extensions;
using MySql.Data.MySqlClient;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal class UserDAL(string connectionString) : IUserDAL
{
    public async Task<UserDALModel?> GetById(int id, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"SELECT id AS Id, username AS Username, email AS Email, password_hash AS PasswordHash, is_active AS IsActive, created_at AS CreatedAt 
                       FROM users 
                       WHERE id = @Id;".ToReadUncommited();

        return await dbConnection.QueryFirstOrDefaultAsync<UserDALModel>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<UserDALModel?> GetByUsernameOrEmail(string identifier, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"SELECT id AS Id, username AS Username, email AS Email, password_hash AS PasswordHash, is_active AS IsActive, created_at AS CreatedAt 
                       FROM users 
                       WHERE username = @Identifier OR email = @Identifier;".ToReadUncommited();

        return await dbConnection.QueryFirstOrDefaultAsync<UserDALModel>(
            new CommandDefinition(sql, new { Identifier = identifier }, cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsByUsernameOrEmail(string username, string email, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"SELECT COUNT(1) 
                       FROM users 
                       WHERE username = @Username OR email = @Email;".ToReadUncommited();

        int count = await dbConnection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { Username = username, Email = email }, cancellationToken: cancellationToken));

        return count > 0;
    }

    public async Task<int> Add(string username, string email, string passwordHash, DateTime createdAt, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"INSERT INTO users (username, email, password_hash, is_active, created_at) 
                       VALUES (@Username, @Email, @PasswordHash, 1, @CreatedAt);
                       SELECT LAST_INSERT_ID();";

        return await dbConnection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                CreatedAt = createdAt
            }, cancellationToken: cancellationToken));
    }
}
