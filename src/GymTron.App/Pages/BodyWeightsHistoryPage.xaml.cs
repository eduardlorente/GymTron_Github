using GymTron.App.ViewModels.Pages;

namespace GymTron.App.Pages;

public partial class BodyWeightsHistoryPage : ContentPage
{


    public BodyWeightsHistoryPage(BodyWeightsHistoryPageViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }


    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("//MainPage");
        return true;
    }
}
