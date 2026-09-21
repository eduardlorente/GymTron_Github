namespace GymTron.App.ViewModels.Entities;

public class TrainingHistoryItemViewModel(int year, string month, int count, Dictionary<int, int> trainingDays)
{
    public int Year { get; } = year;
    public string Month { get; } = month;
    public int Count { get; } = count;
    public List<TrainingDayItemViewModel> TrainingDays { get; } =
        trainingDays.Select(kvp => new TrainingDayItemViewModel(kvp.Key, kvp.Value)).ToList();
}
