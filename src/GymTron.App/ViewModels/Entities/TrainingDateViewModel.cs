namespace GymTron.App.ViewModels.Entities;

public class TrainingDateViewModel
{


    public DateTime FullDate { get; set; }


    public TrainingDateViewModel()
    {
        //For serialization purposes
    }


    public TrainingDateViewModel(DateTime fullDate)
    {
        FullDate = fullDate;
    }
}
