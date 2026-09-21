using GymTron.Web.Services.Api;
using GymTron.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymTron.Web.Pages.ExerciseParameters;

public class CreateModel(IGymTronWebApiClient apiClient) : PageModel
{
    private readonly IGymTronWebApiClient _apiClient = apiClient;

    [BindProperty]
    public ExerciseParameterEditViewModel Exercise { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _apiClient.CreateExerciseParameterAsync(new CreateExerciseParameterRequest(
                Exercise.Name,
                Exercise.Description,
                Exercise.Pattern,
                Exercise.Type,
                Exercise.ReplaysInReserve
            ), HttpContext?.RequestAborted ?? default);

            return RedirectToPage("Index");
        }
        catch (FluentValidation.ValidationException ex)
        {
            GymTron.Web.Extensions.ModelStateExtensions.AddValidationException(ModelState, ex);
            return Page();
        }
        catch (GymTron.Domain.Exceptions.DomainException ex)
        {
            GymTron.Web.Extensions.ModelStateExtensions.AddDomainException(ModelState, ex);
            return Page();
        }
    }
}
