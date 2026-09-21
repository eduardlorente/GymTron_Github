using GymTron.Domain.Aggregates;
using GymTron.Domain.Entities;
using GymTron.Domain.Enums;
using GymTron.Domain.Projections;
using GymTron.Domain.ValueObjects;

namespace GymTron.UnitTests.Domain;

public class RehydrationAndProjectionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Rehydrate_WithOptionalCompletion_PreservesTraining(bool completed)
    {
        DateTime startedOn = new(2026, 3, 4, 5, 6, 7);
        DateTime? completedOn = completed ? startedOn.AddHours(1) : null;
        List<RoutineItem> pending = [CreateRoutineItem(11)];
        List<Exercise> exercises = [Exercise.New(5, 12, "Row", 50, 0, 10, [])];

        Training training = Training.FromDatabase(
            5, 7, 2, startedOn, completedOn,
            completed ? EntityStatusTypes.COMPLETED : EntityStatusTypes.ACTIVE,
            pending, exercises);

        Assert.Equal(5, training.Id);
        Assert.Equal(7, training.RoutineId);
        Assert.Equal(2, training.DayOfTheWeek);
        Assert.Equal(startedOn, training.StartedOn.FullDate);
        Assert.Same(pending, training.PendingWorkout);
        Assert.Same(exercises, training.CompletedWorkout);
        Assert.Equal(completedOn, training.CompletedOn?.FullDate);
    }

    [Fact]
    public void Projections_WithAssignedValues_PreserveReadModelContracts()
    {
        DateTime date = new(2026, 4, 5);
        TrainingHistoryProjection training = new() { StartedOn = new(date), DayOfTheWeek = 3 };
        ExerciseHistoryProjection exercise = new()
        {
            Name = "Press",
            CreatedOn = date,
            Weight = 25,
            Repetitions = 8,
            DurationInSeconds = 30
        };
        BodyWeightHistoryProjection weight = new()
        {
            CreatedOn = date,
            Weight = 80,
            BodyFatPercentage = 15
        };

        RoutineItemProjection item = new()
        {
            Id = 1,
            DayOfWeek = 2,
            ExerciseParametersId = 3,
            ExerciseName = "Bench Press",
            Series = 4,
            RepetitionsMin = 8,
            RepetitionsMax = 12,
            Duration = 45,
            MinRestTimeInSeconds = 60,
            MaxRestTimeInSeconds = 90,
            AlternatingSeries = true,
            Position = 1,
            Type = ExerciseTypes.WEIGHT
        };

        RoutineProjection routine = new()
        {
            Id = 10,
            Name = "Push Day",
            Items = [item]
        };

        ExerciseParameterProjection param = new()
        {
            Id = 20,
            Name = "Squat",
            Description = "Back Squat",
            Pattern = "Squat",
            Type = ExerciseTypes.WEIGHT,
            ReplaysInReserve = 2
        };

        Assert.Equal(date, training.StartedOn.FullDate);
        Assert.Equal(3, training.DayOfTheWeek);
        Assert.Equal(("Press", date, 25m, 8, 30),
            (exercise.Name, exercise.CreatedOn, exercise.Weight, exercise.Repetitions, exercise.DurationInSeconds));
        Assert.Equal((date, 80m, 15m), (weight.CreatedOn, weight.Weight, weight.BodyFatPercentage));

        Assert.Equal(10, routine.Id);
        Assert.Equal("Push Day", routine.Name);
        Assert.Single(routine.Items);
        Assert.Equal((1, 2, 3, "Bench Press", 4, 8, 12, 45, 60, 90, true, 1, ExerciseTypes.WEIGHT),
            (item.Id, item.DayOfWeek, item.ExerciseParametersId, item.ExerciseName, item.Series,
             item.RepetitionsMin, item.RepetitionsMax, item.Duration, item.MinRestTimeInSeconds,
             item.MaxRestTimeInSeconds, item.AlternatingSeries, item.Position, item.Type));

        Assert.Equal((20, "Squat", "Back Squat", "Squat", ExerciseTypes.WEIGHT, 2),
            (param.Id, param.Name, param.Description, param.Pattern, param.Type, param.ReplaysInReserve));
    }

    private static RoutineItem CreateRoutineItem(int id)
        => RoutineItem.FromDatabase(
            id,
            2,
            ExerciseParameters.FromDatabase(
                id, "Exercise", "Description", "Pattern", 3, (8, 12), 0, null,
                (60, 90), null, null, null, ExerciseTypes.WEIGHT, []),
            false,
            1);
}
