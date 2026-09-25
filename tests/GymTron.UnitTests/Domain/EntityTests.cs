using GymTron.Domain.Aggregates;
using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Domain.Events;
using GymTron.Domain.Exceptions;
using GymTron.UnitTests.Helpers;

namespace GymTron.UnitTests.Domain;

public class EntityTests
{
    private static readonly DateTime CreatedOn = new(2026, 2, 3, 4, 5, 6);

    [Fact]
    public void Factories_WithPersistedAndNewEntities_PreserveTheirContracts()
    {
        FakeClock clock = new(CreatedOn);
        BodyWeight newWeight = BodyWeight.New(81.5m, 17.2m, CreatedOn);
        BodyWeight newWeightWithClock = BodyWeight.New(81.5m, 17.2m, clock);
        BodyWeight storedWeight = BodyWeight.FromDatabase(9, 80m, 16m, CreatedOn);
        Exercise exercise = Exercise.FromDatabase(4, 9, 12, "Squat", 100m, 0, 5, CreatedOn, ["Deep"]);
        Exercise newExerciseWithClock = Exercise.New(9, 12, "Squat", 100m, 0, 5, ["Deep"], clock);
        Exercise newExerciseWithDate = Exercise.New(9, 12, "Squat", 100m, 0, 5, ["Deep"], CreatedOn);
        Exercise newExerciseWithDateBefore = Exercise.New(9, 12, "Squat", 100m, 0, 5, CreatedOn, ["Deep"]);
        Exercise newExerciseDefault = Exercise.New(9, 12, "Squat", 100m, 0, 5, ["Deep"]);
        Log log = Log.New("message");
        Log logWithClock = Log.New("message", clock);
        Log logWithDate = Log.New("message", CreatedOn);

        Assert.Equal(0, newWeight.Id);
        Assert.Equal(81.5m, newWeight.Weight);
        Assert.Equal(17.2m, newWeight.BodyFatPercentage);
        Assert.Equal(CreatedOn, newWeight.Status.CreatedOn);
        Assert.Equal(CreatedOn, newWeightWithClock.Status.CreatedOn);
        Assert.Equal(9, storedWeight.Id);
        Assert.Equal(4, exercise.Id);
        Assert.Equal(9, exercise.TrainingId);
        Assert.Equal(12, exercise.ExerciseParametersId);
        Assert.Equal("Squat", exercise.Name);
        Assert.Equal(100m, exercise.Weight);
        Assert.Equal(0, exercise.DurationInSeconds);
        Assert.Equal(5, exercise.CurrentRepetitions);
        Assert.Equal("Deep", Assert.Single(exercise.Observations).Comment);
        Assert.Equal(CreatedOn, newExerciseWithClock.Status.CreatedOn);
        Assert.Equal(CreatedOn, newExerciseWithDate.Status.CreatedOn);
        Assert.Equal(CreatedOn, newExerciseWithDateBefore.Status.CreatedOn);
        Assert.Equal(default, newExerciseDefault.Status.CreatedOn);
        Assert.Equal("message", log.Message);
        Assert.True(log.Status.IsActive);
        Assert.Equal(CreatedOn, logWithClock.Status.CreatedOn);
        Assert.Equal(CreatedOn, logWithDate.Status.CreatedOn);
    }

    [Fact]
    public void DomainEvents_WhenAdded_AreRecordedAndCanBeCleared()
    {
        Training training = Training.CreateAnStartedTraining(7, 1, [], new FakeClock());
        DomainEvent first = new(Guid.NewGuid(), new FakeClock());
        DomainEvent second = new(Guid.NewGuid(), new FakeClock());

        training.AddDomainEvent(first);
        training.AddDomainEvent(second);

        Assert.Equal([first, second], training.DomainEvents);

        training.ClearDomainEvents();
        Assert.Empty(training.DomainEvents);
    }

    [Fact]
    public void DomainEvent_WhenCreated_RecordsCorrelationAndOccurrence()
    {
        Guid correlationId = Guid.NewGuid();
        FakeClock clock = new(new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc));

        DomainEvent domainEventFromClock = new(correlationId, clock);
        DomainEvent domainEventFromDate = new(correlationId, clock.UtcNow);

        Assert.Equal(correlationId, domainEventFromClock.CorrelationId);
        Assert.Equal(clock.UtcNow, domainEventFromClock.OccurredOn);
        Assert.Equal(clock.UtcNow, domainEventFromDate.OccurredOn);
    }

    [Fact]
    public void Exceptions_WhenCreated_PreserveCurrentMessages()
    {
        DomainException domain = new("domain failure");
        EntityNotFoundException missing = new("Routine");

        Assert.Equal("domain failure", domain.Message);
        Assert.Equal("Entity not found: Routine", missing.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Routine_Create_WithInvalidName_ThrowsInvalidDomainOperationException(string? invalidName)
    {
        Assert.Throws<InvalidDomainOperationException>(() => Routine.Create(invalidName!, []));
    }

    [Fact]
    public void Routine_CreateAndRename_PreservesStateAndExercises()
    {
        ExerciseParameters param = ExerciseParameters.Create("Squat", "Legs", "P", ExerciseTypes.WEIGHT, 2);
        RoutineItem item = RoutineItem.Create(1, param, false, 1);
        Routine routine = Routine.Create(" Legs Day ", [item]);

        Assert.Equal("Legs Day", routine.Name);
        Assert.Single(routine.RoutineExercises);
        Assert.Single(routine.WorkByDays[1]);

        routine.Rename(" Heavy Legs ");
        Assert.Equal("Heavy Legs", routine.Name);

        Assert.Throws<InvalidDomainOperationException>(() => routine.Rename(""));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    public void RoutineItem_Create_WithInvalidDayOfWeek_ThrowsInvalidDomainOperationException(int dayOfWeek)
    {
        ExerciseParameters param = ExerciseParameters.Create("Squat", "Legs", "P", ExerciseTypes.WEIGHT, 2);
        Assert.Throws<InvalidDomainOperationException>(() => RoutineItem.Create(dayOfWeek, param, false, 1));
    }

    [Fact]
    public void RoutineItem_Create_WithNullParameters_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => RoutineItem.Create(1, null!, false, 1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ExerciseParameters_CreateAndUpdate_WithInvalidName_ThrowsInvalidDomainOperationException(string? invalidName)
    {
        Assert.Throws<InvalidDomainOperationException>(() => ExerciseParameters.Create(invalidName!, "D", "P", ExerciseTypes.WEIGHT));

        ExerciseParameters valid = ExerciseParameters.Create("Valid", "D", "P", ExerciseTypes.WEIGHT);
        Assert.Throws<InvalidDomainOperationException>(() => valid.Update(invalidName!, "D", "P", ExerciseTypes.WEIGHT, null));
    }

    [Fact]
    public void ExerciseParameters_CreateAndUpdate_PreservesProperties()
    {
        ExerciseParameters param = ExerciseParameters.Create("Bench", "Chest", "P", ExerciseTypes.WEIGHT, 1);
        Assert.Equal("Bench", param.Name);
        Assert.Equal("Chest", param.Description);
        Assert.Equal("P", param.Pattern);
        Assert.Equal(ExerciseTypes.WEIGHT, param.Type);
        Assert.Equal(1, param.ReplaysInReserve);
        Assert.Empty(param.Observations);

        param.Update(" Incline Bench ", "Upper Chest", "P2", ExerciseTypes.WEIGHT, 2);
        Assert.Equal("Incline Bench", param.Name);
        Assert.Equal("Upper Chest", param.Description);
        Assert.Equal("P2", param.Pattern);
        Assert.Equal(2, param.ReplaysInReserve);
    }

    [Fact]
    public void ExerciseParameters_DurationVsWeight_NormalizesRepetitionsAndDuration()
    {
        ExerciseParameters durationFromDb = ExerciseParameters.FromDatabase(
            1, "Plank", "Core", "Isometric", 3, (8, 12), 45, null, (30, 60), null, 45, null, ExerciseTypes.DURATION, []);
        Assert.Equal((0, 0), durationFromDb.Repetitions);
        Assert.Equal(45, durationFromDb.DurationInSeconds);

        ExerciseParameters weightFromDb = ExerciseParameters.FromDatabase(
            2, "Bench", "Chest", "Push", 3, (8, 12), 45, null, (60, 90), 80m, null, 10, ExerciseTypes.WEIGHT, []);
        Assert.Equal((8, 12), weightFromDb.Repetitions);
        Assert.Equal(0, weightFromDb.DurationInSeconds);

        weightFromDb.Update("Plank", "Core", "Isometric", ExerciseTypes.DURATION, null);
        Assert.Equal((0, 0), weightFromDb.Repetitions);

        durationFromDb.Update("Bench", "Chest", "Push", ExerciseTypes.WEIGHT, null);
        Assert.Equal(0, durationFromDb.DurationInSeconds);
    }

    [Fact]
    public void User_NewWithDateAndFromDatabaseInactive_And_UpdatePassword_WorkCorrectly()
    {
        FakeClock clock = new(CreatedOn);
        User userWithDate = User.New("username", "email@test.com", "hash", CreatedOn);
        Assert.Equal(0, userWithDate.Id);
        Assert.Equal("username", userWithDate.Username);
        Assert.Equal("email@test.com", userWithDate.Email);
        Assert.Equal("hash", userWithDate.PasswordHash);
        Assert.True(userWithDate.Status.IsActive);
        Assert.Equal(CreatedOn, userWithDate.Status.CreatedOn);

        User inactiveUser = User.FromDatabase(10, "inactive", "inactive@test.com", "hash2", false, CreatedOn);
        Assert.Equal(10, inactiveUser.Id);
        Assert.False(inactiveUser.Status.IsActive);

        FakeClock updateClock = new(CreatedOn.AddHours(1));
        userWithDate.UpdatePassword("newHash", updateClock);
        Assert.Equal("newHash", userWithDate.PasswordHash);
        Assert.Equal(CreatedOn.AddHours(1), userWithDate.Status.ModifiedOn);
    }

    [Fact]
    public void RefreshToken_New_FromDatabase_Revoke_And_IsActive_WorkCorrectly()
    {
        FakeClock clock = new(CreatedOn);
        DateTime expiresAt = CreatedOn.AddDays(7);
        RefreshToken token = RefreshToken.New(1, "hash123", expiresAt, clock);

        Assert.Equal(0, token.Id);
        Assert.Equal(1, token.UserId);
        Assert.Equal("hash123", token.TokenHash);
        Assert.Equal(expiresAt, token.ExpiresAt);
        Assert.Null(token.RevokedAt);
        Assert.Null(token.ReplacedByTokenHash);
        Assert.False(token.IsRevoked);
        Assert.False(token.IsExpired(CreatedOn));
        Assert.True(token.IsActive(CreatedOn));
        Assert.False(token.IsActive(expiresAt.AddSeconds(1)));

        token.Revoke(CreatedOn.AddDays(1), "replacedHash");
        Assert.True(token.IsRevoked);
        Assert.Equal("replacedHash", token.ReplacedByTokenHash);
        Assert.False(token.IsActive(CreatedOn));
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0, 0, 10)]
    [InlineData(10, 0, 0)]
    [InlineData(-5, 0, 10)]
    [InlineData(10, 0, -2)]
    [InlineData(0, -1, 0)]
    public void Exercise_New_WithInvalidValues_ThrowsInvalidDomainOperationException(decimal weight, int duration, int repetitions)
    {
        Assert.Throws<InvalidDomainOperationException>(() =>
            Exercise.New(1, 1, "Bench Press", weight, duration, repetitions, []));
    }

    [Theory]
    [InlineData(0, 45, 0)]
    [InlineData(80, 0, 10)]
    public void Exercise_New_WithValidValues_Succeeds(decimal weight, int duration, int repetitions)
    {
        Exercise exercise = Exercise.New(1, 1, "Bench Press", weight, duration, repetitions, []);
        Assert.NotNull(exercise);
        Assert.Equal(weight, exercise.Weight);
        Assert.Equal(duration, exercise.DurationInSeconds);
        Assert.Equal(repetitions, exercise.CurrentRepetitions);
    }
}
