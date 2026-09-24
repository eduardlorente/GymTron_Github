using GymTron.Domain.Aggregates;
using GymTron.Domain.Entities;
using GymTron.Domain.Projections;
using GymTron.Domain.Repositories;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL;

namespace GymTron.Infrastructure.Persistence.Repositories;

internal class TrainingRepository(ITrainingDAL trainingDAL,
                                  IRoutineRepository routineRepository,
                                  IExerciseRepository exerciseRepository) : ITrainingRepository
{


    private readonly ITrainingDAL _trainingDAL = trainingDAL;
    private readonly IRoutineRepository _routineRepository = routineRepository;
    private readonly IExerciseRepository _exerciseRepository = exerciseRepository;


    public async Task Add(Training entity, CancellationToken cancellationToken = default)
    {
        await _trainingDAL.Add(entity, cancellationToken);
    }


    public async Task<Training?> GetById(int id, CancellationToken cancellationToken = default)
    {
        TrainingDALModel? trainingData = await _trainingDAL.GetById(id, cancellationToken);

        return await BuildTrainingFromData(trainingData, cancellationToken);
    }


    public async Task<Training?> GetCurrent(int? userId = null, CancellationToken cancellationToken = default)
    {
        TrainingDALModel? trainingData = await _trainingDAL.GetCurrent(userId, cancellationToken);

        return await BuildTrainingFromData(trainingData, cancellationToken);
    }


    public async Task<bool> HasActiveTraining(int userId, CancellationToken cancellationToken = default)
    {
        return await _trainingDAL.HasActiveTraining(userId, cancellationToken);
    }


    public async Task Update(Training entity, CancellationToken cancellationToken = default)
    {
        await _trainingDAL.Update(new TrainingDALModel(entity), cancellationToken);
    }


    public async Task<List<Training>> ListAllWithoutExercises(CancellationToken cancellationToken = default)
    {
        List<TrainingDALModel> trainings = await _trainingDAL.ListAll(null, cancellationToken);

        return trainings.Select(t => Training.FromDatabase(t.Id,
                                                          t.RoutineId,
                                                          t.DayOfWeek,
                                                          t.StartedOn,
                                                          t.CompletedOn,
                                                          (Domain.Enums.EntityStatusTypes)t.StatusType,
                                                          [],
                                                          [],
                                                          t.UserId)).ToList();
    }

    public async Task<List<TrainingHistoryProjection>> ListCompletedHistory(int? userId = null, CancellationToken cancellationToken = default)
    {
        if (!userId.HasValue) return [];

        var completed = await _trainingDAL.ListCompletedHistory(userId.Value, cancellationToken);
        return completed.Select(t => new TrainingHistoryProjection
        {
            StartedOn = new Domain.ValueObjects.TrainingDate(t.StartedOn),
            DayOfTheWeek = t.DayOfWeek
        }).ToList();
    }


    private async Task<Training?> BuildTrainingFromData(TrainingDALModel? trainingData, CancellationToken cancellationToken)
    {
        if (trainingData != null)
        {
            Routine? routine = await _routineRepository.GetById(trainingData.RoutineId, trainingData.UserId, cancellationToken);

            if (routine != null)
            {
                List<Exercise> completedExercises = await _exerciseRepository.ListByTraining(trainingData.Id, cancellationToken);

                var pendingExercises = routine.WorkByDays.TryGetValue(trainingData.DayOfWeek, out var exercisesForDay)
                    ? exercisesForDay
                    : [];

                return Training.FromDatabase(trainingData.Id,
                                             trainingData.RoutineId,
                                             trainingData.DayOfWeek,
                                             trainingData.StartedOn,
                                             trainingData.CompletedOn,
                                             (Domain.Enums.EntityStatusTypes)trainingData.StatusType,
                                             pendingExercises,
                                             completedExercises,
                                             trainingData.UserId);
            }
        }

        return null;
    }
}
