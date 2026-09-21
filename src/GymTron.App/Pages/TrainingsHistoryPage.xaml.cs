using GymTron.App.ViewModels.Pages;

namespace GymTron.App.Pages;

public partial class TrainingsHistoryPage : ContentPage
{


    public TrainingsHistoryPage(TrainingsHistoryPageViewModel viewModel)
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
