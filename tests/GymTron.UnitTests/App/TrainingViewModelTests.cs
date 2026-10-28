using GymTron.App.Services.Api.Models;
using GymTron.App.ViewModels.Entities;
using Xunit;

namespace GymTron.UnitTests.App;

public class TrainingViewModelTests
{
    [Fact]
    public void Constructor_MapsPreviousExerciseMetricsCorrectly()
    {
        TrainingRoutineItemDto routineItem = new(
            Id: 1,
            DayOfWeek: 2,
            ExerciseParametersId: 10,
            ExerciseName: "Bench Press",
            Series: 3,
            RepetitionsMin: 8,
            RepetitionsMax: 12,
            Duration: null,
            MinRestTimeInSeconds: 60,
            MaxRestTimeInSeconds: 90,
            AlternatingSeries: false,
            Position: 1,
            Type: 1,
            LastWeight: 82.5m,
            LastRepetitions: 10,
            LastDuration: 45,
            LastObservations: ["Focus on eccentric phase", "Smooth lockout"],
            Description: "Chest movement",
            Pattern: "Horizontal Press",
            ReplaysInReserve: 2);

        TrainingDto trainingDto = new(
            Id: 100,
            RoutineId: 5,
            DayOfTheWeek: 2,
            StartedOn: new DateTime(2026, 3, 1, 10, 0, 0),
            CompletedOn: null,
            StatusId: 2,
            PendingWorkout: [routineItem],
            CompletedWorkout: []);

        TrainingViewModel vm = new(trainingDto);

        Assert.Equal("100", vm.Id);
        RoutineItemViewModel pendingItem = Assert.Single(vm.PendingWorkout);
        ExerciseParametersViewModel parameters = pendingItem.ExerciseParameters;

        Assert.Equal(82.5m, parameters.LastWeight);
        Assert.Equal(10, parameters.LastRepetitions);
        Assert.Equal(45, parameters.LastDurationInSeconds);
        Assert.Equal(2, parameters.Observations.Count);
        Assert.Equal("Focus on eccentric phase", parameters.Observations[0].Comment);
        Assert.Equal("Smooth lockout", parameters.Observations[1].Comment);
        Assert.Equal("Chest movement", parameters.Description);
        Assert.Equal("Horizontal Press", parameters.Pattern);
        Assert.Equal(2, parameters.ReplaysInReserve);
    }

    [Fact]
    public void Constructor_WithNullPreviousMetrics_HandlesGracefully()
    {
        TrainingRoutineItemDto routineItem = new(
            Id: 1,
            DayOfWeek: 2,
            ExerciseParametersId: 10,
            ExerciseName: "Pushups",
            Series: 3,
            RepetitionsMin: 10,
            RepetitionsMax: 15,
            Duration: null,
            MinRestTimeInSeconds: 60,
            MaxRestTimeInSeconds: null,
            AlternatingSeries: false,
            Position: 1,
            Type: 1,
            LastWeight: null,
            LastRepetitions: null,
            LastDuration: null,
            LastObservations: null);

        TrainingDto trainingDto = new(
            Id: 101,
            RoutineId: 6,
            DayOfTheWeek: 2,
            StartedOn: new DateTime(2026, 3, 2, 10, 0, 0),
            CompletedOn: null,
            StatusId: 2,
            PendingWorkout: [routineItem],
            CompletedWorkout: []);

        TrainingViewModel vm = new(trainingDto);

        RoutineItemViewModel pendingItem = Assert.Single(vm.PendingWorkout);
        ExerciseParametersViewModel parameters = pendingItem.ExerciseParameters;

        Assert.Null(parameters.LastWeight);
        Assert.Null(parameters.LastRepetitions);
        Assert.Null(parameters.LastDurationInSeconds);
        Assert.Empty(parameters.Observations);
    }

    [Fact]
    public void ToDto_RoundTripsPreviousExerciseMetricsAndCompletedExercises()
    {
        TrainingRoutineItemDto originalItem = new(
            Id: 2,
            DayOfWeek: 3,
            ExerciseParametersId: 20,
            ExerciseName: "Pull-ups",
            Series: 4,
            RepetitionsMin: 6,
            RepetitionsMax: 10,
            Duration: 30,
            MinRestTimeInSeconds: 90,
            MaxRestTimeInSeconds: 120,
            AlternatingSeries: true,
            Position: 2,
            Type: 1,
            LastWeight: 15.0m,
            LastRepetitions: 8,
            LastDuration: 30,
            LastObservations: ["Chest to bar"],
            Description: "Back movement",
            Pattern: "Vertical Pull",
            ReplaysInReserve: 1);

        DateTime exerciseTimestamp = new(2026, 3, 3, 10, 15, 0);
        TrainingExerciseDto completedExercise = new(
            Id: 10,
            TrainingId: 102,
            ExerciseParametersId: 20,
            Name: "Pull-ups",
            Weight: 15.0m,
            DurationInSeconds: 30,
            Repetitions: 8,
            Observations: ["Chest to bar"],
            CreatedOn: exerciseTimestamp);

        TrainingDto originalDto = new(
            Id: 102,
            RoutineId: 7,
            DayOfTheWeek: 3,
            StartedOn: new DateTime(2026, 3, 3, 10, 0, 0),
            CompletedOn: null,
            StatusId: 2,
            PendingWorkout: [originalItem],
            CompletedWorkout: [completedExercise]);

        TrainingViewModel vm = new(originalDto);
        TrainingDto convertedDto = vm.ToDto();

        TrainingRoutineItemDto convertedItem = Assert.Single(convertedDto.PendingWorkout);
        Assert.Equal(originalItem.LastWeight, convertedItem.LastWeight);
        Assert.Equal(originalItem.LastRepetitions, convertedItem.LastRepetitions);
        Assert.Equal(originalItem.LastDuration, convertedItem.LastDuration);
        Assert.Equal(originalItem.LastObservations, convertedItem.LastObservations);
        Assert.Equal(originalItem.Description, convertedItem.Description);
        Assert.Equal(originalItem.Pattern, convertedItem.Pattern);
        Assert.Equal(originalItem.ReplaysInReserve, convertedItem.ReplaysInReserve);

        ExerciseViewModel vmExercise = Assert.Single(vm.CompletedWorkout);
        Assert.Equal(exerciseTimestamp, vmExercise.CreatedOn);

        TrainingExerciseDto convertedCompleted = Assert.Single(convertedDto.CompletedWorkout);
        Assert.Equal(exerciseTimestamp, convertedCompleted.CreatedOn);
    }

    [Theory]
    [InlineData(10, 10, "10")]
    [InlineData(8, 12, "8 - 12")]
    [InlineData(0, 15, "15")]
    [InlineData(12, 0, "12")]
    public void RepetitionsFormatting_WhenMinEqualsMax_FormatsSingleValue(int min, int max, string expected)
    {
        RangeViewModel range = new(min, max);
        string literal = FormatRepetitions(range);
        Assert.Equal(expected, literal);
    }

    private static string FormatRepetitions(RangeViewModel repetitions)
    {
        if (repetitions.Min == repetitions.Max)
        {
            return $"{repetitions.Min}";
        }
        else if (repetitions.Min == 0 && repetitions.Max > 0)
        {
            return $"{repetitions.Max}";
        }
        else if (repetitions.Max == 0 && repetitions.Min > 0)
        {
            return $"{repetitions.Min}";
        }

        return $"{repetitions.Min} - {repetitions.Max}";
    }
}
