using GymTron.Application.Common.Events;
using GymTron.Application.Trainings.Commands;
using GymTron.Application.Trainings.Commands.Handlers;
using GymTron.Domain.Aggregates;
using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Domain.Exceptions;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;
using GymTron.UnitTests.Helpers;
using NSubstitute;

namespace GymTron.UnitTests.Application;

public class StartTrainingCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenTrainingIsAlreadyActive_RejectsWithoutAddingAnotherTraining()
    {
        IDomainEventDispatcher eventDispatcher = Substitute.For<IDomainEventDispatcher>();
        ITrainingRepository trainingRepository = Substitute.For<ITrainingRepository>();
        IRoutineRepository routineRepository = Substitute.For<IRoutineRepository>();
        IExceptionLogger<StartTrainingCommand> logger = Substitute.For<IExceptionLogger<StartTrainingCommand>>();
        trainingRepository.GetCurrent().Returns(CreateActiveTraining());
        StartTrainingCommandHandler handler = new(eventDispatcher, trainingRepository, routineRepository, logger, new FakeClock());
        StartTrainingCommand command = new(Guid.NewGuid(), 7, 3);

        InvalidDomainOperationException exception = await Assert.ThrowsAsync<InvalidDomainOperationException>(() => handler.Handle(command, CancellationToken.None));

        Assert.Equal("The previous training is not ended.", exception.Message);
        await trainingRepository.DidNotReceive().Add(Arg.Any<Training>());
        await routineRepository.DidNotReceive().GetById(Arg.Any<int>());
        await logger.Received(1).LogException(exception);
    }

    [Fact]
    public async Task Handle_WithAvailableRoutineDay_AddsActiveTrainingWithExpectedWorkout()
    {
        IDomainEventDispatcher eventDispatcher = Substitute.For<IDomainEventDispatcher>();
        ITrainingRepository trainingRepository = Substitute.For<ITrainingRepository>();
        IRoutineRepository routineRepository = Substitute.For<IRoutineRepository>();
        IExceptionLogger<StartTrainingCommand> logger = Substitute.For<IExceptionLogger<StartTrainingCommand>>();
        trainingRepository.GetCurrent().Returns((Training?)null);
        List<RoutineItem> selectedWorkout = [CreateRoutineItem(11, 3), CreateRoutineItem(22, 3)];
        Routine routine = Routine.FromDatabase(7, "Routine", [.. selectedWorkout, CreateRoutineItem(33, 4)]);
        routineRepository.GetById(7).Returns(routine);
        StartTrainingCommandHandler handler = new(eventDispatcher, trainingRepository, routineRepository, logger, new FakeClock());
        StartTrainingCommand command = new(Guid.NewGuid(), 7, 3);

        await handler.Handle(command, CancellationToken.None);

        await trainingRepository.Received(1).Add(Arg.Is<Training>(training =>
            training.RoutineId == 7
            && training.DayOfTheWeek == 3
            && training.Status.IsActive
            && training.PendingWorkout.SequenceEqual(selectedWorkout)));
        await logger.DidNotReceive().LogException(Arg.Any<Exception>());
    }

    [Fact]
    public async Task Handle_ForwardsCancellationTokenToRepositories()
    {
        IDomainEventDispatcher eventDispatcher = Substitute.For<IDomainEventDispatcher>();
        ITrainingRepository trainingRepository = Substitute.For<ITrainingRepository>();
        IRoutineRepository routineRepository = Substitute.For<IRoutineRepository>();
        IExceptionLogger<StartTrainingCommand> logger = Substitute.For<IExceptionLogger<StartTrainingCommand>>();
        trainingRepository.GetCurrent(null, Arg.Any<CancellationToken>()).Returns((Training?)null);
        Routine routine = Routine.FromDatabase(7, "Routine", [CreateRoutineItem(11, 3)]);
        routineRepository.GetById(7, Arg.Any<CancellationToken>()).Returns(routine);
        StartTrainingCommandHandler handler = new(eventDispatcher, trainingRepository, routineRepository, logger, new FakeClock());
        StartTrainingCommand command = new(Guid.NewGuid(), 7, 3);
        using CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;

        await handler.Handle(command, token);

        await trainingRepository.Received(1).GetCurrent(null, token);
        await routineRepository.Received(1).GetById(7, token);
        await trainingRepository.Received(1).Add(Arg.Any<Training>(), token);
    }

    [Fact]
    public async Task Handle_WhenUserAttemptsToStartAnotherUsersRoutine_ThrowsEntityNotFoundException()
    {
        IDomainEventDispatcher eventDispatcher = Substitute.For<IDomainEventDispatcher>();
        ITrainingRepository trainingRepository = Substitute.For<ITrainingRepository>();
        IRoutineRepository routineRepository = Substitute.For<IRoutineRepository>();
        IExceptionLogger<StartTrainingCommand> logger = Substitute.For<IExceptionLogger<StartTrainingCommand>>();
        trainingRepository.GetCurrent(1, Arg.Any<CancellationToken>()).Returns((Training?)null);
        Routine routine = Routine.FromDatabase(7, "Routine", [CreateRoutineItem(11, 3)], userId: 2);
        routineRepository.GetById(7, Arg.Any<CancellationToken>()).Returns(routine);
        StartTrainingCommandHandler handler = new(eventDispatcher, trainingRepository, routineRepository, logger, new FakeClock());
        StartTrainingCommand command = new(Guid.NewGuid(), 7, 3, userId: 1);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.Handle(command, CancellationToken.None));
        await trainingRepository.DidNotReceive().Add(Arg.Any<Training>());
    }

    [Fact]
    public async Task Handle_WithUser_AddsActiveTrainingWithUserId()
    {
        IDomainEventDispatcher eventDispatcher = Substitute.For<IDomainEventDispatcher>();
        ITrainingRepository trainingRepository = Substitute.For<ITrainingRepository>();
        IRoutineRepository routineRepository = Substitute.For<IRoutineRepository>();
        IExceptionLogger<StartTrainingCommand> logger = Substitute.For<IExceptionLogger<StartTrainingCommand>>();
        trainingRepository.GetCurrent(1, Arg.Any<CancellationToken>()).Returns((Training?)null);
        Routine routine = Routine.FromDatabase(7, "Routine", [CreateRoutineItem(11, 3)], userId: 1);
        routineRepository.GetById(7, Arg.Any<CancellationToken>()).Returns(routine);
        StartTrainingCommandHandler handler = new(eventDispatcher, trainingRepository, routineRepository, logger, new FakeClock());
        StartTrainingCommand command = new(Guid.NewGuid(), 7, 3, userId: 1);

        await handler.Handle(command, CancellationToken.None);

        await trainingRepository.Received(1).Add(Arg.Is<Training>(t => t.UserId == 1 && t.RoutineId == 7));
    }

    private static Training CreateActiveTraining()
        => Training.CreateAnStartedTraining(7, 3, [CreateRoutineItem(11, 3)], new FakeClock());

    private static RoutineItem CreateRoutineItem(int exerciseParametersId, int dayOfWeek)
        => RoutineItem.FromDatabase(
            exerciseParametersId,
            dayOfWeek,
            ExerciseParameters.FromDatabase(
                exerciseParametersId,
                $"Exercise {exerciseParametersId}",
                "Description",
                "Pattern",
                3,
                (8, 12),
                0,
                null,
                (60, 90),
                null,
                null,
                null,
                ExerciseTypes.WEIGHT,
                []),
            false,
            1);
}
