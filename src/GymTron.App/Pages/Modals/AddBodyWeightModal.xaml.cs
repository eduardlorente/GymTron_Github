using GymTron.App.ViewModels.Pages.Modals;

namespace GymTron.App.Pages.Modals;

public partial class AddBodyWeightModal : ContentPage
{
    public AddBodyWeightModal()
    {
        InitializeComponent();
    }

    public AddBodyWeightModal(AddBodyWeightModalViewModel viewModel) : this()
    {
        BindingContext = viewModel;
    }
}