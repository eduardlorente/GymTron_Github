namespace GymTron.App.Services.Biometrics;

public interface IBiometricService
{
    Task<bool> IsBiometricAvailableAsync(CancellationToken ct = default);
    Task<bool> AuthenticateAsync(string title, string reason, string cancelTitle, CancellationToken ct = default);
}
