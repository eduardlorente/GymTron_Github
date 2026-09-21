using FluentValidation;
using GymTron.Application.Base;
using GymTron.Application.DomainServices;
using GymTron.Application.Trainings.Commands;
using GymTron.Domain.Services;
using MediatR;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{


    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        //MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(StartTrainingCommand).Assembly));

        //FluentValidation
        services.AddValidatorsFromAssemblyContaining<StartTrainingCommand>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        //Services
        services.AddTransient(typeof(IExceptionLogger<>), typeof(ExceptionLogger<>));
        services.AddScoped<GymTron.Application.Common.Events.IDomainEventDispatcher, GymTron.Application.Common.Events.MediatRDomainEventDispatcher>();

        return services;
    }
}
