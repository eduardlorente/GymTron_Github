using GymTron.App.Services.Api.Models;

namespace GymTron.App.ViewModels.Entities;

public class TrainingViewModel
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
    public int RoutineId { get; set; }
    public int DayOfTheWeek { get; set; }
    public int StatusId { get; set; }
    public TrainingDateViewModel StartedOn { get; set; } = new(DateTime.MinValue);
    public TrainingDateViewModel? CompletedOn { get; set; }
    public List<RoutineItemViewModel> PendingWorkout { get; set; } = [];
    public List<ExerciseViewModel> CompletedWorkout { get; set; } = [];

    public TrainingViewModel()
    {
        // For serialization purposes
    }

    public TrainingViewModel(TrainingDto training)
    {
        Id = training.Id.ToString();
        CreatedOn = training.StartedOn;
        RoutineId = training.RoutineId;
        DayOfTheWeek = training.DayOfTheWeek;
        StatusId = training.StatusId;
        StartedOn = new TrainingDateViewModel(training.StartedOn);
        CompletedOn = training.CompletedOn.HasValue ? new TrainingDateViewModel(training.CompletedOn.Value) : null;
        PendingWorkout = [.. training.PendingWorkout.Select(ri => new RoutineItemViewModel
        {
            Id = ri.Id,
            DayOfWeek = ri.DayOfWeek,
            ExerciseParameters = new ExerciseParametersViewModel
            {
                Id = ri.ExerciseParametersId,
                Name = ri.ExerciseName,
                Series = ri.Series,
                Repetitions = new RangeViewModel(ri.RepetitionsMin, ri.RepetitionsMax),
                DurationInSeconds = ri.Duration ?? 0,
                RestTimeInSeconds = new RangeViewModel(ri.MinRestTimeInSeconds, ri.MaxRestTimeInSeconds ?? 0),
                TypeId = ri.Type
            },
            AlternatingSeries = ri.AlternatingSeries,
            Position = ri.Position
        })];
        CompletedWorkout = [.. training.CompletedWorkout.Select(e => new ExerciseViewModel
        {
            Id = e.Id,
            TrainingId = e.TrainingId,
            ExerciseParametersId = e.ExerciseParametersId,
            Name = e.Name,
            Weight = e.Weight,
            DurationInSeconds = e.DurationInSeconds,
            CurrentRepetitions = e.Repetitions,
            Observations = [.. e.Observations.Select(obs => new ObservationViewModel(obs))]
        })];
    }

    internal TrainingDto ToDto()
        => new(
            Convert.ToInt32(Id),
            RoutineId,
            DayOfTheWeek,
            StartedOn.FullDate,
            CompletedOn?.FullDate,
            StatusId,
            [.. PendingWorkout.Select(ri => new TrainingRoutineItemDto(
                ri.Id,
                ri.DayOfWeek,
                ri.ExerciseParameters.Id,
                ri.ExerciseParameters.Name,
                ri.ExerciseParameters.Series,
                ri.ExerciseParameters.Repetitions.Min,
                ri.ExerciseParameters.Repetitions.Max,
                ri.ExerciseParameters.DurationInSeconds,
                ri.ExerciseParameters.RestTimeInSeconds.Min,
                ri.ExerciseParameters.RestTimeInSeconds.Max,
                ri.AlternatingSeries,
                ri.Position,
                ri.ExerciseParameters.TypeId))],
            [.. CompletedWorkout.Select(e => new TrainingExerciseDto(
                e.Id,
                e.TrainingId,
                e.ExerciseParametersId,
                e.Name,
                e.Weight,
                e.DurationInSeconds,
                e.CurrentRepetitions,
                e.Observations.Select(o => o.Comment).ToList(),
                e.CreatedOn))]);
}
