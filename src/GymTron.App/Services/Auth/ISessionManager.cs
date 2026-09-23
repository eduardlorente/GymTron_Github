namespace GymTron.App.Services.Auth;

public enum AuthMethod
{
    None = 0,
    Credentials = 1,
    Biometric = 2
}

public interface ISessionManager
{
    TimeSpan SessionTimeout { get; set; }
    bool IsBiometricsEnabled { get; set; }
    AuthMethod LastAuthMethod { get; set; }
    bool HasEnrolledBiometricsPromptBeenShown { get; set; }
    string LastUsername { get; set; }

    bool IsSessionActive();
    Task<bool> IsSessionActiveAsync();
    void RecordUnlock(AuthMethod method);
    void Lock();
    Task<bool> CanUseBiometricAsync();
    Task InvalidateSessionAsync();

    Task EnableBiometricsAsync(string username, string password);
    Task DisableBiometricsAsync();
    Task<(string? Username, string? Password)> GetBiometricCredentialsAsync();
}
