using GymTron.Web.Services.Api;
using GymTron.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymTron.Web.Pages.Users;

public class IndexModel(IGymTronWebApiClient apiClient) : PageModel
{
    private readonly IGymTronWebApiClient _apiClient = apiClient;

    public List<UserViewModel> Users { get; set; } = [];

    public async Task OnGetAsync()
    {
        var users = await _apiClient.GetUsersAsync(HttpContext?.RequestAborted ?? default);

        Users = users.Select(u => new UserViewModel
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            TypeId = u.TypeId,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        }).ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            await _apiClient.DeleteUserAsync(id, HttpContext?.RequestAborted ?? default);
            return RedirectToPage();
        }
        catch (GymTron.Domain.Exceptions.DomainException ex)
        {
            GymTron.Web.Extensions.ModelStateExtensions.AddDomainException(ModelState, ex);
            await OnGetAsync();
            return Page();
        }
    }
}
