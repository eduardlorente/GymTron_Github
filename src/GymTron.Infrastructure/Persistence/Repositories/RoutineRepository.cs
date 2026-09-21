using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Domain.Projections;
using GymTron.Domain.Repositories;
using GymTron.Domain.ValueObjects;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL;
using GymTron.Infrastructure.Persistence.Serialization;

namespace GymTron.Infrastructure.Persistence.Repositories;

internal class RoutineRepository(IRoutineDAL routineDAL) : IRoutineRepository
{
    private readonly IRoutineDAL _routineDAL = routineDAL;

    public async Task<List<Routine>> ListAll(int? userId = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<RoutineFullDetailsDTO> routines = await _routineDAL.ListAll(userId, cancellationToken);

        return BuildRoutineFromData(routines ?? []);
    }

    public async Task<Routine?> GetById(int id, CancellationToken cancellationToken = default)
    {
        IEnumerable<RoutineFullDetailsDTO> routines = await _routineDAL.ListById(id, cancellationToken);

        return BuildRoutineFromData(routines ?? [])
            .FirstOrDefault();
    }

    public async Task<RoutineProjection?> GetRoutineProjection(int id, CancellationToken cancellationToken = default)
    {
        IEnumerable<RoutineFullDetailsDTO> routines = await _routineDAL.ListById(id, cancellationToken);

        return BuildRoutineProjectionFromData(routines ?? [])
            .FirstOrDefault();
    }

    public async Task<List<RoutineProjection>> ListRoutineProjections(int? userId = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<RoutineFullDetailsDTO> routines = await _routineDAL.ListAll(userId, cancellationToken);

        return BuildRoutineProjectionFromData(routines ?? []);
    }

    public async Task<int> Create(Routine routine, CancellationToken cancellationToken = default)
    {
        return await _routineDAL.Create(routine.Name, MapItems(routine.RoutineExercises), routine.UserId, cancellationToken);
    }

    public async Task Update(Routine routine, CancellationToken cancellationToken = default)
    {
        await _routineDAL.Update(routine.Id, routine.Name, MapItems(routine.RoutineExercises), cancellationToken);
    }

    private static List<RoutineProjection> BuildRoutineProjectionFromData(IEnumerable<RoutineFullDetailsDTO> routinesData)
        => [.. routinesData
            .GroupBy(r => r.RoutineId)
            .Select(g => new RoutineProjection
            {
                Id = g.Key,
                UserId = g.First().UserId,
                Name = g.First().RoutineName,
                Items = g.Select(item => new RoutineItemProjection
                {
                    Id = item.RoutineItemId,
                    DayOfWeek = item.DayOfWeek,
                    ExerciseParametersId = item.ExerciseParametersId,
                    ExerciseName = item.ExerciseName,
                    Series = item.Series,
                    RepetitionsMin = item.RepetitionsMin,
                    RepetitionsMax = item.RepetitionsMax,
                    Duration = item.Duration,
                    MinRestTimeInSeconds = item.MinRestTimeInSeconds,
                    MaxRestTimeInSeconds = item.MaxRestTimeInSeconds,
                    AlternatingSeries = item.AlternatingSeries,
                    Position = item.Position,
                    Type = (ExerciseTypes)item.TypeId
                }).ToList()
            })];

    private static List<Routine> BuildRoutineFromData(IEnumerable<RoutineFullDetailsDTO> routinesData)
        => [.. routinesData
            .GroupBy(r => r.RoutineId)
            .Select(g => Routine.FromDatabase(
                            g.Key,
                            g.First().RoutineName,
                            [.. g.Select(item => RoutineItem.FromDatabase(
                                                item.RoutineItemId,
                                                item.DayOfWeek,
                                                ExerciseParameters.FromDatabase(
                                                    item.ExerciseParametersId,
                                                    item.ExerciseName,
                                                    item.Description,
                                                    item.Pattern,
                                                    item.Series,
                                                    (item.RepetitionsMin, item.RepetitionsMax),
                                                    item.Duration ?? 0,
                                                    item.ReplaysInReserve,
                                                    (item.MinRestTimeInSeconds, item.MaxRestTimeInSeconds ?? 0),
                                                    item.LastWeight,
                                                    item.LastDuration,
                                                    item.LastRepetitions,
                                                    (ExerciseTypes)item.TypeId,
                                                    ObservationSerializer.Deserialize(item.LastObservations)),
                                                item.AlternatingSeries,
                                                item.Position))
                            ],
                            g.First().UserId
                            ))];

    private static List<RoutineItemWriteModel> MapItems(IEnumerable<RoutineItem> items)
        => items.Select(item => new RoutineItemWriteModel
        {
            DayOfWeek = item.DayOfWeek,
            ExerciseParametersId = item.ExerciseParameters.Id,
            Series = item.ExerciseParameters.Series,
            RepetitionsMin = item.ExerciseParameters.Repetitions.Min,
            RepetitionsMax = item.ExerciseParameters.Repetitions.Max,
            Duration = item.ExerciseParameters.DurationInSeconds > 0 ? item.ExerciseParameters.DurationInSeconds : null,
            MinRestTimeInSeconds = item.ExerciseParameters.RestTimeInSeconds.Min,
            MaxRestTimeInSeconds = item.ExerciseParameters.RestTimeInSeconds.Max > 0 ? item.ExerciseParameters.RestTimeInSeconds.Max : null,
            AlternatingSeries = item.AlternatingSeries,
            Position = item.Position,
            Type = item.ExerciseParameters.Type
        }).ToList();
}
