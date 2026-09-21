using GymTron.App.Services.Api.Models;

namespace GymTron.App.ViewModels.Entities;

public class RoutineViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Dictionary<int, List<RoutineItemViewModel>> WorkByDays { get; set; } = [];

    public RoutineViewModel()
    {
        // For serialization purposes
    }

    public RoutineViewModel(RoutineDto routine)
    {
        Id = routine.Id;
        Name = routine.Name;
        WorkByDays = routine.Items
            .GroupBy(i => i.DayOfWeek)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Select(ri => new RoutineItemViewModel(ri)).ToList()
            );
    }
}
