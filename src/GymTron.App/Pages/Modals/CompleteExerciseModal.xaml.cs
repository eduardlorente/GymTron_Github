using GymTron.App.ViewModels.Pages.Modals;

namespace GymTron.App.Pages.Modals;

public partial class CompleteExerciseModal : ContentPage
{
    public CompleteExerciseModal()
    {
        InitializeComponent();
    }

    public CompleteExerciseModal(CompleteExerciseModalViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }
}