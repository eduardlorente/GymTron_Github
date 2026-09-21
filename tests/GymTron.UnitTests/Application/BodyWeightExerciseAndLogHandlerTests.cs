using FluentValidation;
using GymTron.Application.BodyWeights.Commands;
using GymTron.Application.BodyWeights.Commands.Handlers;
using GymTron.Application.BodyWeights.Queries;
using GymTron.Application.BodyWeights.Queries.Handlers;
using GymTron.Application.Exercises.Queries;
using GymTron.Application.Exercises.Queries.Handlers;
using GymTron.Application.Logs.Commands;
using GymTron.Application.Logs.Commands.Handlers;
using GymTron.Domain.Entities;
using GymTron.Domain.Projections;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;
using GymTron.UnitTests.Helpers;
using NSubstitute;

namespace GymTron.UnitTests.Application;

public class BodyWeightExerciseAndLogHandlerTests
{
    [Fact]
    public async Task RegisterBodyWeight_WhenValid_AddsMeasurementWithBoundedTimestamp()
    {
        IBodyWeightRepository repository = Substitute.For<IBodyWeightRepository>();
        FakeClock clock = new(new DateTime(2026, 6, 15, 10, 0, 0, DateTimeKind.Utc));

        await new RegisterBodyWeightCommandHandler(repository, Substitute.For<IExceptionLogger<RegisterBodyWeightCommand>>(), clock)
            .Handle(new RegisterBodyWeightCommand(Guid.NewGuid(), 81.5m, 17.2m), CancellationToken.None);

        await repository.Received(1).Add(Arg.Is<BodyWeight>(item =>
            item.Weight == 81.5m && item.BodyFatPercentage == 17.2m
            && item.Status.CreatedOn == clock.UtcNow));
    }



    [Fact]
    public async Task BodyWeightHistory_MapsMeasurementsNewestFirst()
    {
        IBodyWeightRepository repository = Substitute.For<IBodyWeightRepository>();
        repository.ListHistory().Returns([
            new BodyWeightHistoryProjection { Weight = 79m, BodyFatPercentage = 15m, CreatedOn = new DateTime(2026, 2, 1) },
            new BodyWeightHistoryProjection { Weight = 80m, BodyFatPercentage = 16m, CreatedOn = new DateTime(2026, 1, 1) }]);

        List<BodyWeightHistoryProjection> result = await new HistoryBodyWeightQueryHandler(repository, Substitute.For<IExceptionLogger<HistoryBodyWeightQuery>>())
            .Handle(new HistoryBodyWeightQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal([79m, 80m], result.Select(item => item.Weight));
        Assert.Equal([15m, 16m], result.Select(item => item.BodyFatPercentage));
    }

    [Fact]
    public async Task ExerciseHistory_MapsAndOrdersExecutionsByName()
    {
        IExerciseRepository repository = Substitute.For<IExerciseRepository>();
        repository.ListHistory().Returns([
            new ExerciseHistoryProjection { Name = "Plank", DurationInSeconds = 60, Weight = 0, Repetitions = 0, CreatedOn = new DateTime(2026, 1, 1) },
            new ExerciseHistoryProjection { Name = "Squat", DurationInSeconds = 0, Weight = 90, Repetitions = 8, CreatedOn = new DateTime(2026, 2, 1) }]);

        List<ExerciseHistoryProjection> result = await new ExerciseHistoryQueryHandler(repository, Substitute.For<IExceptionLogger<ExerciseHistoryQuery>>())
            .Handle(new ExerciseHistoryQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Equal(["Plank", "Squat"], result.Select(item => item.Name));
        Assert.Equal((60, 0m, 0), (result[0].DurationInSeconds, result[0].Weight, result[0].Repetitions));
        Assert.Equal(new DateTime(2026, 2, 1), result[1].CreatedOn);
    }

    [Fact]
    public async Task RegisterLog_WhenValid_AddsLog()
    {
        ILogRepository repository = Substitute.For<ILogRepository>();
        FakeClock clock = new(new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc));
        RegisterLogCommandHandler handler = new(repository, clock);

        await handler.Handle(new RegisterLogCommand(Guid.NewGuid(), "failure details"), CancellationToken.None);
        await repository.Received(1).Add(Arg.Is<Log>(log => log.Message == "failure details" && log.Status.CreatedOn == clock.UtcNow));
    }
}
