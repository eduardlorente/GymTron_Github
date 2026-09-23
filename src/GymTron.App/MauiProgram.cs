using CommunityToolkit.Maui;
using GymTron.App.Pages;
using GymTron.App.Pages.Modals;
using GymTron.App.Services;
using GymTron.App.Services.Api;
using GymTron.App.Services.Api.Auth;
using GymTron.App.Services.Auth;
using GymTron.App.Services.Biometrics;
using GymTron.App.ViewModels.Pages;
using GymTron.App.ViewModels.Pages.Modals;
using Maui.Biometric;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace GymTron.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        MauiAppBuilder builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBiometricAuthentication()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        ConfigureConfiguration(builder);
        ConfigureServices(builder);

        return builder.Build();
    }

    private static void ConfigureConfiguration(MauiAppBuilder builder)
    {
#if DEBUG
        const string baseConfigFileName = "GymTron.App.Resources.Json.appsettings.Development.json";
#else
        const string baseConfigFileName = "GymTron.App.Resources.Json.appsettings.json";
#endif

        Assembly assembly = typeof(MauiProgram).Assembly;
        using Stream? baseConfigStream = assembly.GetManifestResourceStream(baseConfigFileName);

        var configBuilder = new ConfigurationBuilder();
        if (baseConfigStream != null)
        {
            configBuilder.AddJsonStream(baseConfigStream);
        }

        builder.Configuration.AddConfiguration(configBuilder.Build());
    }

    private static void ConfigureServices(MauiAppBuilder builder)
    {
        // Logging
        builder.Services.AddLogging(loggingBuilder =>
        {
#if DEBUG
            loggingBuilder.AddDebug();
#endif
        });

        // API Client & Auth
        string apiUrl = builder.Configuration["ApiUrl"]
            ?? throw new InvalidOperationException("ApiUrl is required in configuration.");

        // Token Storage & Authentication Handler
        builder.Services.AddSingleton<ITokenStorage, SecureTokenStorage>();
        builder.Services.AddTransient<AuthHttpMessageHandler>();

        // Unauthenticated Auth API Client (used for login, refresh, register without handler)
        builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiUrl);
        });

        // Protected Main API Client (intercepted by AuthHttpMessageHandler)
        builder.Services.AddHttpClient<IGymTronApiClient, GymTronApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiUrl);
        })
        .AddHttpMessageHandler<AuthHttpMessageHandler>()
        .AddStandardResilienceHandler(options =>
        {
            options.Retry.MaxRetryAttempts = 3;
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(35);
        });

        // App services
        builder.Services.AddSingleton<IBiometricService, BiometricService>();
        builder.Services.AddSingleton<ISessionManager, SessionManager>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<ITrainingService, TrainingService>();
        builder.Services.AddTransient<IRoutineService, RoutineService>();
        builder.Services.AddTransient<IExerciseService, ExerciseService>();
        builder.Services.AddTransient<IBodyWeightService, BodyWeightService>();
        builder.Services.AddSingleton<IShare>(Share.Default);

        // View models
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<StartTrainingPageViewModel>();
        builder.Services.AddTransient<CurrentTrainingPageViewModel>();
        builder.Services.AddTransient<ExerciseDetailPageViewModel>();
        builder.Services.AddTransient<TrainingsHistoryPageViewModel>();
        builder.Services.AddTransient<ExercisesHistoryPageViewModel>();
        builder.Services.AddTransient<BodyWeightsHistoryPageViewModel>();
        builder.Services.AddTransient<TrainingSummaryPageViewModel>();
        builder.Services.AddTransient<SettingsPageViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<StartTrainingPage>();
        builder.Services.AddTransient<CurrentTrainingPage>();
        builder.Services.AddTransient<ExerciseDetailPage>();
        builder.Services.AddTransient<TrainingsHistoryPage>();
        builder.Services.AddTransient<ExercisesHistoryPage>();
        builder.Services.AddTransient<BodyWeightsHistoryPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<TrainingSummaryPage>();
    }
}
