using GymTron.Domain.Enums;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL;
using GymTron.IntegrationTests.Database;
using MySql.Data.MySqlClient;

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
}
