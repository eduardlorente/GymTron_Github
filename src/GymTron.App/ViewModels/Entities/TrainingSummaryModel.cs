namespace GymTron.App.ViewModels.Entities;

public class CompletedExerciseSummaryItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public int Repetitions { get; set; }
    public int DurationInSeconds { get; set; }
    public decimal VolumeKg => Weight * Repetitions;
    public string Observations { get; set; } = string.Empty;
    public bool HasObservations => !string.IsNullOrWhiteSpace(Observations);
    public bool IsWeightBased => DurationInSeconds == 0;
    public bool IsDurationBased => DurationInSeconds > 0;

    public string MetricDisplay
    {
        get
        {
            if (IsDurationBased)
            {
                var span = TimeSpan.FromSeconds(DurationInSeconds);
                return span.Hours > 0
                    ? $"{span.Hours}h {span.Minutes}m {span.Seconds}s"
                    : $"{span.Minutes}m {span.Seconds}s";
            }

            return $"{Weight:G29} kg × {Repetitions} reps";
        }
    }
}

public class TrainingSummaryModel
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime > StartTime ? EndTime - StartTime : TimeSpan.Zero;

    public string FormattedDuration
    {
        get
        {
            var d = Duration;
            return $"{d.Hours:D2}:{d.Minutes:D2}:{d.Seconds:D2}";
        }
    }

    public int CompletedExercisesCount => CompletedExercises.Count;
    public int PendingExercisesCount => PendingExercises.Count;
    public int TotalPlannedExercises => CompletedExercisesCount + PendingExercisesCount;

    public decimal TotalVolumeKg => CompletedExercises.Where(e => e.IsWeightBased).Sum(e => e.VolumeKg);
    public int TotalRepetitions => CompletedExercises.Where(e => e.IsWeightBased).Sum(e => e.Repetitions);

    public List<CompletedExerciseSummaryItem> CompletedExercises { get; set; } = [];
    public List<string> PendingExercises { get; set; } = [];

    public bool HasPendingExercises => PendingExercises.Count > 0;

    public static TrainingSummaryModel FromTraining(TrainingViewModel training, DateTime endTime)
    {
        return new TrainingSummaryModel
        {
            StartTime = training.StartedOn.FullDate,
            EndTime = endTime,
            CompletedExercises = [.. training.CompletedWorkout.Select(e => new CompletedExerciseSummaryItem
            {
                Name = e.Name,
                Weight = e.Weight,
                Repetitions = e.CurrentRepetitions,
                DurationInSeconds = e.DurationInSeconds,
                Observations = string.Join("; ", e.Observations.Select(o => o.Comment).Where(t => !string.IsNullOrWhiteSpace(t)))
            })],
            PendingExercises = [.. training.PendingWorkout.Select(p => p.ExerciseParameters.Name)]
        };
    }
}
