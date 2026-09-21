using GymTron.Domain.Aggregates;
using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL;
using GymTron.Infrastructure.Services;
using GymTron.IntegrationTests.Database;

namespace GymTron.IntegrationTests;

[Collection(MySqlCollection.Name)]
public sealed class TrainingAndExerciseDalTests(MySqlCollectionFixture fixture) : MySqlIntegrationTest(fixture)
{
    [Fact]
    public async Task Training_AddGetListCurrentAndUpdate_HandlesNullAndNonNullPaths()
    {
        int routineId = await Database.SeedRoutineAsync();
        TrainingDAL dal = new(ConnectionString);
        Training training = Training.CreateAnStartedTraining(routineId, 3, [], new SystemClock());

        Assert.Null(await dal.GetById(999_999));
        Assert.Null(await dal.GetCurrent());
        await dal.Add(training);

        TrainingDALModel current = Assert.IsType<TrainingDALModel>(await dal.GetCurrent());
        Assert.Equal((routineId, 3, 1), (current.RoutineId, current.DayOfWeek, current.StatusType));
        Assert.Null(current.CompletedOn);
        Assert.Single(await dal.ListAll());

        DateTime completedOn = new(2026, 3, 4, 5, 6, 7);
        current.CompletedOn = completedOn;
        current.StatusType = (int)EntityStatusTypes.COMPLETED;
        await dal.Update(current);

        TrainingDALModel updated = Assert.IsType<TrainingDALModel>(await dal.GetById(current.Id));
        Assert.Equal(completedOn, updated.CompletedOn);
        Assert.Equal((int)EntityStatusTypes.COMPLETED, updated.StatusType);
        Assert.Null(await dal.GetCurrent());
    }

    [Fact]
    public async Task Exercise_AddRangeAndLists_MapJoinedNamesAndExerciseFields()
    {
        int routineId = await Database.SeedRoutineAsync();
        int parameterId = await Database.SeedParameterAsync("Deadlift");
        int trainingId = await Database.SeedTrainingAsync(routineId);
        ExerciseDAL dal = new(ConnectionString);
        Exercise first = Exercise.New(trainingId, parameterId, "ignored", 100.5m, 0, 5, ["steady", "clean"]);
        Exercise second = Exercise.New(trainingId, parameterId, "ignored", 90m, 30, 8, ["tempo"]);

        await dal.AddRange([first, second]);
        var byTraining = (await dal.ListByTrainingId(trainingId)).ToList();
        var all = (await dal.ListAll()).ToList();

        Assert.Equal(2, byTraining.Count);
        Assert.Equal(2, all.Count);
        Assert.All(byTraining, row => Assert.Equal((trainingId, parameterId, "Deadlift"), (row.TrainingId, row.ExerciseParametersId, row.Name)));
        Assert.Contains(byTraining, row => row.Weight == 100.5m && row.Repetitions == 5 && row.ObservationsCSV == "[\"steady\",\"clean\"]");
        Assert.All(all, row => Assert.Equal((parameterId, "Deadlift"), (row.ExerciseParametersId, row.Name)));
    }

    [Fact]
    public async Task Exercise_Add_PersistsSingleExerciseUsingTheDomainMappingContract()
    {
        int routineId = await Database.SeedRoutineAsync();
        int parameterId = await Database.SeedParameterAsync("Plank", (int)ExerciseTypes.DURATION);
        int trainingId = await Database.SeedTrainingAsync(routineId);
        ExerciseDAL dal = new(ConnectionString);

        await dal.Add(Exercise.New(trainingId, parameterId, "ignored", 0, 45, 0, ["aligned"]));
        var row = Assert.Single(await dal.ListByTrainingId(trainingId));

        Assert.Equal((45, 0, "[\"aligned\"]"), (row.DurationInSeconds, row.Repetitions, row.ObservationsCSV));
    }
}
