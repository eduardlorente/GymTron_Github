using GymTron.App.ViewModels.Pages;

namespace GymTron.App.Pages;

public partial class CurrentTrainingPage : ContentPage
{


    public CurrentTrainingPage(CurrentTrainingPageViewModel viewModel)
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