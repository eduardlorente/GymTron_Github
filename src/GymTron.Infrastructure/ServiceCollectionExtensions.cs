using GymTron.Domain.Common;
using GymTron.Domain.Repositories;
using GymTron.Domain.Services;
using GymTron.Infrastructure.BackgroundJobs;
using GymTron.Infrastructure.Persistence;
using GymTron.Infrastructure.Persistence.DAL.MySQL;
using GymTron.Infrastructure.Persistence.Repositories;
using GymTron.Infrastructure.Security;
using GymTron.Infrastructure.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{


    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, string connectionString)
    {
        var sanitizedConnectionString = MySqlConnectionStringHelper.Sanitize(connectionString);

        services.AddMemoryCache();

        // Clock
        services.AddSingleton<IClock, SystemClock>();

        // Unit of Work & Transaction Context
        services.AddScoped<UnitOfWork>(sp => new UnitOfWork(sanitizedConnectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<UnitOfWork>());
        services.AddScoped<IDbTransactionContext>(sp => sp.GetRequiredService<UnitOfWork>());

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
        services.AddTransient<IRoutineDAL>(sp => new RoutineDAL(sanitizedConnectionString));
        services.AddTransient<ITrainingDAL>(sp => new TrainingDAL(sanitizedConnectionString, sp.GetService<IDbTransactionContext>()));
        services.AddTransient<IExerciseDAL>(sp => new ExerciseDAL(sanitizedConnectionString, sp.GetService<IDbTransactionContext>()));
        services.AddTransient<IBodyWeightDAL>(sp => new BodyWeightDAL(sanitizedConnectionString));
        services.AddTransient<ILogDAL>(sp => new LogDAL(sanitizedConnectionString));
        services.AddTransient<IExerciseParameterDAL>(sp => new ExerciseParameterDAL(sanitizedConnectionString));
        services.AddTransient<IUserDAL>(sp => new UserDAL(sanitizedConnectionString));
        services.AddTransient<IRefreshTokenDAL>(sp => new RefreshTokenDAL(sanitizedConnectionString));

        // Hosted Services
        services.AddHostedService<RefreshTokenCleanupJob>();

        return services;
    }
}
