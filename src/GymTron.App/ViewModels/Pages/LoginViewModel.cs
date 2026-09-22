using System.Windows.Input;
using GymTron.App.Services;
using GymTron.App.Services.Auth;
using GymTron.App.Services.Biometrics;
using Microsoft.Extensions.Logging;

namespace GymTron.App.ViewModels.Pages;

public sealed class LoginViewModel : PageBaseViewModel, IDisposable
{
    private readonly IAuthService _authService;
    private readonly ISessionManager _sessionManager;
    private readonly IBiometricService _biometricService;
    private readonly ILogger<LoginViewModel>? _logger;

    private string _identifier = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _hasError;
    private bool _canUseBiometric;
    private bool _disposed;

    public event EventHandler? LoginSuccessful;
    public Func<Task<bool>>? RequestBiometricEnrollmentPrompt { get; set; }

    public string Identifier
    {
        get => _identifier;
        set
        {
            if (SetProperty(ref _identifier, value))
            {
                HasError = false;
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                HasError = false;
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool HasError
    {
        get => _hasError;
        set => SetProperty(ref _hasError, value);
    }

    public bool CanUseBiometric
    {
        get => _canUseBiometric;
        set => SetProperty(ref _canUseBiometric, value);
    }

    public string TitleText => LocalizationService.GetString("Login_Title");
    public string SubtitleText => LocalizationService.GetString("Login_Subtitle");
    public string IdentifierPlaceholder => LocalizationService.GetString("Login_Identifier");
    public string PasswordPlaceholder => LocalizationService.GetString("Login_Password");
    public string SubmitButtonText => LocalizationService.GetString("Login_SubmitButton");
    public string BiometricButtonText => LocalizationService.GetString("Login_BiometricButton");

    public ICommand LoginCommand { get; }
    public ICommand BiometricLoginCommand { get; }

    public LoginViewModel(
        IAuthService authService,
        ISessionManager sessionManager,
        IBiometricService biometricService,
        ILogger<LoginViewModel>? logger = null)
    {
        _authService = authService;
        _sessionManager = sessionManager;
        _biometricService = biometricService;
        _logger = logger;

        LocalizationService.CultureChanged += OnCultureChanged;

        LoginCommand = new Command(async () => await ExecuteLoginAsync(), () => !IsBusy);
        BiometricLoginCommand = new Command(async () => await ExecuteBiometricLoginAsync(), () => !IsBusy);
    }

    public async Task InitializeAsync()
    {
        CanUseBiometric = await _sessionManager.CanUseBiometricAsync() && await _authService.IsAuthenticatedAsync();

        if (CanUseBiometric && _sessionManager.LastAuthMethod == AuthMethod.Biometric)
        {
            await ExecuteBiometricLoginAsync();
        }
    }

    private void SetBusy(bool busy)
    {
        IsBusy = busy;
        if (LoginCommand is Command loginCmd)
        {
            loginCmd.ChangeCanExecute();
        }
        if (BiometricLoginCommand is Command bioCmd)
        {
            bioCmd.ChangeCanExecute();
        }
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(TitleText));
        OnPropertyChanged(nameof(SubtitleText));
        OnPropertyChanged(nameof(IdentifierPlaceholder));
        OnPropertyChanged(nameof(PasswordPlaceholder));
        OnPropertyChanged(nameof(SubmitButtonText));
        OnPropertyChanged(nameof(BiometricButtonText));
    }

    private async Task ExecuteLoginAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Identifier) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = LocalizationService.GetString("Login_Error_RequiredFields");
            HasError = true;
            return;
        }

        try
        {
            SetBusy(true);
            HasError = false;

            bool success = await _authService.LoginAsync(Identifier.Trim(), Password);
            if (!success)
            {
                ErrorMessage = LocalizationService.GetString("Login_Error_InvalidCredentials");
                HasError = true;
                return;
            }

            _sessionManager.RecordUnlock(AuthMethod.Credentials);
            Password = string.Empty;

            if (!_sessionManager.HasEnrolledBiometricsPromptBeenShown &&
                await _biometricService.IsBiometricAvailableAsync())
            {
                _sessionManager.HasEnrolledBiometricsPromptBeenShown = true;
                if (RequestBiometricEnrollmentPrompt != null)
                {
                    bool enable = await RequestBiometricEnrollmentPrompt();
                    if (enable)
                    {
                        _sessionManager.IsBiometricsEnabled = true;
                        _sessionManager.LastAuthMethod = AuthMethod.Biometric;
                    }
                }
            }

            LoginSuccessful?.Invoke(this, EventArgs.Empty);
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogWarning(ex, "Network failure while authenticating user.");
            ErrorMessage = LocalizationService.GetString("Login_Error_Network");
            HasError = true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error during login.");
            ErrorMessage = LocalizationService.GetString("Login_Error_InvalidCredentials");
            HasError = true;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task ExecuteBiometricLoginAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            SetBusy(true);
            HasError = false;

            bool isAuthed = await _authService.IsAuthenticatedAsync();
            if (!isAuthed)
            {
                ErrorMessage = LocalizationService.GetString("Login_Error_InvalidCredentials");
                HasError = true;
                return;
            }

            bool authenticated = await _biometricService.AuthenticateAsync(
                LocalizationService.GetString("Login_Biometric_Title"),
                LocalizationService.GetString("Login_Biometric_Reason"),
                LocalizationService.GetString("Login_Biometric_Cancel"));

            if (authenticated)
            {
                _sessionManager.RecordUnlock(AuthMethod.Biometric);
                Password = string.Empty;
                LoginSuccessful?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _logger?.LogInformation("Biometric authentication was cancelled or unverified.");
                ErrorMessage = LocalizationService.GetString("Login_BiometricFailed");
                HasError = true;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error during biometric login.");
            ErrorMessage = LocalizationService.GetString("Login_BiometricFailed");
            HasError = true;
        }
        finally
        {
            SetBusy(false);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            LocalizationService.CultureChanged -= OnCultureChanged;
            _disposed = true;
        }
    }
}
