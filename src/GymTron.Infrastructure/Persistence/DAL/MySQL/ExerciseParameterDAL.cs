using Dapper;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL.Extensions;
using MySql.Data.MySqlClient;
using System.Data;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal class ExerciseParameterDAL(string connectionString) : IExerciseParameterDAL
{
    public async Task<IEnumerable<ExerciseParameterDALModel>> ListAll(CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"SELECT id AS Id, name AS Name, description AS Description, pattern AS Pattern, type_id AS TypeId, replays_in_reserve AS ReplaysInReserve 
                       FROM exercise_parameters 
                       ORDER BY name;".ToReadUncommited();
        return await dbConnection.QueryAsync<ExerciseParameterDALModel>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<ExerciseParameterDALModel?> GetById(int id, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"SELECT id AS Id, name AS Name, description AS Description, pattern AS Pattern, type_id AS TypeId, replays_in_reserve AS ReplaysInReserve 
                       FROM exercise_parameters 
                       WHERE id = @Id;".ToReadUncommited();
        return await dbConnection.QueryFirstOrDefaultAsync<ExerciseParameterDALModel>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<int> Create(string name, string description, string pattern, int typeId, int? replaysInReserve, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"INSERT INTO exercise_parameters (name, description, pattern, type_id, replays_in_reserve) 
                       VALUES (@Name, @Description, @Pattern, @TypeId, @ReplaysInReserve);
                       SELECT LAST_INSERT_ID();";
        return await dbConnection.ExecuteScalarAsync<int>(new CommandDefinition(sql, new
        {
            Name = name,
            Description = description,
            Pattern = pattern,
            TypeId = typeId,
            ReplaysInReserve = replaysInReserve
        }, cancellationToken: cancellationToken));
    }

    public async Task Update(int id, string name, string description, string pattern, int typeId, int? replaysInReserve, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        string sql = @"UPDATE exercise_parameters 
                       SET name = @Name, description = @Description, pattern = @Pattern, 
                           type_id = @TypeId, replays_in_reserve = @ReplaysInReserve 
                       WHERE id = @Id;";
        await dbConnection.ExecuteAsync(new CommandDefinition(sql, new
        {
            Id = id,
            Name = name,
            Description = description,
            Pattern = pattern,
            TypeId = typeId,
            ReplaysInReserve = replaysInReserve
        }, cancellationToken: cancellationToken));
    }
}
