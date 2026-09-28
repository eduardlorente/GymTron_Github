using FluentValidation;
using GymTron.Domain.Exceptions;
using GymTron.Web.Extensions;
using GymTron.Web.Services.Api;
using GymTron.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymTron.Web.Pages.Users;

public class EditModel(IGymTronWebApiClient apiClient) : PageModel
{
    private readonly IGymTronWebApiClient _apiClient = apiClient;

    [BindProperty]
    public UserEditViewModel UserInput { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _apiClient.GetUserByIdAsync(id, HttpContext?.RequestAborted ?? default);
        if (user == null)
        {
            return NotFound();
        }

        UserInput = new UserEditViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            TypeId = user.TypeId,
            IsActive = user.IsActive
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _apiClient.UpdateUserAsync(id, new UpdateUserRequest(
                UserInput.Username,
                UserInput.Email,
                UserInput.TypeId,
                UserInput.IsActive,
                string.IsNullOrWhiteSpace(UserInput.NewPassword) ? null : UserInput.NewPassword
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
