using GymTron.Web.Services.Api;
using GymTron.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GymTron.Web.Pages.ExerciseParameters;

public class IndexModel(IGymTronWebApiClient apiClient) : PageModel
{
    private readonly IGymTronWebApiClient _apiClient = apiClient;

    public List<ExerciseParameterViewModel> Exercises { get; set; } = [];

    public async Task OnGetAsync()
    {
        var exercises = await _apiClient.GetExerciseParametersAsync(HttpContext?.RequestAborted ?? default);

        Exercises = exercises.Select(e => new ExerciseParameterViewModel
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            Pattern = e.Pattern,
            Type = e.Type,
            ReplaysInReserve = e.ReplaysInReserve
        }).ToList();
    }
}
