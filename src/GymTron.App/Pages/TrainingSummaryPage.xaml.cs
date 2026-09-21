using GymTron.App.ViewModels.Pages;

namespace GymTron.App.Pages;

public partial class TrainingSummaryPage : ContentPage
{
    public TrainingSummaryPage(TrainingSummaryPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
