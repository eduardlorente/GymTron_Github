using FluentValidation;
using GymTron.Application.Trainings.Commands;
using GymTron.Application.Trainings.Commands.Handlers;
using GymTron.Application.Trainings.Queries;
using GymTron.Application.Trainings.Queries.DTO;
using GymTron.Application.Trainings.Queries.Handlers;
using GymTron.Domain.Aggregates;
using GymTron.Domain.Common;
using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Domain.Exceptions;
using GymTron.Domain.Projections;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;
using GymTron.Domain.ValueObjects;
using GymTron.UnitTests.Helpers;
using MediatR;
using NSubstitute;

namespace GymTron.UnitTests.Application;

public class TrainingHandlerTests
{
    [Fact]
    public async Task Start_WhenRoutineIsMissing_LogsAndRethrowsWithoutAdding()
    {
        ITrainingRepository trainings = Substitute.For<ITrainingRepository>();
        IRoutineRepository routines = Substitute.For<IRoutineRepository>();
        IExceptionLogger<StartTrainingCommand> logger = Substitute.For<IExceptionLogger<StartTrainingCommand>>();
        trainings.HasActiveTraining(1, Arg.Any<CancellationToken>()).Returns(false);
        routines.GetById(7).Returns((Routine?)null);
        StartTrainingCommandHandler handler = new(Substitute.For<GymTron.Application.Common.Events.IDomainEventDispatcher>(), trainings, routines, logger, new FakeClock());

        EntityNotFoundException exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => handler.Handle(new StartTrainingCommand(Guid.NewGuid(), 7, 3, userId: 1), CancellationToken.None));

        await logger.Received(1).LogException(exception);
        await trainings.DidNotReceive().Add(Arg.Any<Training>());
    }

    [Fact]
    public async Task AddExercise_WeightAndDurationRequests_CompleteMatchingWorkoutItems()
    {
        IExceptionLogger<AddExerciseToTrainingCommand> logger = Substitute.For<IExceptionLogger<AddExerciseToTrainingCommand>>();
        Training weightTraining = ApplicationTestData.CreateTraining(pendingWorkout: [ApplicationTestData.CreateRoutineItem(11)]);
        Training durationTraining = ApplicationTestData.CreateTraining(pendingWorkout: [ApplicationTestData.CreateRoutineItem(12, type: ExerciseTypes.DURATION)]);
        AddExerciseToTrainingCommand weight = AddExerciseToTrainingCommand.New(Guid.NewGuid(), weightTraining, 11, "Squat", 90m, 8, ["Controlled"]);
        AddExerciseToTrainingCommand duration = AddExerciseToTrainingCommand.New(Guid.NewGuid(), durationTraining, 12, "Plank", 60, []);
        AddExerciseToTrainingCommandHandler handler = new(logger, new FakeClock());

        Training weightResult = await handler.Handle(weight, CancellationToken.None);
        Training durationResult = await handler.Handle(duration, CancellationToken.None);

        Exercise weighted = Assert.Single(weightResult.CompletedWorkout);
        Assert.Equal((90m, 8, 0), (weighted.Weight, weighted.CurrentRepetitions, weighted.DurationInSeconds));
        Assert.Equal("Controlled", Assert.Single(weighted.Observations).Comment);
        Exercise timed = Assert.Single(durationResult.CompletedWorkout);
        Assert.Equal((0m, 0, 60), (timed.Weight, timed.CurrentRepetitions, timed.DurationInSeconds));
    }



    [Fact]
    public async Task FinishTraining_WhenValid_PersistsStatusAndCompletedExercises()
    {
        GymTron.Application.Common.Events.IDomainEventDispatcher eventDispatcher = Substitute.For<GymTron.Application.Common.Events.IDomainEventDispatcher>();
        ITrainingRepository trainings = Substitute.For<ITrainingRepository>();
        IExerciseRepository exercises = Substitute.For<IExerciseRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<CancellationToken, Task>>()(callInfo.Arg<CancellationToken>()));

        Training training = ApplicationTestData.CreateTraining(completedWorkout: [Exercise.New(5, 11, "Squat", 90, 0, 8, [])]);

        IExceptionLogger<FinishTrainingCommand> logger = Substitute.For<IExceptionLogger<FinishTrainingCommand>>();
        await new FinishTrainingCommandHandler(eventDispatcher, trainings, exercises, unitOfWork, logger, new FakeClock())
            .Handle(new FinishTrainingCommand(Guid.NewGuid(), training), CancellationToken.None);

        Assert.True(training.Status.IsCompleted);
        Assert.NotNull(training.CompletedOn);
        await unitOfWork.Received(1).ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>());
        await trainings.Received(1).Update(training, Arg.Any<CancellationToken>());
        await exercises.Received(1).AddRange(training.CompletedWorkout, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelTraining_WhenValid_PersistsCancelledStatusAndDoesNotPersistExercises()
    {
        GymTron.Application.Common.Events.IDomainEventDispatcher eventDispatcher = Substitute.For<GymTron.Application.Common.Events.IDomainEventDispatcher>();
        ITrainingRepository trainings = Substitute.For<ITrainingRepository>();
        Training training = ApplicationTestData.CreateTraining(completedWorkout: [Exercise.New(5, 11, "Squat", 90, 0, 8, [])]);

        IExceptionLogger<CancelTrainingCommand> logger = Substitute.For<IExceptionLogger<CancelTrainingCommand>>();
        await new CancelTrainingCommandHandler(eventDispatcher, trainings, logger, new FakeClock())
            .Handle(new CancelTrainingCommand(Guid.NewGuid(), training), CancellationToken.None);

        Assert.Equal(EntityStatusTypes.CANCELLED, training.Status.Status);
        await trainings.Received(1).Update(training);
    }

    [Fact]
    public async Task TrainingQueries_ReturnCurrentAndMapOnlyCompletedHistory()
    {
        ITrainingRepository repository = Substitute.For<ITrainingRepository>();
        Training current = ApplicationTestData.CreateTraining();
        Training completed = ApplicationTestData.CreateTraining(status: EntityStatusTypes.COMPLETED, completedOn: new DateTime(2026, 1, 3));
        Training cancelled = ApplicationTestData.CreateTraining(status: EntityStatusTypes.CANCELLED);
        repository.GetCurrent().Returns(current);
        repository.ListCompletedHistory().Returns([new TrainingHistoryProjection { StartedOn = completed.StartedOn, DayOfTheWeek = completed.DayOfTheWeek }]);

        TrainingDto? result = await new CurrentTrainingQueryHandler(repository, Substitute.For<IExceptionLogger<CurrentTrainingQuery>>())
            .Handle(new CurrentTrainingQuery(Guid.NewGuid()), CancellationToken.None);
        List<TrainingHistoryDto> history = await new HistoryTrainingsQueryHandler(repository, Substitute.For<IExceptionLogger<HistoryTrainingsQuery>>())
            .Handle(new HistoryTrainingsQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(current.Id, result.Id);
        Assert.Equal(current.RoutineId, result.RoutineId);
        TrainingHistoryDto item = Assert.Single(history);
        Assert.Equal(completed.StartedOn.FullDate, item.StartedOn);
        Assert.Equal(completed.DayOfTheWeek, item.DayOfTheWeek);
    }

    [Fact]
    public async Task CurrentTrainingQueryHandler_MapsPreviousExerciseMetricsCorrectly()
    {
        ITrainingRepository repository = Substitute.For<ITrainingRepository>();
        ExerciseParameters parameters = ExerciseParameters.FromDatabase(
            11, "Bench Press", "Chest exercise", "Push", 3, (8, 12), 45, 2, (60, 90),
            85.5m, 45, 10, ExerciseTypes.WEIGHT, [new Observation("Keep elbows tucked")]);
        RoutineItem item = RoutineItem.FromDatabase(1, 3, parameters, false, 1);
        Training training = ApplicationTestData.CreateTraining(pendingWorkout: [item]);
        repository.GetCurrent(Arg.Any<int?>(), Arg.Any<CancellationToken>()).Returns(training);

        CurrentTrainingQueryHandler handler = new(repository, Substitute.For<IExceptionLogger<CurrentTrainingQuery>>());
        TrainingDto? result = await handler.Handle(new CurrentTrainingQuery(Guid.NewGuid(), 42), CancellationToken.None);

        Assert.NotNull(result);
        TrainingRoutineItemDto routineItemDto = Assert.Single(result.PendingWorkout);
        Assert.Equal(85.5m, routineItemDto.LastWeight);
        Assert.Equal(10, routineItemDto.LastRepetitions);
        Assert.Equal(45, routineItemDto.LastDuration);
        Assert.Equal(["Keep elbows tucked"], routineItemDto.LastObservations);
    }
}
