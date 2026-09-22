using Maui.Biometric;
using Microsoft.Extensions.Logging;

namespace GymTron.App.Services.Biometrics;

public class BiometricService(IBiometricAuthentication? biometricAuth = null, ILogger<BiometricService>? logger = null) : IBiometricService
{
    private readonly IBiometricAuthentication _biometricAuth = biometricAuth ?? BiometricAuthentication.Current;
    private readonly ILogger<BiometricService>? _logger = logger;

    public async Task<bool> IsBiometricAvailableAsync(CancellationToken ct = default)
    {
        try
        {
            AvailabilityResult availability = await _biometricAuth.CheckAvailabilityAsync(Authenticator.Biometric, ct);
            return availability.IsAvailable;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to check biometric availability.");
            return false;
        }
    }

    public async Task<bool> AuthenticateAsync(string title, string reason, string cancelTitle, CancellationToken ct = default)
    {
        try
        {
            var request = new AuthenticationRequest(title, reason)
            {
                CancelTitle = cancelTitle,
                Authenticators = Authenticator.Biometric
            };

            AuthenticationResult result = await _biometricAuth.AuthenticateAsync(request, ct);
            return result.IsSuccessful;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during biometric authentication.");
            return false;
        }
    }
}
