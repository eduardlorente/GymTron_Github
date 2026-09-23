using GymTron.Application.Backup.DTOs;
using GymTron.Application.Backup.Queries;
using GymTron.Application.Backup.Queries.Handlers;
using GymTron.Domain.Projections;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;
using GymTron.UnitTests.Helpers;
using NSubstitute;

namespace GymTron.UnitTests.Application.Backup;

public class ExportUserDataQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenExecuted_ReturnsCompleteUserDataBackupScopedToUser()
    {
        // Arrange
        const int userId = 42;
        var correlationId = Guid.NewGuid();
        var fixedTime = new DateTime(2026, 9, 23, 11, 0, 0, DateTimeKind.Utc);
        var clock = new FakeClock(fixedTime);

        var routineRepo = Substitute.For<IRoutineRepository>();
        var trainingRepo = Substitute.For<ITrainingRepository>();
        var exerciseRepo = Substitute.For<IExerciseRepository>();
        var bodyWeightRepo = Substitute.For<IBodyWeightRepository>();
        var logger = Substitute.For<IExceptionLogger<ExportUserDataQuery>>();

        var expectedRoutines = new List<RoutineProjection>
        {
            new() { Id = 1, Name = "Leg Day", UserId = userId }
        };
        var expectedTrainings = new List<TrainingHistoryProjection>
        {
            new() { DayOfTheWeek = 1 }
        };
        var expectedExercises = new List<ExerciseHistoryProjection>
        {
            new() { Name = "Barbell Squat", Weight = 100m, Repetitions = 5 }
        };
        var expectedBodyWeights = new List<BodyWeightHistoryProjection>
        {
            new() { Weight = 80.5m, BodyFatPercentage = 15m, CreatedOn = fixedTime }
        };

        routineRepo.ListRoutineProjections(userId, Arg.Any<CancellationToken>()).Returns(expectedRoutines);
        trainingRepo.ListCompletedHistory(userId, Arg.Any<CancellationToken>()).Returns(expectedTrainings);
        exerciseRepo.ListHistory(userId, Arg.Any<CancellationToken>()).Returns(expectedExercises);
        bodyWeightRepo.ListHistory(userId, Arg.Any<CancellationToken>()).Returns(expectedBodyWeights);

        var handler = new ExportUserDataQueryHandler(
            routineRepo,
            trainingRepo,
            exerciseRepo,
            bodyWeightRepo,
            clock,
            logger);

        var query = new ExportUserDataQuery(correlationId, userId);

        // Act
        UserDataBackupDto result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Version);
        Assert.Equal(fixedTime, result.ExportedAtUtc);
        Assert.Equal(userId, result.UserId);
        Assert.Single(result.Routines);
        Assert.Equal("Leg Day", result.Routines[0].Name);
        Assert.Single(result.CompletedTrainings);
        Assert.Single(result.ExerciseHistory);
        Assert.Equal("Barbell Squat", result.ExerciseHistory[0].Name);
        Assert.Single(result.BodyWeights);
        Assert.Equal(80.5m, result.BodyWeights[0].Weight);

        await routineRepo.Received(1).ListRoutineProjections(userId, Arg.Any<CancellationToken>());
        await trainingRepo.Received(1).ListCompletedHistory(userId, Arg.Any<CancellationToken>());
        await exerciseRepo.Received(1).ListHistory(userId, Arg.Any<CancellationToken>());
        await bodyWeightRepo.Received(1).ListHistory(userId, Arg.Any<CancellationToken>());
    }
}
