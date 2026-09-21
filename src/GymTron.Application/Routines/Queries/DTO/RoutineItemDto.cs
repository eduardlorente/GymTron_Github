using GymTron.Domain.Enums;

namespace GymTron.Application.Routines.Queries.DTO;

public class RoutineItemDto
{
    public int Id { get; set; }
    public int DayOfWeek { get; set; }
    public int ExerciseParametersId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int Series { get; set; }
    public int RepetitionsMin { get; set; }
    public int RepetitionsMax { get; set; }
    public int? Duration { get; set; }
    public int MinRestTimeInSeconds { get; set; }
    public int? MaxRestTimeInSeconds { get; set; }
    public bool AlternatingSeries { get; set; }
    public int Position { get; set; }
    public ExerciseTypes Type { get; set; }
}
