using Dapper;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL.Extensions;
using MySql.Data.MySqlClient;
using System.Data;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal class RoutineDAL(string connectionString) : IRoutineDAL
{


    public async Task<IEnumerable<RoutineFullDetailsDTO>> ListAll(int? userId = null, CancellationToken cancellationToken = default)
    {
        return await ListAllWithFullInfo(null, userId, cancellationToken);
    }


    public async Task<IEnumerable<RoutineFullDetailsDTO>> ListById(int id, CancellationToken cancellationToken = default)
    {
        return await ListAllWithFullInfo(id, null, cancellationToken);
    }


    private async Task<IEnumerable<RoutineFullDetailsDTO>> ListAllWithFullInfo(int? routineId, int? userId, CancellationToken cancellationToken)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);

        string sql = @"SELECT 
                           r.id AS RoutineId, 
                           r.user_id AS UserId,
                           r.name AS RoutineName,
                       
                           ri.id AS RoutineItemId, 
                           ri.day_of_week AS DayOfWeek, 
                           ri.exercise_parameters_id AS ExerciseParametersId, 
                           ri.min_rest_time_in_seconds AS MinRestTimeInSeconds, 
                           ri.max_rest_time_in_seconds AS MaxRestTimeInSeconds, 
                           ri.alternating_series AS AlternatingSeries, 
                           ri.active AS Active,
                           ri.series AS Series, 
                           ri.repetitions_min AS RepetitionsMin, 
                           ri.repetitions_max AS RepetitionsMax, 
                           ri.duration AS Duration,
                           ri.`position` AS Position,
                       
                           ep.id AS ExerciseParametersId,
                           ep.name AS ExerciseName, 
                           ep.description AS Description,
                           ep.pattern AS Pattern, 
                           ep.replays_in_reserve AS ReplaysInReserve, 
                           ep.type_id AS TypeId,
                       
                           le.weight AS LastWeight,
                           le.duration AS LastDuration,
                           le.repetitions AS LastRepetitions,
                           le.observations AS LastObservations
                       
                       FROM routines r
                       LEFT JOIN routine_items ri ON r.id = ri.routine_id
                       LEFT JOIN exercise_parameters ep ON ri.exercise_parameters_id = ep.id
                       LEFT JOIN exercises le 
                           ON ep.id = le.exercise_parameters_id 
                           AND le.created_on = (SELECT MAX(e2.created_on) 
                                               FROM exercises e2 
                                               WHERE e2.exercise_parameters_id = ep.id)
                       
                       WHERE (@RoutineId IS NULL OR r.id = @RoutineId)
                         AND (@UserId IS NULL OR r.user_id IS NULL OR r.user_id = @UserId)
                       ORDER BY r.id DESC, ri.day_of_week ASC, ri.`position` ASC;".ToReadUncommited();

        var parameters = new { RoutineId = routineId, UserId = userId };

        IEnumerable<RoutineFullDetailsDTO> result = await dbConnection.QueryAsync<RoutineFullDetailsDTO>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        return result.Distinct();
    }

    public async Task<int> Create(string name, IReadOnlyList<RoutineItemWriteModel> items, int? userId = null, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        dbConnection.Open();
        using IDbTransaction transaction = dbConnection.BeginTransaction();
        try
        {
            int routineId = await InsertRoutineAsync(dbConnection, transaction, name, userId, cancellationToken);

            foreach (RoutineItemWriteModel item in items)
            {
                await InsertRoutineItemAsync(dbConnection, transaction, routineId, item, cancellationToken);
            }

            transaction.Commit();
            return routineId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task Update(int id, string name, IReadOnlyList<RoutineItemWriteModel> items, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        dbConnection.Open();
        using IDbTransaction transaction = dbConnection.BeginTransaction();
        try
        {
            await UpdateRoutineAsync(dbConnection, transaction, id, name, cancellationToken);
            await DeleteRoutineItemsAsync(dbConnection, transaction, id, cancellationToken);

            foreach (RoutineItemWriteModel item in items)
            {
                await InsertRoutineItemAsync(dbConnection, transaction, id, item, cancellationToken);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static async Task<int> InsertRoutineAsync(IDbConnection dbConnection, IDbTransaction transaction, string name, int? userId, CancellationToken cancellationToken)
    {
        string sql = @"INSERT INTO routines (user_id, name) VALUES (@UserId, @Name); SELECT LAST_INSERT_ID();";
        object? result = await dbConnection.ExecuteScalarAsync(new CommandDefinition(sql, new { UserId = userId, Name = name }, transaction, cancellationToken: cancellationToken));
        return Convert.ToInt32(result);
    }

    private static async Task UpdateRoutineAsync(IDbConnection dbConnection, IDbTransaction transaction, int id, string name, CancellationToken cancellationToken)
    {
        string sql = @"UPDATE routines SET name = @Name WHERE id = @Id";
        _ = await dbConnection.ExecuteAsync(new CommandDefinition(sql, new { Id = id, Name = name }, transaction, cancellationToken: cancellationToken));
    }

    private static async Task DeleteRoutineItemsAsync(IDbConnection dbConnection, IDbTransaction transaction, int routineId, CancellationToken cancellationToken)
    {
        string sql = @"DELETE FROM routine_items WHERE routine_id = @RoutineId";
        _ = await dbConnection.ExecuteAsync(new CommandDefinition(sql, new { RoutineId = routineId }, transaction, cancellationToken: cancellationToken));
    }

    private static async Task InsertRoutineItemAsync(IDbConnection dbConnection, IDbTransaction transaction, int routineId, RoutineItemWriteModel item, CancellationToken cancellationToken)
    {
        string sql = @"INSERT INTO routine_items 
                       (routine_id, day_of_week, exercise_parameters_id, series, repetitions_min, repetitions_max, duration,
                        min_rest_time_in_seconds, max_rest_time_in_seconds, alternating_series, `position`, active) 
                       VALUES 
                       (@RoutineId, @DayOfWeek, @ExerciseParametersId, @Series, @RepetitionsMin, @RepetitionsMax, @Duration,
                        @MinRestTimeInSeconds, @MaxRestTimeInSeconds, @AlternatingSeries, @Position, 1)";
        _ = await dbConnection.ExecuteAsync(new CommandDefinition(sql, new
        {
            RoutineId = routineId,
            DayOfWeek = item.DayOfWeek,
            ExerciseParametersId = item.ExerciseParametersId,
            Series = item.Series,
            RepetitionsMin = item.RepetitionsMin,
            RepetitionsMax = item.RepetitionsMax,
            Duration = item.Duration,
            MinRestTimeInSeconds = item.MinRestTimeInSeconds,
            MaxRestTimeInSeconds = item.MaxRestTimeInSeconds,
            AlternatingSeries = item.AlternatingSeries,
            Position = item.Position
        }, transaction, cancellationToken: cancellationToken));
    }
}
