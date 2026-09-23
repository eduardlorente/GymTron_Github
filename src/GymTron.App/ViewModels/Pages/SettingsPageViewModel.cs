using System.Globalization;
using System.Windows.Input;
using GymTron.App.Services;
using GymTron.App.Services.Api;
using GymTron.App.Services.Auth;
using GymTron.App.Services.Biometrics;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace GymTron.App.ViewModels.Pages;

public sealed class SettingsPageViewModel : PageBaseViewModel, IDisposable
{
    private readonly IAuthService _authService;
    private readonly ISessionManager _sessionManager;
    private readonly IBiometricService _biometricService;
    private readonly IGymTronApiClient _apiClient;
    private readonly IShare _share;
    private readonly List<CultureInfo> _cultures;
    private int _selectedLanguageIndex;
    private bool _isBiometricsSupported;
    private bool _isTogglingBiometrics;
    private bool _isExportingBackup;
    private bool _disposed;

    public List<string> LanguageNames { get; }

    public int SelectedLanguageIndex
    {
        get => _selectedLanguageIndex;
        set
        {
            if (SetProperty(ref _selectedLanguageIndex, value) && value >= 0 && value < _cultures.Count)
            {
                LocalizationService.SetCulture(_cultures[value]);
            }
        }
    }

    public bool IsBiometricsSupported
    {
        get => _isBiometricsSupported;
        private set
        {
            if (SetProperty(ref _isBiometricsSupported, value))
            {
                OnPropertyChanged(nameof(ShowBiometricsNotSupported));
            }
        }
    }

    public bool ShowBiometricsNotSupported => !IsBiometricsSupported;

    public bool IsBiometricsEnabled
    {
        get => _sessionManager.IsBiometricsEnabled;
        set
        {
            if (_sessionManager.IsBiometricsEnabled != value && !_isTogglingBiometrics)
            {
                _ = HandleBiometricToggledAsync(value);
            }
        }
    }

    public string Title => LocalizationService.GetString("Settings_Title");
    public string LanguageSectionTitle => LocalizationService.GetString("Settings_Language");
    public string LanguageTitle => LocalizationService.GetString("Language_Title");
    public string SecuritySectionTitle => LocalizationService.GetString("Settings_Security");
    public string BiometricsTitle => LocalizationService.GetString("Settings_Biometrics");
    public string BiometricsDesc => LocalizationService.GetString("Settings_Biometrics_Desc");
    public string BiometricsNotSupportedText => LocalizationService.GetString("Settings_Biometrics_NotSupported");
    public string BackupSectionTitle => LocalizationService.GetString("Settings_Backup");
    public string BackupDesc => LocalizationService.GetString("Settings_Backup_Desc");
    public string BackupButtonText => LocalizationService.GetString("Settings_Backup_Button");
    public string LogoutText => LocalizationService.GetString("Settings_Logout");

    public bool IsExportingBackup
    {
        get => _isExportingBackup;
        private set
        {
            if (SetProperty(ref _isExportingBackup, value))
            {
                OnPropertyChanged(nameof(IsNotExportingBackup));
            }
        }
    }

    public bool IsNotExportingBackup => !IsExportingBackup;

    public ICommand LogoutCommand { get; }
    public ICommand ExportBackupCommand { get; }

    public SettingsPageViewModel(
        IAuthService authService,
        ISessionManager sessionManager,
        IBiometricService biometricService,
        IGymTronApiClient apiClient,
        IShare? share = null)
    {
        _authService = authService;
        _sessionManager = sessionManager;
        _biometricService = biometricService;
        _apiClient = apiClient;
        _share = share ?? Share.Default;

        LocalizationService.Initialize();
        _cultures = LocalizationService.GetSupportedCultures();
        LanguageNames = _cultures.Select(LocalizationService.GetLanguageDisplayName).ToList();

        int currentIndex = _cultures.FindIndex(c => c.Name == LocalizationService.CurrentCulture.Name);
        _selectedLanguageIndex = currentIndex >= 0 ? currentIndex : 0;

        LocalizationService.CultureChanged += OnCultureChanged;

        LogoutCommand = new Command(async () => await ExecuteLogoutAsync());
        ExportBackupCommand = new Command(async () => await ExecuteExportBackupAsync());

        _ = CheckBiometricSupportAsync();
    }

    private async Task CheckBiometricSupportAsync()
    {
        IsBiometricsSupported = await _biometricService.IsBiometricAvailableAsync();
    }

    private async Task HandleBiometricToggledAsync(bool enable)
    {
        _isTogglingBiometrics = true;
        try
        {
            if (!enable)
            {
                await _sessionManager.DisableBiometricsAsync();
                OnPropertyChanged(nameof(IsBiometricsEnabled));
                return;
            }

            string? password = await Shell.Current.DisplayPromptAsync(
                LocalizationService.GetString("Settings_Biometrics_Prompt_Title"),
                LocalizationService.GetString("Settings_Biometrics_Prompt_Message"),
                LocalizationService.GetString("Login_BiometricEnroll_Confirm"),
                LocalizationService.GetString("Login_Biometric_Cancel"),
                placeholder: LocalizationService.GetString("Login_Password"));

            if (string.IsNullOrWhiteSpace(password))
            {
                OnPropertyChanged(nameof(IsBiometricsEnabled));
                return;
            }

            string username = _sessionManager.LastUsername;
            if (string.IsNullOrWhiteSpace(username))
            {
                username = await Shell.Current.DisplayPromptAsync(
                    LocalizationService.GetString("Login_Identifier"),
                    LocalizationService.GetString("Login_Identifier"),
                    LocalizationService.GetString("Login_BiometricEnroll_Confirm"),
                    LocalizationService.GetString("Login_Biometric_Cancel"));

                if (string.IsNullOrWhiteSpace(username))
                {
                    OnPropertyChanged(nameof(IsBiometricsEnabled));
                    return;
                }
            }

            bool valid = await _authService.LoginAsync(username.Trim(), password);
            if (!valid)
            {
                await Shell.Current.DisplayAlert(
                    LocalizationService.GetString("Settings_Biometrics"),
                    LocalizationService.GetString("Login_Error_InvalidCredentials"),
                    "OK");

                OnPropertyChanged(nameof(IsBiometricsEnabled));
                return;
            }

            await _sessionManager.EnableBiometricsAsync(username.Trim(), password);
            _sessionManager.HasEnrolledBiometricsPromptBeenShown = true;
            OnPropertyChanged(nameof(IsBiometricsEnabled));

            await Shell.Current.DisplayAlert(
                LocalizationService.GetString("Settings_Biometrics"),
                LocalizationService.GetString("Settings_Biometrics_Enabled"),
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(
                LocalizationService.GetString("Settings_Biometrics"),
                ex.Message,
                "OK");

            OnPropertyChanged(nameof(IsBiometricsEnabled));
        }
        finally
        {
            _isTogglingBiometrics = false;
        }
    }

    private async Task ExecuteLogoutAsync()
    {
        bool confirm = await Shell.Current.DisplayAlert(
            LocalizationService.GetString("Settings_Logout_ConfirmTitle"),
            LocalizationService.GetString("Settings_Logout_ConfirmMessage"),
            LocalizationService.GetString("Common_Yes"),
            LocalizationService.GetString("Common_No"));

        if (!confirm)
        {
            return;
        }

        try
        {
            await _authService.LogoutAsync();
        }
        catch
        {
            // Ignore failure on revoke during logout
        }

        if (Application.Current is App app)
        {
            app.SwitchToLogin();
        }
    }

    private async Task ExecuteExportBackupAsync()
    {
        if (IsExportingBackup)
        {
            return;
        }

        IsExportingBackup = true;
        try
        {
            string jsonContent = await _apiClient.GetBackupJsonAsync();
            string fileName = $"gymtron_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllTextAsync(filePath, jsonContent);

            await _share.RequestAsync(new ShareFileRequest
            {
                Title = LocalizationService.GetString("Settings_Backup"),
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            if (Shell.Current != null)
            {
                await Shell.Current.DisplayAlert(
                    LocalizationService.GetString("Settings_Backup"),
                    $"{LocalizationService.GetString("Settings_Backup_Error")}: {ex.Message}",
                    "OK");
            }
        }
        finally
        {
            IsExportingBackup = false;
        }
    }

    private void OnCultureChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(LanguageSectionTitle));
        OnPropertyChanged(nameof(LanguageTitle));
        OnPropertyChanged(nameof(SecuritySectionTitle));
        OnPropertyChanged(nameof(BiometricsTitle));
        OnPropertyChanged(nameof(BiometricsDesc));
        OnPropertyChanged(nameof(BiometricsNotSupportedText));
        OnPropertyChanged(nameof(BackupSectionTitle));
        OnPropertyChanged(nameof(BackupDesc));
        OnPropertyChanged(nameof(BackupButtonText));
        OnPropertyChanged(nameof(LogoutText));
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
