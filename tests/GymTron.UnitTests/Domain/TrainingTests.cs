using GymTron.Domain.Aggregates;
using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Domain.Exceptions;
using GymTron.Domain.ValueObjects;
using GymTron.UnitTests.Helpers;

namespace GymTron.UnitTests.Domain;

public class TrainingTests
{
    private static readonly FakeClock Clock = new();

    [Fact]
    public void Start_WithRoutineDayAndWorkout_CreatesActiveTraining()
    {
        List<RoutineItem> workout = [CreateRoutineItem(11), CreateRoutineItem(22)];

        Training training = Training.CreateAnStartedTraining(7, 3, workout, Clock);

        Assert.Equal(7, training.RoutineId);
        Assert.Equal(3, training.DayOfTheWeek);
        Assert.True(training.Status.IsActive);
        Assert.Same(workout, training.PendingWorkout);
        Assert.Empty(training.CompletedWorkout);
    }

    [Fact]
    public void CompleteExercise_WithMatchingPendingItem_MovesExerciseOnlyOnce()
    {
        Training training = Training.CreateAnStartedTraining(
            7,
            3,
            [CreateRoutineItem(11), CreateRoutineItem(22)],
            Clock);
        Exercise exercise = CreateExercise(11);

        training.CompleteExercise(exercise, Clock);
        training.CompleteExercise(exercise, Clock);

        Exercise completedExercise = Assert.Single(training.CompletedWorkout);
        Assert.Same(exercise, completedExercise);
        RoutineItem pendingItem = Assert.Single(training.PendingWorkout);
        Assert.Equal(22, pendingItem.ExerciseParameters.Id);
    }

    [Fact]
    public void CompleteExercise_WithInvalidExecutionValues_ThrowsInvalidDomainOperationException()
    {
        Training training = CreateActiveTraining();
        Exercise invalidExercise = Exercise.FromDatabase(1, training.Id, 11, "Squat", 0, 0, 0, DateTime.UtcNow, []);

        Assert.Throws<InvalidDomainOperationException>(() => training.CompleteExercise(invalidExercise, Clock));
    }

    [Fact]
    public void Complete_WhenActive_MarksTrainingCompletedAndRecordsCompletionTime()
    {
        Training training = CreateActiveTraining();

        training.Complete(Clock);

        Assert.True(training.Status.IsCompleted);
        Assert.NotNull(training.CompletedOn);
        Assert.Equal(Clock.UtcNow, training.CompletedOn.FullDate);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_IsRejected()
    {
        Training training = CreateActiveTraining();
        training.Complete(Clock);

        InvalidDomainOperationException exception = Assert.Throws<InvalidDomainOperationException>(() => training.Complete(Clock));

        Assert.Equal("Training already completed.", exception.Message);
    }

    [Fact]
    public void Complete_WhenCancelled_IsRejected()
    {
        Training training = CreateActiveTraining();
        training.Cancel(Clock);

        InvalidDomainOperationException exception = Assert.Throws<InvalidDomainOperationException>(() => training.Complete(Clock));

        Assert.Equal("Training already completed.", exception.Message);
    }

    [Fact]
    public void Cancel_WhenActive_MarksTrainingCancelled()
    {
        Training training = CreateActiveTraining();

        training.Cancel(Clock);

        Assert.Equal(EntityStatusTypes.CANCELLED, training.Status.Status);
        Assert.False(training.Status.IsActive);
        Assert.Equal(Clock.UtcNow, training.Status.ModifiedOn);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_IsRejected()
    {
        Training training = CreateActiveTraining();
        training.Cancel(Clock);

        InvalidDomainOperationException exception = Assert.Throws<InvalidDomainOperationException>(() => training.Cancel(Clock));

        Assert.Equal("Training already completed.", exception.Message);
    }

    [Fact]
    public void Cancel_WhenCompleted_IsRejected()
    {
        Training training = CreateActiveTraining();
        training.Complete(Clock);

        InvalidDomainOperationException exception = Assert.Throws<InvalidDomainOperationException>(() => training.Cancel(Clock));

        Assert.Equal("Training already completed.", exception.Message);
    }

    private static Training CreateActiveTraining()
        => Training.CreateAnStartedTraining(7, 3, [CreateRoutineItem(11)], Clock);

    private static RoutineItem CreateRoutineItem(int exerciseParametersId)
        => RoutineItem.FromDatabase(
            exerciseParametersId,
            3,
            CreateExerciseParameters(exerciseParametersId),
            false,
            1);

    private static ExerciseParameters CreateExerciseParameters(int id)
        => ExerciseParameters.FromDatabase(
            id,
            $"Exercise {id}",
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
            []);

    private static Exercise CreateExercise(int exerciseParametersId)
        => Exercise.New(0, exerciseParametersId, $"Exercise {exerciseParametersId}", 20, 0, 10, []);
}
