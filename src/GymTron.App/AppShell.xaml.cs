using GymTron.App.Pages;
using GymTron.App.Services;

namespace GymTron.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(StartTrainingPage), typeof(StartTrainingPage));
        Routing.RegisterRoute(nameof(CurrentTrainingPage), typeof(CurrentTrainingPage));
        Routing.RegisterRoute(nameof(ExerciseDetailPage), typeof(ExerciseDetailPage));
        Routing.RegisterRoute(nameof(TrainingsHistoryPage), typeof(TrainingsHistoryPage));
        Routing.RegisterRoute(nameof(ExercisesHistoryPage), typeof(ExercisesHistoryPage));
        Routing.RegisterRoute(nameof(BodyWeightsHistoryPage), typeof(BodyWeightsHistoryPage));
        Routing.RegisterRoute(nameof(TrainingSummaryPage), typeof(TrainingSummaryPage));
    }

    public AppShell(ITrainingService _) : this()
    {
    }
}
