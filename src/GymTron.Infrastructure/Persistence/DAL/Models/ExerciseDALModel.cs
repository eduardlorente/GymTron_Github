namespace GymTron.Infrastructure.Persistence.DAL.Models;

internal class ExerciseDALModel
{
    public int Id { get; set; }
    public int TrainingId { get; set; }
    public int ExerciseParametersId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public int DurationInSeconds { get; set; }
    public int Repetitions { get; set; }
    public DateTime CreatedOn { get; set; }
    public string ObservationsCSV { get; set; } = string.Empty;
}