namespace GymTron.Application.Trainings.Queries.DTO;

public class TrainingDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int RoutineId { get; set; }
    public int DayOfTheWeek { get; set; }
    public DateTime StartedOn { get; set; }
    public DateTime? CompletedOn { get; set; }
    public int StatusId { get; set; }
    public List<TrainingRoutineItemDto> PendingWorkout { get; set; } = [];
    public List<TrainingExerciseDto> CompletedWorkout { get; set; } = [];
}
