using GymTron.App.ViewModels.Pages;

namespace GymTron.App.Pages;

public partial class ExerciseDetailPage : ContentPage
{


    public ExerciseDetailPage(ExerciseDetailPageViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
