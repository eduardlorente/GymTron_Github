using FluentValidation;
using GymTron.Domain.Exceptions;
using GymTron.Web.Extensions;
using GymTron.Web.Services.Api;
using GymTron.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymTron.Web.Pages.Users;

public class CreateModel(IGymTronWebApiClient apiClient) : PageModel
{
    private readonly IGymTronWebApiClient _apiClient = apiClient;

    [BindProperty]
    public UserCreateViewModel UserInput { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _apiClient.CreateUserAsync(new CreateUserRequest(
                UserInput.Username,
                UserInput.Email,
                UserInput.Password,
                UserInput.TypeId
            ), HttpContext?.RequestAborted ?? default);

            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            ModelStateExtensions.AddValidationException(ModelState, ex);
            return Page();
        }
        catch (DomainException ex)
        {
            ModelStateExtensions.AddDomainException(ModelState, ex);
            return Page();
        }
    }
}
