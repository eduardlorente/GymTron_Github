using GymTron.App.Services.Api.Models;

namespace GymTron.App.ViewModels.Entities;

public class ExerciseViewModel
{
    public int Id { get; set; }
    public DateTime CreatedOn { get; set; }
    public int TrainingId { get; set; }
    public int ExerciseParametersId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public int DurationInSeconds { get; set; }
    public int CurrentRepetitions { get; set; }
    public List<ObservationViewModel> Observations { get; set; } = [];
    public bool HasWeight => DurationInSeconds == 0;
    public bool HasDuration => DurationInSeconds > 0;

    public ExerciseViewModel()
    {
        // For serialization purposes
    }

    public ExerciseViewModel(ExerciseHistoryItemDto exercise)
    {
        Name = exercise.Name;
        CreatedOn = exercise.CreatedOn;
        Weight = exercise.Weight ?? 0;
        CurrentRepetitions = exercise.Repetitions ?? 0;
        DurationInSeconds = exercise.DurationInSeconds ?? 0;
    }
}
