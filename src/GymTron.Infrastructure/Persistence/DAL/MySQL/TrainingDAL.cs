using Dapper;
using GymTron.Domain.Aggregates;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL.Extensions;
using MySql.Data.MySqlClient;
using System.Data;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal class TrainingDAL(string connectionString) : ITrainingDAL
{


    public async Task Add(Training entity, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            entity.UserId,
            entity.RoutineId,
            DayOfWeek = entity.DayOfTheWeek,
            StartedOn = entity.StartedOn.FullDate,
            entity.Status.Status
        };

        using IDbConnection dbConnection = new MySqlConnection(connectionString);

        string query = @"INSERT INTO trainings 
                            (user_id, routine_id, day_of_week, started_on, status) 
                         VALUES 
                            (@UserId, @RoutineId, @DayOfWeek, @StartedOn, @Status);";

        await dbConnection.ExecuteAsync(new CommandDefinition(query, parameters, cancellationToken: cancellationToken));
    }


    public async Task<TrainingDALModel?> GetById(int id, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);

        string query = @"SELECT 
                            id AS Id,
                            user_id AS UserId,
                            routine_id AS RoutineId,
                            day_of_week AS DayOfWeek,
                            started_on AS StartedOn,
                            completed_on AS CompletedOn,
                            status AS StatusType 
                         FROM 
                            trainings 
                         WHERE 
                            id = @Id;".ToReadUncommited();

        return await dbConnection.QuerySingleOrDefaultAsync<TrainingDALModel>(new CommandDefinition(query, new { Id = id }, cancellationToken: cancellationToken));
    }


    public async Task<TrainingDALModel?> GetCurrent(int? userId = null, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);

        string query = @"SELECT 
                            id AS Id,
                            user_id AS UserId,
                            routine_id AS RoutineId,
                            day_of_week AS DayOfWeek,
                            started_on AS StartedOn,
                            completed_on AS CompletedOn,
                            status AS StatusType
                         FROM 
                            trainings 
                         WHERE 
                            completed_on IS NULL 
                            AND status = 1
                            AND (@UserId IS NULL OR user_id = @UserId)
                         ORDER BY 
                            started_on DESC LIMIT 1;".ToReadUncommited();

        return await dbConnection.QuerySingleOrDefaultAsync<TrainingDALModel>(new CommandDefinition(query, new { UserId = userId }, cancellationToken: cancellationToken));
    }


    public async Task<List<TrainingDALModel>> ListAll(int? userId = null, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);

        string query = @"SELECT 
                            id AS Id,
                            user_id AS UserId,
                            routine_id AS RoutineId,
                            day_of_week AS DayOfWeek,
                            started_on AS StartedOn,
                            completed_on AS CompletedOn,
                            status AS StatusType
                         FROM 
                            trainings
                         WHERE
                            (@UserId IS NULL OR user_id = @UserId);".ToReadUncommited();

        return (await dbConnection.QueryAsync<TrainingDALModel>(new CommandDefinition(query, new { UserId = userId }, cancellationToken: cancellationToken))).ToList();
    }


    public async Task Update(TrainingDALModel model, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);

        string query = @"UPDATE 
                            trainings 
                        SET 
                            user_id = @UserId,
                            routine_id = @RoutineId, 
                            day_of_week = @DayOfWeek, 
                            started_on = @StartedOn, 
                            completed_on = @CompletedOn, 
                            status = @StatusType 
                        WHERE 
                            id = @Id
                            AND (@UserId IS NULL OR user_id = @UserId);";

        await dbConnection.ExecuteAsync(new CommandDefinition(query, model, cancellationToken: cancellationToken));
    }
}
