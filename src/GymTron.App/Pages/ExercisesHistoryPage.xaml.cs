using GymTron.App.ViewModels.Pages;

namespace GymTron.App.Pages;

public partial class ExercisesHistoryPage : ContentPage
{


    public ExercisesHistoryPage(ExercisesHistoryPageViewModel viewModel)
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
