namespace GymTron.App.Services.Auth;

public interface IAuthService
{
    event EventHandler? SessionExpired;
    Task<bool> LoginAsync(string identifier, string password, CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
    Task<bool> IsAuthenticatedAsync();
    Task<bool> RegisterAsync(string username, string email, string password, CancellationToken ct = default);
}
