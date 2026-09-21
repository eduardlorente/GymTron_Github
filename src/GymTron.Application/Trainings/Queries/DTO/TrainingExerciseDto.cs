namespace GymTron.Application.Trainings.Queries.DTO;

public class TrainingExerciseDto
{
    public int Id { get; set; }
    public int TrainingId { get; set; }
    public int ExerciseParametersId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public int DurationInSeconds { get; set; }
    public int Repetitions { get; set; }
    public List<string> Observations { get; set; } = [];
    public DateTime CreatedOn { get; set; }
}
