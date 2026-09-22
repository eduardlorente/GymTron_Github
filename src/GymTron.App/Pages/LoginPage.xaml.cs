using GymTron.App.Extensions;
using GymTron.App.Services;
using GymTron.App.ViewModels.Pages;
using Microsoft.Extensions.Logging;

namespace GymTron.App.Pages;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;
    private readonly ILogger<LoginPage>? _logger;

    public LoginPage(LoginViewModel viewModel, ILogger<LoginPage>? logger = null)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _logger = logger;
        BindingContext = viewModel;

        _viewModel.RequestBiometricEnrollmentPrompt = async () =>
        {
            return await DisplayAlert(
                LocalizationService.GetString("Login_BiometricEnroll_Title"),
                LocalizationService.GetString("Login_BiometricEnroll_Message"),
                LocalizationService.GetString("Login_BiometricEnroll_Confirm"),
                LocalizationService.GetString("Login_BiometricEnroll_Cancel"));
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.InitializeAsync().SafeFireAndForget(logger: _logger);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.Dispose();
    }
}
