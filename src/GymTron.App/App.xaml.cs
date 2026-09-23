using GymTron.App.Pages;
using GymTron.App.Services;
using GymTron.App.Services.Auth;
using GymTron.App.ViewModels.Pages;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace GymTron.App;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly ILogger<App> _logger;
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider, ILogger<App> logger)
    {
        InitializeComponent();

        _logger = logger;
        _serviceProvider = serviceProvider;

        // Initialize localization from user preferences (or default culture)
        LocalizationService.Initialize();

        var authService = serviceProvider.GetRequiredService<IAuthService>();
        var sessionManager = serviceProvider.GetRequiredService<ISessionManager>();

        authService.SessionExpired += (s, e) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                sessionManager.Lock();
                SwitchToLogin();
            });
        };

        if (sessionManager.IsBiometricsEnabled)
        {
            SwitchToLogin();
        }
        else if (sessionManager.IsSessionActive())
        {
            SwitchToShell();
        }
        else
        {
            SwitchToLogin();
        }
    }

    public void SwitchToShell()
    {
        ITrainingService trainingService = _serviceProvider.GetRequiredService<ITrainingService>();
        MainPage = new AppShell(trainingService);
    }

    public void SwitchToLogin()
    {
        var loginPage = _serviceProvider.GetRequiredService<LoginPage>();
        if (loginPage.BindingContext is LoginViewModel vm)
        {
            vm.LoginSuccessful -= OnLoginSuccessful;
            vm.LoginSuccessful += OnLoginSuccessful;
        }
        MainPage = loginPage;
    }

    private void OnLoginSuccessful(object? sender, EventArgs e)
    {
        SwitchToShell();
    }

    protected override void OnResume()
    {
        base.OnResume();

        var sessionManager = _serviceProvider.GetService<ISessionManager>();
        if (sessionManager != null && !sessionManager.IsSessionActive())
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (MainPage is not LoginPage)
                {
                    _logger.LogInformation("12-hour session expired while backgrounded. Locking app.");
                    SwitchToLogin();
                }
            });
        }
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            _logger.LogError(ex, "Unhandled Exception");
            HandleException(ex, "Unhandled Exception");
        }
    }

    private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        _logger.LogError(e.Exception, "Unobserved Task Exception");
        HandleException(e.Exception, "Unobserved Task Exception");
        e.SetObserved();
    }

    private void HandleException(Exception ex, string source)
    {
        Debug.WriteLine($"[{source}] {ex}");

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            Page? currentPage = Current?.MainPage ?? Shell.Current;
            if (currentPage != null)
            {
                await currentPage.DisplayAlert(
                    "Error",
                    LocalizationService.GetString("Common_UnexpectedError"),
                    LocalizationService.GetString("Dialog_Ok")
                );
            }
        });
    }
}
