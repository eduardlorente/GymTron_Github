using GymTron.Domain.Enums;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL;
using GymTron.IntegrationTests.Database;
using MySqlConnector;

namespace GymTron.IntegrationTests;

[Collection(MySqlCollection.Name)]
public sealed class RoutineDalTests(MySqlCollectionFixture fixture) : MySqlIntegrationTest(fixture)
{
    [Fact]
    public async Task CreateAndRead_MapsWeightAndDurationItems()
    {
        RoutineDAL dal = new(ConnectionString);
        int weightId = await Database.SeedParameterAsync("Weighted", (int)ExerciseTypes.WEIGHT);
        int durationId = await Database.SeedParameterAsync("Timed", (int)ExerciseTypes.DURATION);
        RoutineItemWriteModel[] items =
        [
            new()
            {
                DayOfWeek = 1, ExerciseParametersId = weightId, Series = 3, RepetitionsMin = 8,
                RepetitionsMax = 12, Duration = null, MinRestTimeInSeconds = 60, MaxRestTimeInSeconds = 120,
                AlternatingSeries = false, Position = 1, Type = ExerciseTypes.WEIGHT
            },
            new()
            {
                DayOfWeek = 1, ExerciseParametersId = durationId, Series = 2, RepetitionsMin = 0,
                RepetitionsMax = 0, Duration = 45, MinRestTimeInSeconds = 30, MaxRestTimeInSeconds = null,
                AlternatingSeries = true, Position = 2, Type = ExerciseTypes.DURATION
            }
        ];

        int routineId = await dal.Create("Initial", items);

        var rows = (await dal.ListById(routineId)).ToList();
        Assert.Equal(2, rows.Count);
        Assert.All(rows, row => Assert.Equal("Initial", row.RoutineName));
        var weight = Assert.Single(rows, row => row.ExerciseParametersId == weightId);
        Assert.Equal((8, 12, (int?)null), (weight.RepetitionsMin, weight.RepetitionsMax, weight.Duration));
        var duration = Assert.Single(rows, row => row.ExerciseParametersId == durationId);
        Assert.Equal((0, 0, (int?)45), (duration.RepetitionsMin, duration.RepetitionsMax, duration.Duration));
        Assert.True(duration.AlternatingSeries);
        Assert.Single(await dal.ListAll(), row => row.RoutineId == routineId && row.RoutineItemId == weight.RoutineItemId);
    }

    [Fact]
    public async Task Update_ReplacesItemsButPreservesRoutine()
    {
        RoutineDAL dal = new(ConnectionString);
        int weightId = await Database.SeedParameterAsync("Weighted", (int)ExerciseTypes.WEIGHT);
        RoutineItemWriteModel[] items =
        [
            new()
            {
                DayOfWeek = 1, ExerciseParametersId = weightId, Series = 3, RepetitionsMin = 8,
                RepetitionsMax = 10, MinRestTimeInSeconds = 60, AlternatingSeries = false, Position = 1,
                Type = ExerciseTypes.WEIGHT
            }
        ];
        int routineId = await dal.Create("Keep parent", items);

        await dal.Update(routineId, "Updated", []);
        var rows = (await dal.ListById(routineId)).ToList();

        var parent = Assert.Single(rows);
        Assert.Equal("Updated", parent.RoutineName);
        Assert.Equal(0, parent.RoutineItemId);
    }

    [Fact]
    public async Task Create_WithInvalidForeignKey_RollsBackWithoutPersistingRoutine()
    {
        RoutineDAL dal = new(ConnectionString);
        RoutineItemWriteModel[] items =
        [
            new()
            {
                DayOfWeek = 1, ExerciseParametersId = 999_999, Series = 3, RepetitionsMin = 8,
                RepetitionsMax = 10, MinRestTimeInSeconds = 60, AlternatingSeries = false, Position = 1,
                Type = ExerciseTypes.WEIGHT
            }
        ];

        await Assert.ThrowsAnyAsync<Exception>(() => dal.Create("Should Rollback", items));

        int routineCount = await Database.QuerySingleAsync<int>("SELECT COUNT(*) FROM routines");
        int itemCount = await Database.QuerySingleAsync<int>("SELECT COUNT(*) FROM routine_items");
        Assert.Equal(0, routineCount);
        Assert.Equal(0, itemCount);
    }

    [Fact]
    public async Task Update_WithInvalidForeignKey_RollsBackWithoutLosingOriginalItems()
    {
        RoutineDAL dal = new(ConnectionString);
        int weightId = await Database.SeedParameterAsync("Weighted", (int)ExerciseTypes.WEIGHT);
        RoutineItemWriteModel[] originalItems =
        [
            new()
            {
                DayOfWeek = 1, ExerciseParametersId = weightId, Series = 3, RepetitionsMin = 8,
                RepetitionsMax = 10, MinRestTimeInSeconds = 60, AlternatingSeries = false, Position = 1,
                Type = ExerciseTypes.WEIGHT
            }
        ];
        int routineId = await dal.Create("Original", originalItems);

        RoutineItemWriteModel[] failingItems =
        [
            new()
            {
                DayOfWeek = 2, ExerciseParametersId = 999_999, Series = 4, RepetitionsMin = 6,
                RepetitionsMax = 8, MinRestTimeInSeconds = 45, AlternatingSeries = false, Position = 1,
                Type = ExerciseTypes.WEIGHT
            }
        ];

        await Assert.ThrowsAnyAsync<Exception>(() => dal.Update(routineId, "Should Rollback", failingItems));

        var rows = (await dal.ListById(routineId)).ToList();
        Assert.Single(rows, row => row.RoutineName == "Original" && row.ExerciseParametersId == weightId);
    }

    [Fact]
    public async Task Routine_ListById_WithUserId_RetrievesUserSpecificLastExerciseMetrics()
    {
        int weightId = await Database.SeedParameterAsync("Squat", (int)ExerciseTypes.WEIGHT);
        RoutineDAL routineDal = new(ConnectionString);
        RoutineItemWriteModel[] items =
        [
            new()
            {
                DayOfWeek = 1, ExerciseParametersId = weightId, Series = 3, RepetitionsMin = 5,
                RepetitionsMax = 5, Duration = null, MinRestTimeInSeconds = 120, MaxRestTimeInSeconds = null,
                AlternatingSeries = false, Position = 1, Type = ExerciseTypes.WEIGHT
            }
        ];

        int user1 = 101;
        int user2 = 102;
        int routineId = await routineDal.Create("Squat Routine", items, userId: null);

        // Seed training and exercise for user 1
        int trainingUser1 = await Database.SeedTrainingAsync(routineId, userId: user1);
        ExerciseDAL exerciseDal = new(ConnectionString);
        await exerciseDal.AddRange([GymTron.Domain.Entities.Exercise.New(trainingUser1, weightId, "Squat", 120m, 0, 5, ["user1 clean"])]);

        // Seed newer training and exercise for user 2
        int trainingUser2 = await Database.SeedTrainingAsync(routineId, userId: user2);
        await exerciseDal.AddRange([GymTron.Domain.Entities.Exercise.New(trainingUser2, weightId, "Squat", 150m, 0, 3, ["user2 heavy"])]);

        // Query routine with user1 context
        var user1Details = (await routineDal.ListById(routineId, user1)).ToList();
        var item1 = Assert.Single(user1Details);
        Assert.Equal(120m, item1.LastWeight);
        Assert.Equal(5, item1.LastRepetitions);
        Assert.Equal("[\"user1 clean\"]", item1.LastObservations);

        // Query routine with user2 context
        var user2Details = (await routineDal.ListById(routineId, user2)).ToList();
        var item2 = Assert.Single(user2Details);
        Assert.Equal(150m, item2.LastWeight);
        Assert.Equal(3, item2.LastRepetitions);
        Assert.Equal("[\"user2 heavy\"]", item2.LastObservations);
    }
}
