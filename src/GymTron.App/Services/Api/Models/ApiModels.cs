namespace GymTron.App.Services.Api.Models;

public enum ExerciseTypes
{
    WEIGHT = 1,
    DURATION = 2
}

public enum EntityStatusTypes
{
    CREATED = 1,
    STARTED = 2,
    COMPLETED = 3,
    CANCELLED = 4
}

public record RoutineItemDto(
    int Id,
    int DayOfWeek,
    int ExerciseParametersId,
    string ExerciseName,
    int Series,
    int RepetitionsMin,
    int RepetitionsMax,
    int? Duration,
    int MinRestTimeInSeconds,
    int? MaxRestTimeInSeconds,
    bool AlternatingSeries,
    int Position,
    int Type);

public record RoutineDto(
    int Id,
    string Name,
    List<RoutineItemDto> Items);

public record ExerciseHistoryItemDto(
    string Name,
    DateTime CreatedOn,
    decimal? Weight,
    int? Repetitions,
    int? DurationInSeconds);

public record BodyWeightHistoryDto(
    DateTime CreatedOn,
    decimal Weight,
    decimal BodyFatPercentage);

public record RegisterBodyWeightRequest(
    decimal Weight,
    decimal BodyFatPercentage);

public record TrainingRoutineItemDto(
    int Id,
    int DayOfWeek,
    int ExerciseParametersId,
    string ExerciseName,
    int Series,
    int RepetitionsMin,
    int RepetitionsMax,
    int? Duration,
    int MinRestTimeInSeconds,
    int? MaxRestTimeInSeconds,
    bool AlternatingSeries,
    int Position,
    int Type,
    decimal? LastWeight = null,
    int? LastRepetitions = null,
    int? LastDuration = null,
    List<string>? LastObservations = null);

public record TrainingExerciseDto(
    int Id,
    int TrainingId,
    int ExerciseParametersId,
    string Name,
    decimal Weight,
    int DurationInSeconds,
    int Repetitions,
    List<string> Observations,
    DateTime CreatedOn);

public record TrainingDto(
    int Id,
    int RoutineId,
    int DayOfTheWeek,
    DateTime StartedOn,
    DateTime? CompletedOn,
    int StatusId,
    List<TrainingRoutineItemDto> PendingWorkout,
    List<TrainingExerciseDto> CompletedWorkout);

public record TrainingHistoryDto(
    DateTime StartedOn,
    int DayOfTheWeek);

public record StartTrainingRequest(
    int RoutineId,
    int DayOfWeek);

public record FinishTrainingRequest(
    TrainingDto Training);

public record CancelTrainingRequest(
    TrainingDto Training);

public record AddExerciseToTrainingRequest(
    TrainingDto CurrentTraining,
    int ExerciseParametersId,
    string ExerciseParametersName,
    decimal? Weight,
    int? Repetitions,
    int? DurationInSeconds,
    List<string> Observations);
