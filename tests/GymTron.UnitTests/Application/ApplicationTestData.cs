using GymTron.Domain.Aggregates;
using GymTron.Domain.Entities;
using GymTron.Domain.Enums;

namespace GymTron.UnitTests.Application;

internal static class ApplicationTestData
{
    internal static ExerciseParameters CreateExerciseParameters(int id = 11, ExerciseTypes type = ExerciseTypes.WEIGHT)
        => ExerciseParameters.FromDatabase(
            id,
            $"Exercise {id}",
            "Description",
            "Pattern",
            3,
            (8, 12),
            type == ExerciseTypes.DURATION ? 45 : 0,
            2,
            (60, 90),
            80m,
            40,
            10,
            type,
            []);

    internal static RoutineItem CreateRoutineItem(int id = 11, int dayOfWeek = 3, ExerciseTypes type = ExerciseTypes.WEIGHT)
        => RoutineItem.FromDatabase(id, dayOfWeek, CreateExerciseParameters(id, type), true, 2);

    internal static Training CreateTraining(
        int id = 5,
        EntityStatusTypes status = EntityStatusTypes.ACTIVE,
        DateTime? startedOn = null,
        DateTime? completedOn = null,
        List<RoutineItem>? pendingWorkout = null,
        List<Exercise>? completedWorkout = null)
        => Training.FromDatabase(
            id,
            7,
            3,
            startedOn ?? new DateTime(2026, 1, 2, 3, 4, 5),
            completedOn,
            status,
            pendingWorkout ?? [CreateRoutineItem()],
            completedWorkout ?? []);
}
