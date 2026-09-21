using System.Globalization;
using FluentValidation;
using GymTron.Application.Routines.Commands;
using GymTron.Domain.Enums;
using GymTron.Domain.Exceptions;
using GymTron.Web.Extensions;
using GymTron.Web.Services.Api;
using GymTron.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymTron.Web.Pages.Routines;

public class CreateModel(IGymTronWebApiClient apiClient) : PageModel
{
    private readonly IGymTronWebApiClient _apiClient = apiClient;

    [BindProperty]
    public RoutineEditViewModel Routine { get; set; } = new();

    public List<SelectListItem> ExerciseOptions { get; set; } = [];

    public Dictionary<int, ExerciseTypes> ExerciseTypes { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadExerciseOptions();
    }

    public async Task<IActionResult> OnPostAsync(string? addItem, int? removeIndex)
    {
        await LoadExerciseOptions();

        if (addItem == "true")
        {
            ModelState.Clear();

            int nextOrder = Routine.Items.Count > 0 ? Routine.Items.Max(i => i.Position) + 1 : 1;

            Routine.Items.Add(new RoutineItemEditViewModel
            {
                DayOfWeek = 1,
                Series = 3,
                RepetitionsMin = 8,
                RepetitionsMax = 12,
                Position = nextOrder,
                ExerciseParametersId = 0
            });
            return Page();
        }

        if (removeIndex.HasValue && removeIndex >= 0 && removeIndex < Routine.Items.Count)
        {
            ModelState.Clear();
            var removedDayOfWeek = Routine.Items[removeIndex.Value].DayOfWeek;
            Routine.Items.RemoveAt(removeIndex.Value);

            var exercisesInDay = Routine.Items.Where(i => i.DayOfWeek == removedDayOfWeek).OrderBy(i => i.Position).ToList();
            for (int i = 0; i < exercisesInDay.Count; i++)
            {
                exercisesInDay[i].Position = i + 1;
            }

            return Page();
        }

        if (!ModelState.IsValid)
            return Page();

        var items = Routine.Items.Select(i => new RoutineItemInput
        {
            DayOfWeek = i.DayOfWeek,
            ExerciseParametersId = i.ExerciseParametersId,
            Series = i.Series,
            RepetitionsMin = i.RepetitionsMin,
            RepetitionsMax = i.RepetitionsMax,
            Duration = i.Duration,
            MinRestTimeInSeconds = i.MinRestTimeInSeconds,
            MaxRestTimeInSeconds = i.MaxRestTimeInSeconds,
            AlternatingSeries = i.AlternatingSeries,
            Position = i.Position,
            Type = i.Type
        }).ToList();

        try
        {
            await _apiClient.CreateRoutineAsync(new CreateRoutineRequest(Routine.Name, items), HttpContext?.RequestAborted ?? default);
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            ModelState.AddValidationException(ex);
            return Page();
        }
        catch (DomainException ex)
        {
            ModelState.AddDomainException(ex);
            return Page();
        }
    }

    private async Task LoadExerciseOptions()
    {
        var exercises = await _apiClient.GetExerciseParametersAsync(HttpContext?.RequestAborted ?? default);
        ExerciseOptions = exercises.Select(e => new SelectListItem(e.Name, e.Id.ToString())).ToList();
        ExerciseTypes = exercises.ToDictionary(e => e.Id, e => e.Type);
    }

    public List<RoutineItemEditViewModel> GetExercisesForDay(int dayOfWeek)
    {
        return Routine.Items.Where(i => i.DayOfWeek == dayOfWeek)
                           .OrderBy(i => i.Position)
                           .ToList();
    }

    public string GetDayName(int dayOfWeek)
    {
        if (dayOfWeek is < 1 or > 7)
            return "Unknown";

        var dayEnum = (DayOfWeek)(dayOfWeek % 7);
        var name = CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(dayEnum);
        return string.IsNullOrEmpty(name) ? "Unknown" : char.ToUpper(name[0], CultureInfo.CurrentCulture) + name[1..];
    }
}
