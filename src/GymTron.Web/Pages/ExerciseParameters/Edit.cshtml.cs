using GymTron.Web.Services.Api;
using GymTron.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymTron.Web.Pages.ExerciseParameters;

public class EditModel(IGymTronWebApiClient apiClient) : PageModel
{
    private readonly IGymTronWebApiClient _apiClient = apiClient;

    [BindProperty]
    public ExerciseParameterEditViewModel Exercise { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var exercise = await _apiClient.GetExerciseParameterByIdAsync(id, HttpContext?.RequestAborted ?? default);

        if (exercise == null)
            return NotFound();

        Exercise = new ExerciseParameterEditViewModel
        {
            Id = exercise.Id,
            Name = exercise.Name,
            Description = exercise.Description,
            Pattern = exercise.Pattern,
            Type = exercise.Type,
            ReplaysInReserve = exercise.ReplaysInReserve
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
            return Page();

        try
        {
            await _apiClient.UpdateExerciseParameterAsync(id, new UpdateExerciseParameterRequest(
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
