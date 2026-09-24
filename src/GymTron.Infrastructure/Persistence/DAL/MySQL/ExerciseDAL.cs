using Dapper;
using GymTron.Domain.Entities;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.Serialization;
using MySqlConnector;
using System.Data;
using System.Text;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal class ExerciseDAL(string connectionString, IDbTransactionContext? transactionContext = null) : IExerciseDAL
{
    private readonly IDbTransactionContext? _transactionContext = transactionContext;


    public async Task Add(Exercise entity, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);

        string query = @"INSERT INTO exercises 
                            (training_id, exercise_parameters_id, weight, duration, repetitions, created_on, observations) 
                         VALUES 
                            (@TrainingId, @ExerciseParametersId, @Weight, @Duration, @Repetitions, @CreatedOn, @Observations);";

        var parameters = new
        {
            entity.TrainingId,
            entity.ExerciseParametersId,
            entity.Weight,
            Duration = entity.DurationInSeconds,
            Repetitions = entity.CurrentRepetitions,
            entity.Status.CreatedOn,
            Observations = ObservationSerializer.Serialize(entity.Observations)
        };

        await dbConnection.ExecuteAsync(new CommandDefinition(query, parameters, cancellationToken: cancellationToken));
    }


    public async Task AddRange(List<Exercise> exercises, CancellationToken cancellationToken = default)
    {
        if (exercises.Count == 0) return;

        var sb = new StringBuilder();
        sb.Append(@"INSERT INTO exercises 
                   (training_id, exercise_parameters_id, weight, duration, repetitions, created_on, observations) 
                   VALUES ");

        var parameters = new DynamicParameters();
        for (int i = 0; i < exercises.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append($"(@TId{i}, @EPId{i}, @Weight{i}, @Dur{i}, @Reps{i}, @CreatedOn{i}, @Obs{i})");

            var e = exercises[i];
            parameters.Add($"TId{i}", e.TrainingId);
            parameters.Add($"EPId{i}", e.ExerciseParametersId);
            parameters.Add($"Weight{i}", e.Weight);
            parameters.Add($"Dur{i}", e.DurationInSeconds);
            parameters.Add($"Reps{i}", e.CurrentRepetitions);
            parameters.Add($"CreatedOn{i}", e.Status.CreatedOn);
            parameters.Add($"Obs{i}", ObservationSerializer.Serialize(e.Observations));
        }

        if (_transactionContext?.HasActiveTransaction == true && _transactionContext.ActiveConnection != null)
        {
            await _transactionContext.ActiveConnection.ExecuteAsync(
                new CommandDefinition(sb.ToString(), parameters, _transactionContext.ActiveTransaction, cancellationToken: cancellationToken));
        }
        else
        {
            using IDbConnection dbConnection = new MySqlConnection(connectionString);
            await dbConnection.ExecuteAsync(new CommandDefinition(sb.ToString(), parameters, cancellationToken: cancellationToken));
        }
    }


    public async Task<IEnumerable<ExerciseDALModel>> ListByTrainingId(int trainingId, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);

        string query = @"SELECT 
                            E.id AS Id,
                            E.training_id AS TrainingId,
                            E.exercise_parameters_id AS ExerciseParametersId,
                            EP.name AS Name,
                            E.weight AS Weight,
                            E.duration AS DurationInSeconds,
                            E.repetitions AS Repetitions,
                            E.created_on AS CreatedOn,
                            E.observations AS ObservationsCSV
                         FROM 
                            exercises E
                         LEFT JOIN 
                            exercise_parameters EP ON E.exercise_parameters_id = EP.id
                         WHERE 
                            E.training_id = @TrainingId;";

        return await dbConnection.QueryAsync<ExerciseDALModel>(new CommandDefinition(query, new { TrainingId = trainingId }, cancellationToken: cancellationToken));
    }


    public async Task<IEnumerable<ExerciseDALModel>> ListAll(int? userId = null, CancellationToken cancellationToken = default)
    {
        using IDbConnection dbConnection = new MySqlConnection(connectionString);

        string query = @"SELECT 
                            E.id AS Id,
                            E.training_id AS TrainingId,
                            E.exercise_parameters_id AS ExerciseParametersId,
                            EP.name AS Name,
                            E.weight AS Weight,
                            E.duration AS DurationInSeconds,
                            E.repetitions AS Repetitions,
                            E.created_on AS CreatedOn,
                            E.observations AS ObservationsCSV
                         FROM 
                            exercises E
                         LEFT JOIN 
                            trainings T ON E.training_id = T.id
                         LEFT JOIN 
                            exercise_parameters EP ON E.exercise_parameters_id = EP.id
                         WHERE
                            (@UserId IS NULL OR T.user_id = @UserId)
                         ORDER BY 
                            EP.name ASC, E.created_on DESC;";

        return await dbConnection.QueryAsync<ExerciseDALModel>(new CommandDefinition(query, new { UserId = userId }, cancellationToken: cancellationToken));
    }
}
