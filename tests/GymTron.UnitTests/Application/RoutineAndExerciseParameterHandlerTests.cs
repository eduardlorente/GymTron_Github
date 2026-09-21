using GymTron.Application.ExerciseParameters.Commands;
using GymTron.Application.ExerciseParameters.Commands.Handlers;
using GymTron.Application.ExerciseParameters.Queries;
using GymTron.Application.ExerciseParameters.Queries.DTO;
using GymTron.Application.ExerciseParameters.Queries.Handlers;
using GymTron.Application.Routines.Commands;
using GymTron.Application.Routines.Commands.Handlers;
using GymTron.Application.Routines.Queries;
using GymTron.Application.Routines.Queries.DTO;
using GymTron.Application.Routines.Queries.Handlers;
using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Domain.Exceptions;
using GymTron.Domain.Projections;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;
using NSubstitute;

namespace GymTron.UnitTests.Application;

public class RoutineAndExerciseParameterHandlerTests
{
    [Fact]
    public async Task RoutineCommands_MapEveryItemFieldToRepositoryContracts()
    {
        IRoutineRepository repository = Substitute.For<IRoutineRepository>();
        repository.Create(Arg.Any<Routine>()).Returns(42);
        RoutineItemInput item = CreateInput();

        int id = await new CreateRoutineCommandHandler(repository, Substitute.For<IExceptionLogger<CreateRoutineCommand>>())
            .Handle(new CreateRoutineCommand(Guid.NewGuid(), "Strength", [item]), CancellationToken.None);
        await new UpdateRoutineCommandHandler(repository, Substitute.For<IExceptionLogger<UpdateRoutineCommand>>())
            .Handle(new UpdateRoutineCommand(Guid.NewGuid(), 42, "Strength 2", [item]), CancellationToken.None);

        Assert.Equal(42, id);
        await repository.Received(1).Create(Arg.Is<Routine>(r => r.Name == "Strength" && Matches(r.RoutineExercises.Single(), item)));
        await repository.Received(1).Update(Arg.Is<Routine>(r => r.Id == 42 && r.Name == "Strength 2" && Matches(r.RoutineExercises.Single(), item)));
    }

    [Fact]
    public async Task RoutineQueries_MapFoundRoutineAndReturnEmptyShapesForMissingData()
    {
        IRoutineRepository repository = Substitute.For<IRoutineRepository>();
        RoutineProjection routine = new()
        {
            Id = 7,
            Name = "Strength",
            Items =
            [
                new RoutineItemProjection
                {
                    Id = 1,
                    DayOfWeek = 3,
                    ExerciseParametersId = 11,
                    ExerciseName = "Exercise 11",
                    Series = 3,
                    RepetitionsMin = 8,
                    RepetitionsMax = 12,
                    Duration = 0,
                    MinRestTimeInSeconds = 60,
                    MaxRestTimeInSeconds = 90,
                    AlternatingSeries = true,
                    Position = 2,
                    Type = ExerciseTypes.WEIGHT
                }
            ]
        };
        repository.GetRoutineProjection(7).Returns(routine);
        repository.GetRoutineProjection(8).Returns((RoutineProjection?)null);
        repository.ListRoutineProjections().Returns(_ => Task.FromResult(new List<RoutineProjection> { routine }), _ => Task.FromResult(new List<RoutineProjection>()));
        IExceptionLogger<GetRoutineByIdQuery> getLogger = Substitute.For<IExceptionLogger<GetRoutineByIdQuery>>();
        GetRoutineByIdQueryHandler getHandler = new(repository, getLogger);

        RoutineDto? mapped = await getHandler.Handle(new GetRoutineByIdQuery(Guid.NewGuid(), 7), CancellationToken.None);
        RoutineDto? missing = await getHandler.Handle(new GetRoutineByIdQuery(Guid.NewGuid(), 8), CancellationToken.None);
        ListAllQueryHandler listHandler = new(repository, Substitute.For<IExceptionLogger<ListAllQuery>>());
        List<RoutineDto> populated = await listHandler.Handle(new ListAllQuery(Guid.NewGuid()), CancellationToken.None);
        List<RoutineDto> empty = await listHandler.Handle(new ListAllQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal((7, "Strength"), (mapped!.Id, mapped.Name));
        RoutineItemDto item = Assert.Single(mapped.Items);
        Assert.Equal((11, 3, "Exercise 11", 3, 8, 12, 0, 60, 90, true, 2, ExerciseTypes.WEIGHT),
            (item.ExerciseParametersId, item.DayOfWeek, item.ExerciseName, item.Series, item.RepetitionsMin,
             item.RepetitionsMax, item.Duration, item.MinRestTimeInSeconds, item.MaxRestTimeInSeconds,
             item.AlternatingSeries, item.Position, item.Type));
        Assert.Null(missing);
        Assert.Equal(routine.Id, Assert.Single(populated).Id);
        Assert.Empty(empty);
    }

    [Fact]
    public async Task ExerciseParameterCommands_DelegateValuesAndReturnCreatedId()
    {
        IExerciseParameterRepository repository = Substitute.For<IExerciseParameterRepository>();
        repository.Create(Arg.Any<ExerciseParameters>()).Returns(9);

        int id = await new CreateExerciseParameterCommandHandler(repository, Substitute.For<IExceptionLogger<CreateExerciseParameterCommand>>())
            .Handle(new CreateExerciseParameterCommand(Guid.NewGuid(), "Squat", "Desc", "Pattern", ExerciseTypes.WEIGHT, 2), CancellationToken.None);
        await new UpdateExerciseParameterCommandHandler(repository, Substitute.For<IExceptionLogger<UpdateExerciseParameterCommand>>())
            .Handle(new UpdateExerciseParameterCommand(Guid.NewGuid(), 9, "Plank", "Desc 2", "Pattern 2", ExerciseTypes.DURATION, null), CancellationToken.None);

        Assert.Equal(9, id);
        await repository.Received(1).Create(Arg.Is<ExerciseParameters>(p =>
            p.Name == "Squat" && p.Description == "Desc" && p.Pattern == "Pattern" && p.Type == ExerciseTypes.WEIGHT && p.ReplaysInReserve == 2));
        await repository.Received(1).Update(Arg.Is<ExerciseParameters>(p =>
            p.Id == 9 && p.Name == "Plank" && p.Description == "Desc 2" && p.Pattern == "Pattern 2" && p.Type == ExerciseTypes.DURATION && p.ReplaysInReserve == null));
    }

    [Fact]
    public async Task ExerciseParameterQueries_MapListsAndReturnNullForMissingEntity()
    {
        IExerciseParameterRepository repository = Substitute.For<IExerciseParameterRepository>();
        ExerciseParameterProjection projection = new()
        {
            Id = 11,
            Name = "Exercise 11",
            Description = "Description",
            Pattern = "Pattern",
            Type = ExerciseTypes.WEIGHT,
            ReplaysInReserve = 2
        };
        repository.ListProjections().Returns([projection]);
        repository.GetProjection(11).Returns(projection);
        repository.GetProjection(12).Returns((ExerciseParameterProjection?)null);

        List<ExerciseParameterDto> list = await new ListExerciseParametersQueryHandler(repository, Substitute.For<IExceptionLogger<ListExerciseParametersQuery>>())
            .Handle(new ListExerciseParametersQuery(Guid.NewGuid()), CancellationToken.None);
        GetExerciseParameterByIdQueryHandler getHandler = new(repository, Substitute.For<IExceptionLogger<GetExerciseParameterByIdQuery>>());
        ExerciseParameterDto? found = await getHandler.Handle(new GetExerciseParameterByIdQuery(Guid.NewGuid(), 11), CancellationToken.None);
        ExerciseParameterDto? missing = await getHandler.Handle(new GetExerciseParameterByIdQuery(Guid.NewGuid(), 12), CancellationToken.None);

        ExerciseParameterDto listed = Assert.Single(list);
        Assert.Equal((11, "Exercise 11", "Description", "Pattern", ExerciseTypes.WEIGHT, 2),
            (listed.Id, listed.Name, listed.Description, listed.Pattern, listed.Type, listed.ReplaysInReserve));
        Assert.Equal(listed.Id, found!.Id);
        Assert.Null(missing);
    }

    private static RoutineItemInput CreateInput() => new()
    {
        DayOfWeek = 3,
        ExerciseParametersId = 11,
        Series = 4,
        RepetitionsMin = 6,
        RepetitionsMax = 10,
        Duration = null,
        MinRestTimeInSeconds = 60,
        MaxRestTimeInSeconds = 90,
        AlternatingSeries = true,
        Position = 2,
        Type = ExerciseTypes.WEIGHT
    };

    private static bool Matches(RoutineItem actual, RoutineItemInput expected)
        => actual.DayOfWeek == expected.DayOfWeek &&
           actual.ExerciseParameters.Id == expected.ExerciseParametersId &&
           actual.ExerciseParameters.Series == expected.Series &&
           actual.ExerciseParameters.Repetitions.Min == expected.RepetitionsMin &&
           actual.ExerciseParameters.Repetitions.Max == expected.RepetitionsMax &&
           actual.ExerciseParameters.DurationInSeconds == (expected.Duration ?? 0) &&
           actual.ExerciseParameters.RestTimeInSeconds.Min == expected.MinRestTimeInSeconds &&
           actual.ExerciseParameters.RestTimeInSeconds.Max == (expected.MaxRestTimeInSeconds ?? 0) &&
           actual.AlternatingSeries == expected.AlternatingSeries &&
           actual.Position == expected.Position &&
           actual.ExerciseParameters.Type == expected.Type;

    [Fact]
    public async Task UpdateRoutine_WhenUserDoesNotOwnRoutine_ThrowsEntityNotFoundException()
    {
        IRoutineRepository repository = Substitute.For<IRoutineRepository>();
        Routine existing = Routine.FromDatabase(42, "Existing", [], userId: 2);
        repository.GetById(42).Returns(existing);
        RoutineItemInput item = CreateInput();
        UpdateRoutineCommandHandler handler = new(repository, Substitute.For<IExceptionLogger<UpdateRoutineCommand>>());
        UpdateRoutineCommand command = new(Guid.NewGuid(), 42, "Updated", [item], userId: 1);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.Handle(command, CancellationToken.None));
        await repository.DidNotReceive().Update(Arg.Any<Routine>());
    }

    [Fact]
    public async Task UpdateRoutine_WhenUserOwnsRoutine_UpdatesSuccessfully()
    {
        IRoutineRepository repository = Substitute.For<IRoutineRepository>();
        Routine existing = Routine.FromDatabase(42, "Existing", [], userId: 1);
        repository.GetById(42).Returns(existing);
        RoutineItemInput item = CreateInput();
        UpdateRoutineCommandHandler handler = new(repository, Substitute.For<IExceptionLogger<UpdateRoutineCommand>>());
        UpdateRoutineCommand command = new(Guid.NewGuid(), 42, "Updated", [item], userId: 1);

        await handler.Handle(command, CancellationToken.None);

        await repository.Received(1).Update(Arg.Is<Routine>(r => r.Id == 42 && r.UserId == 1));
    }

    [Fact]
    public async Task GetRoutineById_WhenUserDoesNotOwnRoutine_ReturnsNull()
    {
        IRoutineRepository repository = Substitute.For<IRoutineRepository>();
        repository.GetRoutineProjection(7).Returns(new RoutineProjection { Id = 7, UserId = 2, Name = "Other" });
        GetRoutineByIdQueryHandler handler = new(repository, Substitute.For<IExceptionLogger<GetRoutineByIdQuery>>());
        GetRoutineByIdQuery query = new(Guid.NewGuid(), 7, userId: 1);

        RoutineDto? result = await handler.Handle(query, CancellationToken.None);

        Assert.Null(result);
    }
}
