using GymTron.Domain.Aggregates;
using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL;
using GymTron.Infrastructure.Persistence.Repositories;
using GymTron.IntegrationTests.Database;
using Microsoft.Extensions.Caching.Memory;

namespace GymTron.IntegrationTests;

[Collection(MySqlCollection.Name)]
public sealed class RepositoryTests(MySqlCollectionFixture fixture) : MySqlIntegrationTest(fixture)
{
    [Fact]
    public async Task ExerciseParameterRepository_GetById_WithNonExistentId_ReturnsNull()
    {
        ExerciseParameterDAL dal = new(ConnectionString);
        ExerciseParameterRepository repository = new(dal);

        ExerciseParameters? result = await repository.GetById(999_999);

        Assert.Null(result);
    }

    [Fact]
    public async Task RoutineRepository_ListAll_CachesResultAndInvalidatesOnWrite()
    {
        RoutineDAL dal = new(ConnectionString);
        IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        CachedRoutineRepository repository = new(new RoutineRepository(dal), cache);

        int parameterId = await Database.SeedParameterAsync("Squat", (int)ExerciseTypes.WEIGHT);
        RoutineItemWriteModel[] items =
        [
            new()
            {
                DayOfWeek = 1, ExerciseParametersId = parameterId, Series = 3, RepetitionsMin = 8,
                RepetitionsMax = 12, Duration = null, MinRestTimeInSeconds = 60, MaxRestTimeInSeconds = 90,
                AlternatingSeries = false, Position = 1, Type = ExerciseTypes.WEIGHT
            }
        ];

        List<RoutineItem> routineItems = items.Select(i => RoutineItem.Create(
            i.DayOfWeek,
            ExerciseParameters.FromDatabase(
                i.ExerciseParametersId, "", "", "", i.Series, (i.RepetitionsMin, i.RepetitionsMax),
                i.Duration ?? 0, null, (i.MinRestTimeInSeconds, i.MaxRestTimeInSeconds ?? 0),
                null, null, null, i.Type, []),
            i.AlternatingSeries,
            i.Position)).ToList();

        await repository.Create(Routine.Create("Cached Routine", routineItems));

        List<Routine> firstCall = await repository.ListAll();
        List<Routine> secondCall = await repository.ListAll();

        Assert.Single(firstCall);
        Assert.Single(secondCall);
        Assert.Equal("Cached Routine", firstCall[0].Name);
    }

    [Fact]
    public async Task TrainingRepository_GetById_WithNonExistentId_ReturnsNull()
    {
        TrainingDAL trainingDal = new(ConnectionString);
        RoutineDAL routineDal = new(ConnectionString);
        ExerciseDAL exerciseDal = new(ConnectionString);
        RoutineRepository routineRepository = new(routineDal);
        ExerciseRepository exerciseRepository = new(exerciseDal);
        TrainingRepository repository = new(trainingDal, routineRepository, exerciseRepository);

        Training? result = await repository.GetById(999_999);

        Assert.Null(result);
    }

    [Fact]
    public async Task TrainingRepository_GetById_WithExistingTrainingButMissingRoutine_ReturnsNull()
    {
        int routineId = await Database.SeedRoutineAsync();
        int trainingId = await Database.SeedTrainingAsync(routineId);

        await Database.ExecuteAsync("SET FOREIGN_KEY_CHECKS = 0; DELETE FROM routines WHERE id = @Id; SET FOREIGN_KEY_CHECKS = 1;", new { Id = routineId });

        TrainingDAL trainingDal = new(ConnectionString);
        RoutineDAL routineDal = new(ConnectionString);
        ExerciseDAL exerciseDal = new(ConnectionString);
        RoutineRepository routineRepository = new(routineDal);
        ExerciseRepository exerciseRepository = new(exerciseDal);
        TrainingRepository repository = new(trainingDal, routineRepository, exerciseRepository);

        Training? result = await repository.GetById(trainingId);

        Assert.Null(result);
    }

    [Fact]
    public async Task TrainingRepository_GetById_WithCompletedTraining_MapsCompletedOn()
    {
        int parameterId = await Database.SeedParameterAsync("Squat", (int)ExerciseTypes.WEIGHT);
        RoutineDAL routineDal = new(ConnectionString);
        RoutineItemWriteModel[] items =
        [
            new()
            {
                DayOfWeek = 2, ExerciseParametersId = parameterId, Series = 3, RepetitionsMin = 8,
                RepetitionsMax = 12, Duration = null, MinRestTimeInSeconds = 60, MaxRestTimeInSeconds = 90,
                AlternatingSeries = false, Position = 1, Type = ExerciseTypes.WEIGHT
            }
        ];
        int routineId = await routineDal.Create("Test Routine", items);

        DateTime completedOn = new(2026, 3, 4, 5, 6, 7, DateTimeKind.Unspecified);
        int trainingId = await Database.SeedTrainingAsync(routineId, completedOn: completedOn, status: (int)EntityStatusTypes.COMPLETED);

        TrainingDAL trainingDal = new(ConnectionString);
        ExerciseDAL exerciseDal = new(ConnectionString);
        RoutineRepository routineRepository = new(routineDal);
        ExerciseRepository exerciseRepository = new(exerciseDal);
        TrainingRepository repository = new(trainingDal, routineRepository, exerciseRepository);

        Training? result = await repository.GetById(trainingId);

        Assert.NotNull(result);
        Assert.True(result.Status.IsCompleted);
        Assert.NotNull(result.CompletedOn);
    }
}
