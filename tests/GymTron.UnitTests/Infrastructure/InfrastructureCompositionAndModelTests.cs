using GymTron.Domain.Common;
using GymTron.Domain.Entities;
using GymTron.Domain.Repositories;
using GymTron.Infrastructure.BackgroundJobs;
using GymTron.Infrastructure.Persistence;
using GymTron.Infrastructure.Persistence.DAL.Models;
using GymTron.Infrastructure.Persistence.DAL.MySQL;
using GymTron.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace GymTron.UnitTests.Infrastructure;

public class InfrastructureCompositionAndModelTests
{
    [Fact]
    public void AddInfrastructureServices_RegistersTransientRepositoriesAndInternalDalFactories()
    {
        ServiceCollection services = new();

        IServiceCollection returned = services.AddInfrastructureServices("Server=unused;");

        Assert.Same(services, returned);
        AssertRegistrations<ITrainingRepository, TrainingRepository>(services);
        AssertFactory<IRoutineRepository, CachedRoutineRepository>(services);
        AssertRegistrations<IExerciseRepository, ExerciseRepository>(services);
        AssertRegistrations<IBodyWeightRepository, BodyWeightRepository>(services);
        AssertRegistrations<ILogRepository, LogRepository>(services);
        AssertRegistrations<IExerciseParameterRepository, ExerciseParameterRepository>(services);
        AssertFactory<ITrainingDAL, TrainingDAL>(services);
        AssertFactory<IRoutineDAL, RoutineDAL>(services);
        AssertFactory<IExerciseDAL, ExerciseDAL>(services);
        AssertFactory<IBodyWeightDAL, BodyWeightDAL>(services);
        AssertFactory<ILogDAL, LogDAL>(services);
        AssertFactory<IExerciseParameterDAL, ExerciseParameterDAL>(services);
        AssertFactory<IRefreshTokenDAL, RefreshTokenDAL>(services);
        AssertScopedFactory<UnitOfWork, UnitOfWork>(services);
        AssertScopedFactory<IUnitOfWork, UnitOfWork>(services);
        AssertScopedFactory<IDbTransactionContext, UnitOfWork>(services);
        Assert.Contains(services, d => d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(RefreshTokenCleanupJob));
    }

    [Fact]
    public void DalModels_DefaultAndAssignedProperties_PreserveMappingContracts()
    {
        DateTime date = new(2026, 4, 5);
        BodyWeightDALModel weight = new() { Id = 1, Weight = 80, BodyFatPercentage = 15, CreatedOn = date };
        ExerciseDALModel exercise = new()
        {
            Id = 2, TrainingId = 3, ExerciseParametersId = 4, Name = "Row", Weight = 50,
            DurationInSeconds = 30, Repetitions = 8, CreatedOn = date, ObservationsCSV = "good"
        };
        ExerciseParameterDALModel parameter = new()
        {
            Id = 4, Name = "Row", Description = "Desc", Pattern = "Pattern", TypeId = 1, ReplaysInReserve = null
        };
        RoutineFullDetailsDTO routine = new()
        {
            RoutineId = 5, RoutineName = "Routine", RoutineItemId = 6, DayOfWeek = 2,
            ExerciseParametersId = 4, MinRestTimeInSeconds = 60, MaxRestTimeInSeconds = null,
            AlternatingSeries = true, Active = true, Position = 1, ExerciseName = "Row",
            Description = "Desc", Pattern = "Pattern", Series = 3, RepetitionsMin = 8,
            RepetitionsMax = 12, Duration = null, ReplaysInReserve = 2, TypeId = 1,
            LastWeight = 50, LastDuration = 30, LastRepetitions = 8, LastObservations = "good"
        };

        Assert.Equal((1, 80m, 15m, date), (weight.Id, weight.Weight, weight.BodyFatPercentage, weight.CreatedOn));
        Assert.Equal((2, 3, 4, "Row", 50m, 30, 8, date, "good"),
            (exercise.Id, exercise.TrainingId, exercise.ExerciseParametersId, exercise.Name, exercise.Weight,
             exercise.DurationInSeconds, exercise.Repetitions, exercise.CreatedOn, exercise.ObservationsCSV));
        Assert.Equal((4, "Row", "Desc", "Pattern", 1, null),
            (parameter.Id, parameter.Name, parameter.Description, parameter.Pattern, parameter.TypeId, parameter.ReplaysInReserve));
        Assert.Equal((5, "Routine", 6, 2, 4, 60, null, true, true, 1),
            (routine.RoutineId, routine.RoutineName, routine.RoutineItemId, routine.DayOfWeek,
             routine.ExerciseParametersId, routine.MinRestTimeInSeconds, routine.MaxRestTimeInSeconds,
             routine.AlternatingSeries, routine.Active, routine.Position));
        Assert.Equal(("Row", "Desc", "Pattern", 3, 8, 12, null, 2, 1, 50m, 30, 8, "good"),
            (routine.ExerciseName, routine.Description, routine.Pattern, routine.Series, routine.RepetitionsMin,
             routine.RepetitionsMax, routine.Duration, routine.ReplaysInReserve, routine.TypeId,
             routine.LastWeight, routine.LastDuration, routine.LastRepetitions, routine.LastObservations));
    }

    [Fact]
    public async Task LogRepository_DelegatesAdd()
    {
        ILogDAL dal = Substitute.For<ILogDAL>();
        LogRepository repository = new(dal);
        Log log = Log.New("details");

        await repository.Add(log);

        await dal.Received(1).Add(log);
    }

    private static void AssertRegistrations<TService, TImplementation>(IServiceCollection services)
    {
        ServiceDescriptor descriptor = Assert.Single(services, item => item.ServiceType == typeof(TService));
        Assert.Equal(typeof(TImplementation), descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
    }

    private static void AssertFactory<TService, TImplementation>(IServiceCollection services)
    {
        ServiceDescriptor descriptor = Assert.Single(services, item => item.ServiceType == typeof(TService));
        Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
        Assert.NotNull(descriptor.ImplementationFactory);
        using ServiceProvider provider = services.BuildServiceProvider();
        Assert.IsType<TImplementation>(descriptor.ImplementationFactory(provider));
    }

    private static void AssertScopedFactory<TService, TImplementation>(IServiceCollection services)
    {
        ServiceDescriptor descriptor = Assert.Single(services, item => item.ServiceType == typeof(TService));
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.NotNull(descriptor.ImplementationFactory);
        using ServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();
        Assert.IsType<TImplementation>(descriptor.ImplementationFactory(scope.ServiceProvider));
    }
}
