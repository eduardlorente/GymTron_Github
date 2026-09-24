using Dapper;
using GymTron.Domain.Aggregates;
using GymTron.Infrastructure.Persistence.DAL.Models;
using MySqlConnector;
using System.Data;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal class TrainingDAL(string connectionString, IDbTransactionContext? transactionContext = null) : ITrainingDAL
{
    private readonly IDbTransactionContext? _transactionContext = transactionContext;



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
                            id = @Id;";

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
                            started_on DESC LIMIT 1;";

        return await dbConnection.QuerySingleOrDefaultAsync<TrainingDALModel>(new CommandDefinition(query, new { UserId = userId }, cancellationToken: cancellationToken));
    }


    public async Task<bool> HasActiveTraining(int userId, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        const string query = @"SELECT EXISTS(
                                  SELECT 1 
                                  FROM trainings 
                                  WHERE completed_on IS NULL 
                                    AND status = 1 
                                    AND user_id = @UserId
                               );";
        return await dbConnection.ExecuteScalarAsync<bool>(
            new CommandDefinition(query, new { UserId = userId }, cancellationToken: cancellationToken));
    }


    public async Task<IEnumerable<TrainingHistoryDALModel>> ListCompletedHistory(int userId, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);
        const string query = @"SELECT 
                                  started_on AS StartedOn, 
                                  day_of_week AS DayOfWeek 
                               FROM trainings 
                               WHERE user_id = @UserId 
                                 AND status = 6 
                               ORDER BY started_on DESC;";
        return await dbConnection.QueryAsync<TrainingHistoryDALModel>(
            new CommandDefinition(query, new { UserId = userId }, cancellationToken: cancellationToken));
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
                            (@UserId IS NULL OR user_id = @UserId);";

        return (await dbConnection.QueryAsync<TrainingDALModel>(new CommandDefinition(query, new { UserId = userId }, cancellationToken: cancellationToken))).ToList();
    }


    public async Task Update(TrainingDALModel model, CancellationToken cancellationToken = default)
    {
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

        if (_transactionContext?.HasActiveTransaction == true && _transactionContext.ActiveConnection != null)
        {
            await _transactionContext.ActiveConnection.ExecuteAsync(new CommandDefinition(
                query, model, _transactionContext.ActiveTransaction, cancellationToken: cancellationToken));
        }
        else
        {
            using IDbConnection dbConnection = new MySqlConnection(connectionString);
            await dbConnection.ExecuteAsync(new CommandDefinition(query, model, cancellationToken: cancellationToken));
        }
    }
}
