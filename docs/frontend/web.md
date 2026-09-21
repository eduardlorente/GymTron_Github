# Web Frontend

This guide describes `src/GymTron.Web` presentation behavior.

## Current pattern

- Razor Pages are grouped by feature, including Routines and ExerciseParameters.
- Feature `PageModel` classes commonly use primary-constructor injection of `IMediator` and dispatch Application commands or queries from page handlers.
- Editable pages expose `[BindProperty]` Web-specific view models, use DataAnnotations and `ModelState`, and map between request results and presentation models.
- The Web project is a composition root: it registers Application and Infrastructure but feature pages communicate through MediatR rather than directly using persistence adapters.
- Localization is configured in `Program.cs` via `AddLocalization`, `AddViewLocalization`, `AddDataAnnotationsLocalization`, and `RequestLocalizationOptions` supporting Catalan (`ca`, default), Spanish (`es`), and English (`en`). Feature resources are organized under `Resources/Pages/<Feature>/<Page>.<culture>.resx`.
- Automated testing for `PageModel` classes is provided by `tests/GymTron.Web.Tests` using xUnit and NSubstitute.

- Form commands across all editable pages (`ExerciseParameters` and `Routines`) handle errors uniformly: MediatR dispatches are wrapped to catch `ValidationException` (mapped to field-level `ModelState` errors) and `DomainException` (mapped to model-level `ModelState` summary errors) via `ModelStateExtensions`.

## Observed inconsistency

None currently observed.

## Boundaries

Razor presentation, page binding, and Web view models belong here. Handler validation and response contracts belong to Application architecture; persistence and delivery belong in their canonical documents.
