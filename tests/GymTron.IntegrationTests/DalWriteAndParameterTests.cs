using GymTron.Domain.Entities;
using GymTron.Infrastructure.Persistence.DAL.MySQL;
using GymTron.IntegrationTests.Database;

namespace GymTron.IntegrationTests;

[Collection(MySqlCollection.Name)]
public sealed class DalWriteAndParameterTests(MySqlCollectionFixture fixture) : MySqlIntegrationTest(fixture)
{
    [Fact]
    public async Task BodyWeight_AddThenListAll_PreservesValuesAndTimestamp()
    {
        DateTime createdOn = new(2026, 2, 3, 4, 5, 6, DateTimeKind.Unspecified);
        BodyWeightDAL dal = new(ConnectionString);

        await dal.Add(BodyWeight.New(81.25m, 17.5m, createdOn));
        var row = Assert.Single(await dal.ListAll());

        Assert.Equal((81.25m, 17.5m, createdOn), (row.Weight, row.BodyFatPercentage, row.CreatedOn));
    }

    [Fact]
    public async Task Log_Add_PersistsMessageAndCreationTime()
    {
        Log log = Log.New("integration details");

        await new LogDAL(ConnectionString).Add(log);
        LogRow row = await Database.QuerySingleAsync<LogRow>(
            "SELECT message AS Message, created_on AS CreatedOn FROM logs");

        Assert.Equal(log.Message, row.Message);
        long expectedTicks = (log.Status.CreatedOn.Ticks / 10) * 10;
        long actualTicks = (row.CreatedOn.Ticks / 10) * 10;
        Assert.Equal(expectedTicks, actualTicks);
    }

    [Fact]
    public async Task ExerciseParameter_CreateUpdateListAndGet_MapsNullableValues()
    {
        ExerciseParameterDAL dal = new(ConnectionString);
        int id = await dal.Create("Zulu", "old", "push", 1, null);
        await dal.Create("Alpha", "other", "core", 2, 3);

        Assert.Null(await dal.GetById(999_999));
        var created = Assert.IsType<GymTron.Infrastructure.Persistence.DAL.Models.ExerciseParameterDALModel>(await dal.GetById(id));
        Assert.Null(created.ReplaysInReserve);

        await dal.Update(id, "Beta", "updated", "pull", 2, 4);
        var all = (await dal.ListAll()).ToList();

        Assert.Equal(["Alpha", "Beta"], all.Select(item => item.Name));
        var updated = Assert.Single(all, item => item.Id == id);
        Assert.Equal(("updated", "pull", 2, 4), (updated.Description, updated.Pattern, updated.TypeId, updated.ReplaysInReserve));
    }

    private sealed class LogRow
    {
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; }
    }
}
