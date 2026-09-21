using GymTron.App.Services.Api.Models;

namespace GymTron.App.ViewModels.Entities;

public class ExerciseParametersViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public RangeViewModel Repetitions { get; set; } = new();
    public int DurationInSeconds { get; set; }
    public int Series { get; set; }
    public int? ReplaysInReserve { get; set; }
    public RangeViewModel RestTimeInSeconds { get; set; } = new();
    public decimal? LastWeight { get; set; }
    public int? LastDurationInSeconds { get; set; }
    public int? LastRepetitions { get; set; }
    public int TypeId { get; set; }
    public List<ObservationViewModel> Observations { get; set; } = [];

    public ExerciseParametersViewModel()
    {
        // For serialization purposes
    }
}
