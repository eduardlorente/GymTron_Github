using GymTron.App.ViewModels.Pages;

namespace GymTron.App.Pages;

public partial class StartTrainingPage : ContentPage
{


    public StartTrainingPage(StartTrainingPageViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
