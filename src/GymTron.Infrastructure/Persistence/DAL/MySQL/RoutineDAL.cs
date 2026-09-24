using Dapper;
using GymTron.Infrastructure.Persistence.DAL.Models;
using MySqlConnector;
using System.Data;
using System.Text;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal class RoutineDAL(string connectionString) : IRoutineDAL
{


    public async Task<IEnumerable<RoutineFullDetailsDTO>> ListAll(int? userId = null, CancellationToken cancellationToken = default)
    {
        return await ListAllWithFullInfo(null, userId, cancellationToken);
    }


    public async Task<IEnumerable<RoutineFullDetailsDTO>> ListById(int id, int? userId = null, CancellationToken cancellationToken = default)
    {
        return await ListAllWithFullInfo(id, userId, cancellationToken);
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
                           ON le.id = (SELECT e2.id 
                                       FROM exercises e2 
                                       JOIN trainings t2 ON e2.training_id = t2.id 
                                       WHERE e2.exercise_parameters_id = ep.id 
                                         AND (@UserId IS NULL OR t2.user_id = @UserId) 
                                       ORDER BY e2.created_on DESC, e2.id DESC 
                                       LIMIT 1)
                       
                       WHERE (@RoutineId IS NULL OR r.id = @RoutineId)
                         AND (@UserId IS NULL OR r.user_id IS NULL OR r.user_id = @UserId)
                       ORDER BY r.id DESC, ri.day_of_week ASC, ri.`position` ASC;";

        var parameters = new { RoutineId = routineId, UserId = userId };

        IEnumerable<RoutineFullDetailsDTO> result = await dbConnection.QueryAsync<RoutineFullDetailsDTO>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        return result;
    }

    public async Task<int> Create(string name, IReadOnlyList<RoutineItemWriteModel> items, int? userId = null, CancellationToken cancellationToken = default)
    {
        await using var dbConnection = new MySqlConnection(connectionString);
        await dbConnection.OpenAsync(cancellationToken);
        await using var transaction = await dbConnection.BeginTransactionAsync(cancellationToken);
        try
        {
            int routineId = await InsertRoutineAsync(dbConnection, transaction, name, userId, cancellationToken);

            await InsertRoutineItemsBatchAsync(dbConnection, transaction, routineId, items, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return routineId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task Update(int id, string name, IReadOnlyList<RoutineItemWriteModel> items, CancellationToken cancellationToken = default)
    {
        await using var dbConnection = new MySqlConnection(connectionString);
        await dbConnection.OpenAsync(cancellationToken);
        await using var transaction = await dbConnection.BeginTransactionAsync(cancellationToken);
        try
        {
            await UpdateRoutineAsync(dbConnection, transaction, id, name, cancellationToken);
            await DeleteRoutineItemsAsync(dbConnection, transaction, id, cancellationToken);

            await InsertRoutineItemsBatchAsync(dbConnection, transaction, id, items, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
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

    private static async Task InsertRoutineItemsBatchAsync(IDbConnection dbConnection, IDbTransaction transaction, int routineId, IReadOnlyList<RoutineItemWriteModel> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0) return;

        var sb = new StringBuilder();
        sb.Append(@"INSERT INTO routine_items 
                   (routine_id, day_of_week, exercise_parameters_id, series, repetitions_min, repetitions_max, duration,
                    min_rest_time_in_seconds, max_rest_time_in_seconds, alternating_series, `position`, active) 
                   VALUES ");

        var parameters = new DynamicParameters();
        parameters.Add("RoutineId", routineId);

        for (int i = 0; i < items.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append($"(@RoutineId, @Day{i}, @EP{i}, @Series{i}, @RepMin{i}, @RepMax{i}, @Dur{i}, @MinRest{i}, @MaxRest{i}, @Alt{i}, @Pos{i}, 1)");

            var item = items[i];
            parameters.Add($"Day{i}", item.DayOfWeek);
            parameters.Add($"EP{i}", item.ExerciseParametersId);
            parameters.Add($"Series{i}", item.Series);
            parameters.Add($"RepMin{i}", item.RepetitionsMin);
            parameters.Add($"RepMax{i}", item.RepetitionsMax);
            parameters.Add($"Dur{i}", item.Duration);
            parameters.Add($"MinRest{i}", item.MinRestTimeInSeconds);
            parameters.Add($"MaxRest{i}", item.MaxRestTimeInSeconds);
            parameters.Add($"Alt{i}", item.AlternatingSeries);
            parameters.Add($"Pos{i}", item.Position);
        }

        await dbConnection.ExecuteAsync(new CommandDefinition(sb.ToString(), parameters, transaction, cancellationToken: cancellationToken));
    }
}
