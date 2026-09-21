using GymTron.Domain.Aggregates;
using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Domain.Repositories;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL;
using GymTron.Infrastructure.Persistence.Repositories;
using NSubstitute;

namespace GymTron.UnitTests.Infrastructure;

public class TrainingRepositoryTests
{
    [Fact]
    public async Task AddAndUpdate_DelegateDomainAndMappedDalModel()
    {
        ITrainingDAL dal = Substitute.For<ITrainingDAL>();
        TrainingRepository repository = CreateRepository(dal);
        DateTime startedOn = new(2026, 1, 2, 3, 4, 5);
        DateTime completedOn = startedOn.AddHours(1);
        Training training = Training.FromDatabase(5, 7, 2, startedOn, completedOn,
            EntityStatusTypes.COMPLETED, [], []);

        await repository.Add(training);
        await repository.Update(training);

        await dal.Received(1).Add(training);
        await dal.Received(1).Update(Arg.Is<TrainingDALModel>(model =>
            model.Id == 5 && model.RoutineId == 7 && model.DayOfWeek == 2
            && model.StartedOn == startedOn && model.CompletedOn == completedOn
            && model.StatusType == (int)EntityStatusTypes.COMPLETED));
    }

    [Fact]
    public async Task GetCurrentAndGetById_MapRoutineDayAndCompletedExercises()
    {
        ITrainingDAL dal = Substitute.For<ITrainingDAL>();
        IRoutineRepository routines = Substitute.For<IRoutineRepository>();
        IExerciseRepository exercises = Substitute.For<IExerciseRepository>();
        DateTime startedOn = new(2026, 2, 3);
        TrainingDALModel model = new()
        {
            Id = 5, RoutineId = 7, DayOfWeek = 2, StartedOn = startedOn,
            StatusType = (int)EntityStatusTypes.ACTIVE
        };
        dal.GetCurrent().Returns(model);
        dal.GetById(5).Returns(model);
        RoutineItem pending = CreateRoutineItem(11, 2);
        routines.GetById(7).Returns(Routine.FromDatabase(7, "Routine", [pending, CreateRoutineItem(12, 3)]));
        Exercise completed = Exercise.FromDatabase(4, 5, 11, "Row", 50, 0, 8, startedOn, []);
        exercises.ListByTraining(5).Returns([completed]);
        TrainingRepository repository = new(dal, routines, exercises);

        Training? current = await repository.GetCurrent();
        Training? byId = await repository.GetById(5);

        Assert.NotNull(current);
        Assert.NotNull(byId);
        Assert.Same(pending, Assert.Single(current.PendingWorkout));
        Assert.Same(completed, Assert.Single(current.CompletedWorkout));
        Assert.Equal((5, 7, 2, startedOn), (current.Id, current.RoutineId, current.DayOfTheWeek, current.StartedOn.FullDate));
        await routines.Received(2).GetById(7);
        await exercises.Received(2).ListByTraining(5);
    }

    [Fact]
    public async Task GetMethods_WhenTrainingOrRoutineIsMissing_ReturnNullWithoutLoadingExercises()
    {
        ITrainingDAL dal = Substitute.For<ITrainingDAL>();
        IRoutineRepository routines = Substitute.For<IRoutineRepository>();
        IExerciseRepository exercises = Substitute.For<IExerciseRepository>();
        dal.GetCurrent().Returns((TrainingDALModel?)null);
        dal.GetById(5).Returns(new TrainingDALModel { Id = 5, RoutineId = 7 });
        routines.GetById(7).Returns((Routine?)null);
        TrainingRepository repository = new(dal, routines, exercises);

        Assert.Null(await repository.GetCurrent());
        Assert.Null(await repository.GetById(5));

        await exercises.DidNotReceive().ListByTraining(Arg.Any<int>());
    }

    [Fact]
    public async Task ListAllWithoutExercises_MapsEveryTrainingInSourceOrder()
    {
        ITrainingDAL dal = Substitute.For<ITrainingDAL>();
        DateTime first = new(2026, 1, 1);
        DateTime second = new(2026, 2, 1);
        dal.ListAll().Returns([
            new TrainingDALModel { Id = 2, RoutineId = 8, DayOfWeek = 3, StartedOn = second, CompletedOn = second.AddHours(1), StatusType = (int)EntityStatusTypes.COMPLETED },
            new TrainingDALModel { Id = 1, RoutineId = 7, DayOfWeek = 2, StartedOn = first, StatusType = (int)EntityStatusTypes.ACTIVE }
        ]);
        TrainingRepository repository = CreateRepository(dal);

        List<Training> result = await repository.ListAllWithoutExercises();

        Assert.Equal([2, 1], result.Select(item => item.Id));
        Assert.Equal(second.AddHours(1), result[0].CompletedOn!.FullDate);
        Assert.Empty(result[0].PendingWorkout);
        Assert.Empty(result[0].CompletedWorkout);
        Assert.Null(result[1].CompletedOn);
    }

    [Fact]
    public async Task ListCompletedHistory_MapsOnlyCompletedTrainings()
    {
        ITrainingDAL dal = Substitute.For<ITrainingDAL>();
        DateTime first = new(2026, 1, 1);
        DateTime second = new(2026, 2, 1);
        dal.ListAll().Returns([
            new TrainingDALModel { Id = 2, RoutineId = 8, DayOfWeek = 3, StartedOn = second, CompletedOn = second.AddHours(1), StatusType = (int)EntityStatusTypes.COMPLETED },
            new TrainingDALModel { Id = 1, RoutineId = 7, DayOfWeek = 2, StartedOn = first, StatusType = (int)EntityStatusTypes.ACTIVE }
        ]);
        TrainingRepository repository = CreateRepository(dal);

        var result = await repository.ListCompletedHistory();

        var item = Assert.Single(result);
        Assert.Equal(3, item.DayOfTheWeek);
        Assert.Equal(second, item.StartedOn.FullDate);
    }

    private static TrainingRepository CreateRepository(ITrainingDAL dal) => new(
        dal, Substitute.For<IRoutineRepository>(), Substitute.For<IExerciseRepository>());

    private static RoutineItem CreateRoutineItem(int id, int day) => RoutineItem.FromDatabase(
        id, day,
        ExerciseParameters.FromDatabase(id, "Exercise", "Description", "Pattern", 3, (8, 12), 0, null,
            (60, 90), null, null, null, ExerciseTypes.WEIGHT, []),
        false, 1);
}
