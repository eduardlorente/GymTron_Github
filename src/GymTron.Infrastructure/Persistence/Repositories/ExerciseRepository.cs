using GymTron.Domain.Entities;
using GymTron.Domain.Projections;
using GymTron.Domain.Repositories;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL;
using GymTron.Infrastructure.Persistence.Serialization;

namespace GymTron.Infrastructure.Persistence.Repositories;

internal class ExerciseRepository(IExerciseDAL exerciseDAL) : IExerciseRepository
{


    private readonly IExerciseDAL _exerciseDAL = exerciseDAL;


    public async Task Add(Exercise entity, CancellationToken cancellationToken = default)
    {
        await _exerciseDAL.Add(entity, cancellationToken);
    }


    public async Task AddRange(IReadOnlyCollection<Exercise> exercises, CancellationToken cancellationToken = default)
    {
        List<Exercise> list = exercises as List<Exercise> ?? [.. exercises];
        await _exerciseDAL.AddRange(list, cancellationToken);
    }


    public async Task<List<Exercise>> ListByTraining(int trainingId, CancellationToken cancellationToken = default)
    {
        IEnumerable<ExerciseDALModel> exercises = await _exerciseDAL.ListByTrainingId(trainingId, cancellationToken);

        return BuildExercisesList(exercises);
    }


    public async Task<List<Exercise>> ListAll(CancellationToken cancellationToken = default)
    {
        IEnumerable<ExerciseDALModel> exercises = await _exerciseDAL.ListAll(cancellationToken: cancellationToken);

        return BuildExercisesList(exercises);
    }

    public async Task<List<ExerciseHistoryProjection>> ListHistory(int? userId = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<ExerciseDALModel> exercises = await _exerciseDAL.ListAll(userId, cancellationToken);

        return exercises.Select(x => new ExerciseHistoryProjection
        {
            Name = x.Name,
            CreatedOn = x.CreatedOn,
            Weight = x.Weight,
            Repetitions = x.Repetitions,
            DurationInSeconds = x.DurationInSeconds
        })
        .ToList();
    }


    private static List<Exercise> BuildExercisesList(IEnumerable<ExerciseDALModel> exercises)
    {
        return exercises.Select(e => Exercise.FromDatabase(e.Id,
                                                           e.TrainingId,
                                                           e.ExerciseParametersId,
                                                           e.Name,
                                                           e.Weight,
                                                           e.DurationInSeconds,
                                                           e.Repetitions,
                                                           e.CreatedOn,
                                                           ObservationSerializer.DeserializeComments(e.ObservationsCSV)))
                        .ToList();
    }
}
