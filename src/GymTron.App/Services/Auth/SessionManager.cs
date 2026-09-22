using System.Globalization;
using GymTron.App.Services.Biometrics;
using Microsoft.Extensions.Logging;

namespace GymTron.App.Services.Auth;

public class SessionManager(
    ITokenStorage tokenStorage,
    IBiometricService biometricService,
    ILogger<SessionManager>? logger = null) : ISessionManager
{
    private const string BiometricsEnabledKey = "gymtron_biometrics_enabled";
    private const string LastAuthMethodKey = "gymtron_last_auth_method";
    private const string LastUnlockUtcKey = "gymtron_last_unlock_utc";
    private const string BiometricsPromptShownKey = "gymtron_biometrics_prompt_shown";

    private readonly ITokenStorage _tokenStorage = tokenStorage;
    private readonly IBiometricService _biometricService = biometricService;
    private readonly ILogger<SessionManager>? _logger = logger;

    public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromHours(12);

    public bool IsBiometricsEnabled
    {
        get => Preferences.Get(BiometricsEnabledKey, false);
        set => Preferences.Set(BiometricsEnabledKey, value);
    }

    public AuthMethod LastAuthMethod
    {
        get
        {
            string saved = Preferences.Get(LastAuthMethodKey, nameof(AuthMethod.None));
            return Enum.TryParse(saved, out AuthMethod method) ? method : AuthMethod.None;
        }
        set => Preferences.Set(LastAuthMethodKey, value.ToString());
    }

    public bool HasEnrolledBiometricsPromptBeenShown
    {
        get => Preferences.Get(BiometricsPromptShownKey, false);
        set => Preferences.Set(BiometricsPromptShownKey, value);
    }

    public bool IsSessionActive()
    {
        string lastUnlockStr = Preferences.Get(LastUnlockUtcKey, string.Empty);
        if (string.IsNullOrEmpty(lastUnlockStr))
        {
            return false;
        }

        if (!DateTime.TryParse(lastUnlockStr, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime lastUnlockUtc))
        {
            return false;
        }

        TimeSpan elapsed = DateTime.UtcNow - lastUnlockUtc;
        bool isActive = elapsed < SessionTimeout;

        _logger?.LogDebug("Session check: elapsed {Elapsed}, timeout {Timeout}, isActive: {IsActive}",
            elapsed, SessionTimeout, isActive);

        return isActive;
    }

    public async Task<bool> IsSessionActiveAsync()
    {
        if (!IsSessionActive())
        {
            return false;
        }

        string? refreshToken = await _tokenStorage.GetRefreshTokenAsync();
        string? accessToken = await _tokenStorage.GetAccessTokenAsync();
        bool hasTokens = !string.IsNullOrWhiteSpace(refreshToken) || !string.IsNullOrWhiteSpace(accessToken);

        if (!hasTokens)
        {
            Lock();
            return false;
        }

        return true;
    }

    public void RecordUnlock(AuthMethod method)
    {
        string nowUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        Preferences.Set(LastUnlockUtcKey, nowUtc);
        LastAuthMethod = method;

        _logger?.LogInformation("Session unlocked via {Method} at {Timestamp}", method, nowUtc);
    }

    public void Lock()
    {
        Preferences.Remove(LastUnlockUtcKey);
        _logger?.LogInformation("Session locked.");
    }

    public async Task<bool> CanUseBiometricAsync()
    {
        if (!IsBiometricsEnabled)
        {
            return false;
        }

        return await _biometricService.IsBiometricAvailableAsync();
    }

    public async Task InvalidateSessionAsync()
    {
        Lock();
        await _tokenStorage.ClearTokensAsync();
        _logger?.LogInformation("Session invalidated and tokens cleared.");
    }
}
