namespace GymTron.Infrastructure.Persistence.DAL.Models;

internal class RoutineFullDetailsDTO
{
    public int RoutineId { get; set; }
    public int? UserId { get; set; }
    public string RoutineName { get; set; } = string.Empty;
    //
    public int RoutineItemId { get; set; }
    public int DayOfWeek { get; set; }
    public int ExerciseParametersId { get; set; }
    public int MinRestTimeInSeconds { get; set; }
    public int? MaxRestTimeInSeconds { get; set; } 
    public bool AlternatingSeries { get; set; }
    public bool Active { get; set; }
    public int Position { get; set; }
    //
    public string ExerciseName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public int Series { get; set; }
    public int RepetitionsMin { get; set; }
    public int RepetitionsMax { get; set; }
    public int? Duration { get; set; } 
    public int ReplaysInReserve { get; set; }
    public int TypeId { get; set; }
    //
    public decimal? LastWeight { get; set; }
    public int? LastDuration { get; set; }
    public int? LastRepetitions { get; set; }
    public string LastObservations { get; set; } = string.Empty;
}
