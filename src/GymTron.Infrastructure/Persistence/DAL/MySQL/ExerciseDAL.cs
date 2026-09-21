using Dapper;
using GymTron.Domain.Entities;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL.Extensions;
using GymTron.Infrastructure.Persistence.Serialization;
using MySql.Data.MySqlClient;
using System.Data;

namespace GymTron.Infrastructure.Persistence.DAL.MySQL;

internal class ExerciseDAL(string connectionString) : IExerciseDAL
{


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
        using IDbConnection dbConnection = new MySqlConnection(connectionString);

        string query = @"INSERT INTO exercises 
                        (training_id, exercise_parameters_id, weight, duration, repetitions, created_on, observations) 
                     VALUES 
                        (@TrainingId, @ExerciseParametersId, @Weight, @Duration, @Repetitions, @CreatedOn, @Observations);";

        var parameters = exercises.Select(e => new
        {
            e.TrainingId,
            e.ExerciseParametersId,
            e.Weight,
            Duration = e.DurationInSeconds,
            Repetitions = e.CurrentRepetitions,
            e.Status.CreatedOn,
            Observations = ObservationSerializer.Serialize(e.Observations)
        });

        await dbConnection.ExecuteAsync(new CommandDefinition(query, parameters, cancellationToken: cancellationToken));
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
                            E.training_id = @TrainingId;".ToReadUncommited();

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
                            (@UserId IS NULL OR T.user_id = @UserId);".ToReadUncommited();

        return await dbConnection.QueryAsync<ExerciseDALModel>(new CommandDefinition(query, new { UserId = userId }, cancellationToken: cancellationToken));
    }
}
