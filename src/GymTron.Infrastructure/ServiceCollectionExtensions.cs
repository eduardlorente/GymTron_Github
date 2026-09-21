using GymTron.Domain.Repositories;
using GymTron.Domain.Services;
using GymTron.Infrastructure.Persistence.DAL.MySQL;
using GymTron.Infrastructure.Persistence.Repositories;
using GymTron.Infrastructure.Security;
using GymTron.Infrastructure.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{


    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
    {
        services.AddMemoryCache();

        // Clock
        services.AddSingleton<IClock, SystemClock>();

        // Repositories
        services.AddTransient<ITrainingRepository, TrainingRepository>();
        services.AddTransient<RoutineRepository>();
        services.AddTransient<IRoutineRepository>(sp =>
            new CachedRoutineRepository(
                sp.GetRequiredService<RoutineRepository>(),
                sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>()));
        services.AddTransient<IExerciseRepository, ExerciseRepository>();
        services.AddTransient<IBodyWeightRepository, BodyWeightRepository>();
        services.AddTransient<ILogRepository, LogRepository>();
        services.AddTransient<IExerciseParameterRepository, ExerciseParameterRepository>();


        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IRefreshTokenRepository, RefreshTokenRepository>();

        // Security services
        services.AddSingleton<IPasswordHasher, PasswordHasherService>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        // DALs
        services.AddTransient<IRoutineDAL>(sp => new RoutineDAL(connectionString));
        services.AddTransient<ITrainingDAL>(sp => new TrainingDAL(connectionString));
        services.AddTransient<IExerciseDAL>(sp => new ExerciseDAL(connectionString));
        services.AddTransient<IBodyWeightDAL>(sp => new BodyWeightDAL(connectionString));
        services.AddTransient<ILogDAL>(sp => new LogDAL(connectionString));
        services.AddTransient<IExerciseParameterDAL>(sp => new ExerciseParameterDAL(connectionString));
        services.AddTransient<IUserDAL>(sp => new UserDAL(connectionString));
        services.AddTransient<IRefreshTokenDAL>(sp => new RefreshTokenDAL(connectionString));

        return services;
    }
}
