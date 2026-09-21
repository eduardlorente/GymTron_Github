namespace GymTron.App.ViewModels.Entities;

public class TrainingDayItemViewModel(int key, int value)
{
    public int Key { get; } = key;
    public int Value { get; } = value;
}
