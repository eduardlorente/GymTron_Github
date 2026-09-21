using GymTron.App.Services.Api.Models;

namespace GymTron.App.ViewModels.Entities;

public class RoutineItemViewModel
{
    public int Id { get; set; }
    public int DayOfWeek { get; set; }
    public ExerciseParametersViewModel ExerciseParameters { get; set; } = new();
    public bool AlternatingSeries { get; set; }
    public int Position { get; set; }

    public RoutineItemViewModel()
    {
        // For serialization purposes
    }

    public RoutineItemViewModel(RoutineItemDto item)
    {
        Id = item.Id;
        DayOfWeek = item.DayOfWeek;
        ExerciseParameters = new ExerciseParametersViewModel
        {
            Id = item.ExerciseParametersId,
            Name = item.ExerciseName,
            Series = item.Series,
            Repetitions = new RangeViewModel(item.RepetitionsMin, item.RepetitionsMax),
            DurationInSeconds = item.Duration ?? 0,
            RestTimeInSeconds = new RangeViewModel(item.MinRestTimeInSeconds, item.MaxRestTimeInSeconds ?? 0),
            TypeId = item.Type
        };
        AlternatingSeries = item.AlternatingSeries;
        Position = item.Position;
    }
}
