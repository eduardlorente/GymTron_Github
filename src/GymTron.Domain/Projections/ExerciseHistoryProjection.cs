namespace GymTron.Domain.Projections;

public class ExerciseHistoryProjection
{
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.MinValue;
    public decimal? Weight { get; set; }
    public int? Repetitions { get; set; }
    public int? DurationInSeconds { get; set; }
}
