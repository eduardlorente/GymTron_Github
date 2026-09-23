using GymTron.App.ViewModels.Pages;

namespace GymTron.App;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MainPageViewModel vm)
        {
            _ = vm.LoadLoggedUserAsync();
        }
    }
}
