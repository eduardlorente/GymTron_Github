using GymTron.Web.Services.Api;
using GymTron.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymTron.Web.Pages.Routines;

public class IndexModel(IGymTronWebApiClient apiClient) : PageModel
{
    private readonly IGymTronWebApiClient _apiClient = apiClient;

    public List<RoutineViewModel> Routines { get; set; } = [];

    public async Task OnGetAsync()
    {
        var routines = await _apiClient.GetRoutinesAsync(HttpContext?.RequestAborted ?? default);

        Routines = routines.Select(r => new RoutineViewModel
        {
            Id = r.Id,
            Name = r.Name,
            Items = r.Items.Select(i => new RoutineItemViewModel
            {
                Id = i.Id,
                DayOfWeek = i.DayOfWeek,
                ExerciseParametersId = i.ExerciseParametersId,
                ExerciseName = i.ExerciseName,
                Series = i.Series,
                RepetitionsMin = i.RepetitionsMin,
                RepetitionsMax = i.RepetitionsMax,
                MinRestTimeInSeconds = i.MinRestTimeInSeconds,
                MaxRestTimeInSeconds = i.MaxRestTimeInSeconds ?? 0,
                AlternatingSeries = i.AlternatingSeries
            }).ToList()
        }).ToList();
    }
}
