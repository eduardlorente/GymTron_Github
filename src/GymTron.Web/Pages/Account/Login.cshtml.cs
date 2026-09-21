using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using GymTron.Web.Services.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymTron.Web.Pages.Account;

[AllowAnonymous]
public class LoginModel(IAuthApiClient authApiClient) : PageModel
{
    private readonly IAuthApiClient _authApiClient = authApiClient;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        public string Identifier { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var authResult = await _authApiClient.LoginAsync(Input.Identifier, Input.Password, HttpContext?.RequestAborted ?? default);
        if (authResult == null || string.IsNullOrWhiteSpace(authResult.AccessToken))
        {
            ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
            return Page();
        }

        var claims = ExtractClaimsFromJwt(authResult.AccessToken);
        if (!claims.Any(c => c.Type == ClaimTypes.Name))
        {
            claims.Add(new Claim(ClaimTypes.Name, Input.Identifier));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };
        authProperties.StoreTokens(
        [
            new AuthenticationToken { Name = "access_token", Value = authResult.AccessToken },
            new AuthenticationToken { Name = "refresh_token", Value = authResult.RefreshToken }
        ]);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToPage("/Routines/Index");
    }

    private static List<Claim> ExtractClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var parts = jwt.Split('.');
        if (parts.Length < 2)
        {
            return claims;
        }

        string payload = parts[1];
        int remainder = payload.Length % 4;
        int paddingLength = (remainder == 0) ? 0 : (4 - remainder);
        payload = payload.PadRight(payload.Length + paddingLength, '=')
                         .Replace('-', '+')
                         .Replace('_', '/');
        var jsonBytes = Convert.FromBase64String(payload);
        using var document = JsonDocument.Parse(jsonBytes);

        foreach (var property in document.RootElement.EnumerateObject())
        {
            string claimType = property.Name switch
            {
                "nameid" or "sub" => ClaimTypes.NameIdentifier,
                "unique_name" or "name" => ClaimTypes.Name,
                "email" => ClaimTypes.Email,
                _ => property.Name
            };
            claims.Add(new Claim(claimType, property.Value.ToString()));
        }

        return claims;
    }
}
