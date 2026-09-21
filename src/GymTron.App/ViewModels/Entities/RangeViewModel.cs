namespace GymTron.App.ViewModels.Entities;

public class RangeViewModel
{
    public int Min { get; set; }
    public int Max { get; set; }

    public RangeViewModel()
    {
        // For serialization purposes
    }

    public RangeViewModel(int min, int max)
    {
        Min = min;
        Max = max;
    }

    public RangeViewModel((int Min, int Max) range)
    {
        Min = range.Min;
        Max = range.Max;
    }
}
