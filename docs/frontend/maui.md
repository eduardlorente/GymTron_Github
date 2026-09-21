# MAUI Frontend

This guide describes the current MAUI frontend, `src/GymTron.App`. The deleted Tortuga project is not part of current frontend scope.

## Current pattern

- The main flow is page/XAML -> view model -> App service -> `IGymTronApiClient` (HTTP) -> `GymTron.Api`. App services consume API DTOs and map them into UI view models. `GymTron.App` has zero direct dependencies on internal domain, application, or persistence layers.
- View models use the repository's manual `INotifyPropertyChanged` base and `ICommand`/MAUI `Command`; they do not use CommunityToolkit.Mvvm generators. CommunityToolkit.Maui supplies controls, behaviors, and toasts.
- Most feature pages constructor-inject a view model and assign `BindingContext`. Modal flows are exceptions that manually construct view models and pages or resolve services.
- `ObservableCollection<T>` is common for bindable lists. Existing code usually clears and repopulates collections, while other UI/domain-view models replace collection values; neither strategy is a universal rule.
- XAML is binding-heavy and uses converters, shared resources, inline styling, and `EventToCommandBehavior`. No `x:DataType` compiled bindings were found.
- The active project targets Android and conditionally Windows on .NET 9. Residual platform folders do not establish additional supported targets.

- Localization is fully implemented using `LocalizationService` backed by `.resx` resource files (`AppResources.resx`, `.es.resx`, `.en.resx`) and dynamic date formatting via `DateTimeFormatInfo`.
- Safe background tasks and timer cancellations are handled through `TaskExtensions.SafeFireAndForget` to avoid unhandled exceptions.
- Automated mobile UI testing is formally discarded by ADR-0005 in favor of compiled bindings, typed API contracts, and headless unit/integration test gates.

- Modal flows (`AddBodyWeightModal`, `CompleteExerciseModal`) use constructor injection for their view models and pages; `App.Services` service locator calls have been eliminated and marked obsolete.
- Navigation utilizes Shell routes for primary flows (`GoToAsync`) and explicit modal push/pop with typed constructor dependencies for transient input dialogs.

## Observed inconsistency

- Data loading occurs across constructors, `OnAppearing`, and `EventToCommandBehavior`.

## Boundaries

Presentation composition and UI behavior belong here. Application request handling, Domain behavior, persistence, configuration security, and delivery rules remain owned by their focused canonical documents.
