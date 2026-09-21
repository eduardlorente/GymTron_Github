using GymTron.Web.Services.Api;
using GymTron.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymTron.Web.Pages.Routines;

public class DetailsModel(IGymTronWebApiClient apiClient) : PageModel
{
    private readonly IGymTronWebApiClient _apiClient = apiClient;

    public RoutineViewModel Routine { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var routine = await _apiClient.GetRoutineByIdAsync(id, HttpContext?.RequestAborted ?? default);

        if (routine == null)
            return NotFound();

        Routine = new RoutineViewModel
        {
            Id = routine.Id,
            Name = routine.Name,
            Items = routine.Items.Select(i => new RoutineItemViewModel
            {
                Id = i.Id,
                DayOfWeek = i.DayOfWeek,
                ExerciseParametersId = i.ExerciseParametersId,
                ExerciseName = i.ExerciseName,
                Series = i.Series,
                RepetitionsMin = i.RepetitionsMin,
                RepetitionsMax = i.RepetitionsMax,
                MinRestTimeInSeconds = i.MinRestTimeInSeconds,
                MaxRestTimeInSeconds = i.MaxRestTimeInSeconds,
                AlternatingSeries = i.AlternatingSeries
            }).ToList()
        };

        return Page();
    }
}
