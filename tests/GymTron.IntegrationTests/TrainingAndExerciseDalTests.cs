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

    [Fact]
    public async Task Training_HasActiveTraining_IdentifiesActiveAndCompleted()
    {
        int routineId = await Database.SeedRoutineAsync();
        TrainingDAL dal = new(ConnectionString);
        int userId = 42;

        Assert.False(await dal.HasActiveTraining(userId));

        Training training = Training.CreateAnStartedTraining(routineId, 1, [], new SystemClock(), userId);
        await dal.Add(training);

        Assert.True(await dal.HasActiveTraining(userId));
        Assert.False(await dal.HasActiveTraining(999));

        var current = await dal.GetCurrent(userId);
        Assert.NotNull(current);
        current.StatusType = (int)EntityStatusTypes.COMPLETED;
        current.CompletedOn = DateTime.UtcNow;
        await dal.Update(current);

        Assert.False(await dal.HasActiveTraining(userId));
    }

    [Fact]
    public async Task Training_ListCompletedHistory_ReturnsSortedByDate()
    {
        int routineId = await Database.SeedRoutineAsync();
        TrainingDAL dal = new(ConnectionString);
        int userId = 77;

        Training t1 = Training.CreateAnStartedTraining(routineId, 1, [], new SystemClock(), userId);
        await dal.Add(t1);
        var cur1 = await dal.GetCurrent(userId);
        Assert.NotNull(cur1);
        cur1.StatusType = (int)EntityStatusTypes.COMPLETED;
        cur1.StartedOn = new DateTime(2026, 1, 1, 10, 0, 0);
        cur1.CompletedOn = new DateTime(2026, 1, 1, 11, 0, 0);
        await dal.Update(cur1);

        Training t2 = Training.CreateAnStartedTraining(routineId, 2, [], new SystemClock(), userId);
        await dal.Add(t2);
        var cur2 = await dal.GetCurrent(userId);
        Assert.NotNull(cur2);
        cur2.StatusType = (int)EntityStatusTypes.COMPLETED;
        cur2.StartedOn = new DateTime(2026, 1, 5, 10, 0, 0);
        cur2.CompletedOn = new DateTime(2026, 1, 5, 11, 0, 0);
        await dal.Update(cur2);

        var history = (await dal.ListCompletedHistory(userId)).ToList();
        Assert.Equal(2, history.Count);
        Assert.Equal(new DateTime(2026, 1, 5, 10, 0, 0), history[0].StartedOn);
        Assert.Equal(new DateTime(2026, 1, 1, 10, 0, 0), history[1].StartedOn);
    }

    [Fact]
    public async Task UnitOfWork_ExecuteInTransaction_RollsBackOnFailureAndCommitsOnSuccess()
    {
        int routineId = await Database.SeedRoutineAsync();
        int parameterId = await Database.SeedParameterAsync("Bench Press");
        int trainingId = await Database.SeedTrainingAsync(routineId);

        await using GymTron.Infrastructure.Persistence.UnitOfWork uow = new(ConnectionString);
        TrainingDAL trainingDal = new(ConnectionString, uow);
        ExerciseDAL exerciseDal = new(ConnectionString, uow);

        // 1. Rollback test: simulate error during transaction
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await uow.ExecuteInTransactionAsync(async ct =>
            {
                TrainingDALModel? model = await trainingDal.GetById(trainingId, ct);
                Assert.NotNull(model);
                model.StatusType = (int)EntityStatusTypes.COMPLETED;
                model.CompletedOn = DateTime.UtcNow;
                await trainingDal.Update(model, ct);

                Exercise ex = Exercise.New(trainingId, parameterId, "Bench Press", 80m, 0, 10, ["fail"]);
                await exerciseDal.AddRange([ex], ct);

                throw new InvalidOperationException("Force rollback");
            });
        });

        // Assert: training is still active and no exercise was inserted
        TrainingDAL checkDal = new(ConnectionString);
        var unchangedTraining = await checkDal.GetById(trainingId);
        Assert.NotNull(unchangedTraining);
        Assert.Equal(1, unchangedTraining.StatusType);
        Assert.Null(unchangedTraining.CompletedOn);
        Assert.Empty(await new ExerciseDAL(ConnectionString).ListByTrainingId(trainingId));

        // 2. Commit test: successful transaction commits both training and exercise
        await uow.ExecuteInTransactionAsync(async ct =>
        {
            TrainingDALModel? model = await trainingDal.GetById(trainingId, ct);
            Assert.NotNull(model);
            model.StatusType = (int)EntityStatusTypes.COMPLETED;
            model.CompletedOn = new DateTime(2026, 3, 10, 12, 0, 0);
            await trainingDal.Update(model, ct);

            Exercise ex = Exercise.New(trainingId, parameterId, "Bench Press", 85m, 0, 8, ["committed"]);
            await exerciseDal.AddRange([ex], ct);
        });

        var committedTraining = await checkDal.GetById(trainingId);
        Assert.NotNull(committedTraining);
        Assert.Equal((int)EntityStatusTypes.COMPLETED, committedTraining.StatusType);
        var exercises = await new ExerciseDAL(ConnectionString).ListByTrainingId(trainingId);
        Assert.Single(exercises, e => e.Weight == 85m && e.Repetitions == 8);
    }
}
