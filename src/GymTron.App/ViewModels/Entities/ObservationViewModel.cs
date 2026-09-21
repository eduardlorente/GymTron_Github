namespace GymTron.App.ViewModels.Entities;

public class ObservationViewModel
{
    public string Comment { get; set; } = string.Empty;

    public ObservationViewModel()
    {
        // For serialization purposes
    }

    public ObservationViewModel(string comment)
    {
        Comment = comment;
    }
}
